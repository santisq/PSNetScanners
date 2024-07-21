using namespace System.IO

$moduleName = (Get-Item ([Path]::Combine($PSScriptRoot, '..', 'module', '*.psd1'))).BaseName
$manifestPath = [Path]::Combine($PSScriptRoot, '..', 'output', $moduleName)

Import-Module $manifestPath
Import-Module ([Path]::Combine($PSScriptRoot, 'common.psm1'))

Describe TestPingAsyncCommand {
    Context 'Output Streams' {
        It 'Success' {
            Test-PingAsync -Target github.com |
                Should -BeOfType ([PSNetScanners.PingResult])
        }

        It 'Error' {
            { Test-PingAsync -Target "$([guid]::NewGuid()).com" -ErrorAction Stop } |
                Should -Throw -ExceptionType ([System.Net.Sockets.SocketException])
        }
    }

    Context 'DnsResult Type' {
        It 'DnsSuccess' {
            $result = Test-PingAsync google.com -ResolveDns
            $result.DnsResult | Should -BeOfType ([PSNetScanners.DnsSuccess])
            $result.DnsResult.Status | Should -Be ([PSNetScanners.DnsStatus]::Success)
        }

        It 'DnsFailure' {
            $result = makeiprange 10.0.0 1 255 |
                Test-PingAsync -ResolveDns |
                Where-Object { $_.DnsResult.Status -eq [PSNetScanners.DnsStatus]::Error } |
                Select-Object -First 1
            $result.DnsResult | Should -BeOfType ([PSNetScanners.DnsFailure])
            $result.DnsResult.Status | Should -Be ([PSNetScanners.DnsStatus]::Error)
        }
    }

    Context 'PingResult Type' {
        BeforeAll {
            $ping = Test-PingAsync google.com
            $ping | Out-Null
        }

        It 'Source' {
            $ping.Source | Should -Not -BeNullOrEmpty
            $ping.Source | Should -BeOfType ([string])
        }

        It 'Destination' {
            $ping.Destination | Should -Not -BeNullOrEmpty
            $ping.Destination | Should -BeOfType ([string])
        }

        It 'DisplayAddress' {
            $ping.DisplayAddress | Should -Not -BeNullOrEmpty
            $ping.DisplayAddress | Should -BeOfType ([string])
        }

        It 'Status' {
            $ping.Status | Should -BeOfType ([System.Net.NetworkInformation.IPStatus])
        }

        It 'Address' {
            $ping.Address | Should -BeOfType ([ipaddress])
        }

        It 'Latency' {
            $ping.Latency | Should -BeOfType ([long])
        }
    }

    Context 'Test-PingAsync' {
        BeforeAll {
            $range = makeiprange 127.0.0 1 255
            $range | Out-Null
        }

        It 'Parallel Pings' {
            Measure-Command { $range | Test-PingAsync } |
                ForEach-Object TotalMinutes |
                Should -BeLessThan 2
        }

        It 'Stops processing early' {
            Measure-Command { $range | Test-PingAsync | Select-Object -First 10 } |
                ForEach-Object TotalSeconds |
                Should -BeLessThan 10
        }
    }

    Context 'Parameters' {
        BeforeAll {
            $range = makeiprange 127.0.0 1 20
            $range | Out-Null
        }

        It 'ConnectionTimeout' {
            $range | Test-PingAsync -ConnectionTimeout 200 -ErrorAction Stop |
                Should -HaveCount 20

            $range | Test-PingAsync -ConnectionTimeout 200 -ResolveDns -ErrorAction Stop |
                Should -HaveCount 20
        }

        It 'ThrottleLimit' {
            $targets.Target | Sort-Object -Unique | Test-PingAsync -ThrottleLimit 1 |
                Should -HaveCount 4

            $range | Test-PingAsync -ThrottleLimit 300 -ErrorAction Stop |
                Should -HaveCount 20
        }

        It 'BufferSize' {
            $range | Test-PingAsync -BufferSize 1 -ErrorAction Stop |
                Should -HaveCount 20
        }

        It 'Ttl' {
            $range | Test-PingAsync -Ttl 1 -ErrorAction Stop |
                Should -HaveCount 20
        }

        It 'DontFragment' {
            $range | Test-PingAsync -DontFragment -ErrorAction Stop |
                Should -HaveCount 20
        }
    }

    Context 'Formatting' {
        BeforeAll {
            $ping = Test-PingAsync google.com
            $ping | Out-Null
        }

        It 'Format Latency' {
            [PSNetScanners.Internal._Format]::FormatLatency($ping) |
                Should -Not -BeNullOrEmpty
        }
    }
}
