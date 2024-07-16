using namespace System.IO

$moduleName = (Get-Item ([Path]::Combine($PSScriptRoot, '..', 'module', '*.psd1'))).BaseName
$manifestPath = [Path]::Combine($PSScriptRoot, '..', 'output', $moduleName)

Import-Module $manifestPath
Import-Module ([Path]::Combine($PSScriptRoot, 'common.psm1'))

Describe TestConnectionAsyncCommand {
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
    }
}
