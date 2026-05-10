using namespace System.IO
using namespace System.Net.Sockets

Import-Module ([Path]::Combine($PSScriptRoot, 'common.psm1'))
Import-Module $manifestPath

Describe TestTcpAsyncCommand {
    Context 'Output Streams' {
        It 'Success' {
            Test-TcpAsync -Target google.com -Port 80 |
                Should -BeOfType ([PSNetScanners.Tcp.TcpResult])
        }
    }

    Context 'TcpResult Type' {
        BeforeAll {
            $result = Test-TcpAsync -Target google.com -Port 80
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

        It 'Success' {
            $result.Success | Should -BeOfType ([bool])
        }

        It 'Error' {
            $result = Test-TcpAsync -Target google.com -Port 8080 -ConnectionTimeout ([int]::MaxValue)
            $result.Error | Should -BeOfType ([SocketException])
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

        It 'Should be able to Cancel the cmdlet' {
            $testCmdletCancellationSplat = @{
                Script          = '$input | Test-TcpAsync -ConnectionTimeout ([int]::MaxValue)'
                ModulePath      = $manifestPath
                InvocationInput = $targets
            }
            Test-CmdletCancellation @testCmdletCancellationSplat | Should -BeLessThan ([timespan] '00:00:02')
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
                ForEach-Object Error |
                Should -BeOfType ([SocketException])
        }
    }

    Context 'Formatting' {
        BeforeAll {
            $tcp = Test-TcpAsync google.com 80
            $tcp | Out-Null
        }

        It 'Gets IPEndpoint IPAddress' {
            [PSNetScanners.Internal._Format]::GetClient($tcp) | Should -Not -BeNullOrEmpty
        }
    }
}
