using namespace System.IO

$moduleName = (Get-Item ([Path]::Combine($PSScriptRoot, '..', 'module', '*.psd1'))).BaseName
$manifestPath = [Path]::Combine($PSScriptRoot, '..', 'output', $moduleName)

Import-Module $manifestPath
Import-Module ([Path]::Combine($PSScriptRoot, 'common.psm1'))

Describe TestPingAsyncCommand {
    Context 'Output Streams' {
        It 'Success' {
            Test-ConnectionAsync -Target github.com |
                Should -BeOfType ([PSNetScanners.PingResult])
        }
        It 'Error' {
            { Test-ConnectionAsync -Target doesNotExist.com -ErrorAction Stop } |
                Should -Throw

            { Test-ConnectionAsync -Target noSuchAddress -ErrorAction Stop } |
                Should -Throw
        }
    }
    Context 'DnsResult Type' {
        It 'DnsSuccess' {
            $result = Test-ConnectionAsync google.com
            $result.DnsResult | Should -BeOfType ([PSNetScanners.DnsSuccess])
            $result.DnsResult.Status | Should -Be ([PSNetScanners.DnsStatus]::Success)
        }
        It 'DnsFailure' {
            $result = Test-ConnectionAsync 127.0.0.2
            $result.DnsResult | Should -BeOfType ([PSNetScanners.DnsFailure])
            $result.DnsResult.Status | Should -Be ([PSNetScanners.DnsStatus]::Error)
        }
    }
    Context 'Test-PingAsync' {
        It 'Parallel Pings' {
            Measure-Command {
                1..254 | ForEach-Object { "127.0.0.$_" } | Test-ConnectionAsync
            } | ForEach-Object TotalMinutes | Should -BeLessThan 2
        }
        It 'Stops processing early' {
            Measure-Command {
                1..254 | ForEach-Object { "127.0.0.$_" } |
                    Test-ConnectionAsync |
                    Select-Object -First 10
            } | ForEach-Object TotalSeconds | Should -BeLessThan 10
        }
    }
    Context 'Parameters' {
        It 'TaskTimeoutMilliseconds' {
            { 1..254 | ForEach-Object { "127.0.0.$_" } |
                Test-ConnectionAsync -TaskTimeoutMilliseconds 200 -ErrorAction Stop } |
                Should -Not -Throw
        }
        It 'ThrottleLimit' {
            { 1..254 | ForEach-Object { "127.0.0.$_" } |
                Test-ConnectionAsync -ThrottleLimit 300 -ErrorAction Stop } |
                Should -Not -Throw
        }
        It 'BufferSize' {
            { 1..254 | ForEach-Object { "127.0.0.$_" } |
                Test-ConnectionAsync -BufferSize 1 -ErrorAction Stop } |
                Should -Not -Throw
        }
    }
}
