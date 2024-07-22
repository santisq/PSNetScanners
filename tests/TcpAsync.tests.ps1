using namespace System.IO

$moduleName = (Get-Item ([Path]::Combine($PSScriptRoot, '..', 'module', '*.psd1'))).BaseName
$manifestPath = [Path]::Combine($PSScriptRoot, '..', 'output', $moduleName)

Import-Module $manifestPath
Import-Module ([Path]::Combine($PSScriptRoot, 'common.psm1'))

Describe TestPingAsyncCommand {
    Context 'Output Streams' {
        It 'Success' {
            Test-TcpAsync -Target github.com -Port 80 |
                Should -BeOfType ([PSNetScanners.TcpResult])
        }
    }

    Context 'TcpResult Type' {
        BeforeAll {
            $result = Test-TcpAsync -Target github.com -Port 80
            $result | Out-Null
        }

        It 'Source' {
            $result.Source | Should -Not -BeNullOrEmpty
            $result.Source | Should -BeOfType ([string])
        }

        It 'Destination' {
            $result.Destination | Should -Not -BeNullOrEmpty
            $result.Destination | Should -BeOfType ([string])
        }

        It 'Client' {
            $result.Client | Should -Not -BeNullOrEmpty
            $result.Client | Should -BeOfType ([IPEndpoint])
        }

        It 'Port' {
            $result.Port | Should -Not -BeNullOrEmpty
            $result.Port | Should -BeOfType ([int])
        }

        It 'Status' {
            $result.Status | Should -BeExactly ([PSNetScanners.TcpStatus]::Opened)
        }

        It 'Details' {
            $result = Test-TcpAsync -Target github.com -Port 8080 -ConnectionTimeout ([int]::MaxValue)
            $result.Details | Should -BeOfType ([System.Net.Sockets.SocketException])
        }
    }

    Context 'Test-TcpAsync' {
        It 'Parallel Tcp Tests' {
            Measure-Command { $targets | Test-TcpAsync } |
                ForEach-Object TotalSeconds |
                Should -BeLessOrEqual 150
        }

        It 'Stops processing early' {
            Measure-Command { $targets | Test-TcpAsync | Select-Object -First 5 } |
                ForEach-Object TotalSeconds |
                Should -BeLessOrEqual 1
        }
    }

    Context 'Parameters' {
        It 'ThrottleLimit' {
            $result = $targets | Select-Object -First 9 |
                Test-TcpAsync -ThrottleLimit 1 -ConnectionTimeout ([int]::MaxValue)
            $result | Should -HaveCount 9
            $result.Status | Should -Contain ([PSNetScanners.TcpStatus]::Opened)
            $result.Status | Should -Contain ([PSNetScanners.TcpStatus]::Closed)
        }

        It 'ConnectionTimeout' {
            $result = $targets | Test-TcpAsync -ThrottleLimit $targets.Count -ConnectionTimeout 200
            $result | Should -HaveCount $targets.Count
            $result.Status | Should -Contain ([PSNetScanners.TcpStatus]::Opened)
            $result.Status | Should -Contain ([PSNetScanners.TcpStatus]::TimedOut)
            $result |
                Where-Object Status -EQ TimedOut |
                ForEach-Object Details |
                Should -BeOfType ([System.TimeoutException])
        }
    }
}
