﻿using namespace System.IO

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
            $result | Out-Host
        }

        It 'Source' {
            $result.Source | Should -Not -BeNullOrEmpty
            $result.Source | Should -BeOfType ([string])
        }

        It 'Destination' {
            $result.Destination | Should -Not -BeNullOrEmpty
            $result.Destination | Should -BeOfType ([string])
        }

        It 'Port' {
            $result.Port | Should -Not -BeNullOrEmpty
            $result.Port | Should -BeOfType ([int])
        }

        It 'Status' {
            $result.Status | Should -BeExactly ([PSNetScanners.TcpStatus]::Success)
        }

        It 'Details' {
            $result = Test-TcpAsync -Target 192.1.1.1 -Port 80
            $result.Details | Should -BeOfType ([System.Net.Sockets.SocketException])
        }
    }

    Context 'Test-TcpAsync' {
        It 'Parallel Tcp Tests' {
            $timeout = 30
            if ($IsLinux) {
                # seems to be much slower in linux
                $timeout = 150
            }

            Measure-Command { $targets | Test-TcpAsync } |
                ForEach-Object TotalSeconds |
                Should -BeLessOrEqual $timeout
        }

        It 'Stops processing early' {
            Measure-Command { $targets | Test-TcpAsync | Select-Object -First 5 } |
                ForEach-Object TotalSeconds |
                Should -BeLessOrEqual 1
        }
    }

    Context 'Parameters' {
        It 'ThrottleLimit' {
            $result = $targets | Test-TcpAsync -ThrottleLimit 3
            $result | Should -HaveCount $targets.Count
            $result.Status | Should -Contain ([PSNetScanners.DnsStatus]::Success)
            $result.Status | Should -Contain ([PSNetScanners.DnsStatus]::Error)
        }

        It 'ConnectionTimeout' {
            $result = $targets | Test-TcpAsync -ThrottleLimit $targets.Count -ConnectionTimeout 200
            $result | Should -HaveCount $targets.Count
            $result.Status | Should -Contain ([PSNetScanners.DnsStatus]::Success)
            $result.Status | Should -Contain ([PSNetScanners.DnsStatus]::Timeout)
            $result |
                Where-Object Status -EQ Timeout |
                ForEach-Object Details |
                Should -BeOfType ([System.TimeoutException])
        }
    }
}