using namespace System.Threading.Tasks
using namespace System.Collections.Generic
using namespace System.Net.NetworkInformation
using namespace System.Net
using namespace System.Diagnostics

function Test-ICMPConnectionAsync {
    [cmdletbinding()]
    param(
        [parameter(Mandatory, ValueFromPipeline, Position = 0)]
        [string[]] $Address,

        [parameter(Position = 1)]
        [ValidateRange(1, [int]::MaxValue)]
        [int] $TimeOut = 10, # In seconds!

        [parameter(Position = 2)]
        [ValidateRange(1, [int]::MaxValue)]
        [int] $BufferSize = 32
    )

    begin {
        $tasks   = [List[hashtable]]::new()
        $timer   = [Stopwatch]::StartNew()
        $data    = [byte[]] [char[]] 'A' * $BufferSize
        $options = [PingOptions]::new()
        $TimeOut = [timespan]::FromSeconds($TimeOut).TotalMilliseconds
        $options.DontFragment = $true

        $outObject = {
            [pscustomobject]@{
                Source     = $env:COMPUTERNAME
                Target     = $target
                Address    = $response.Address.IPAddressToString
                DnsName    = $dnsresol
                Latency    = $latency
                Status     = $response.Status
            }
        }
    }
    process {
        foreach($addr in $Address) {
            $ping = [Ping]::new()
            $tasks.Add(@{
                Target   = $addr
                Instance = $ping
                PingTask = $ping.SendPingAsync($addr, $TimeOut, $data, $options)
                DnsTask  = [Dns]::GetHostEntryAsync($addr)
            })
        }
    }
    end {
        while($tasks -and $timer.ElapsedMilliseconds -le $timeout) {
            $id = [Task]::WaitAny($tasks.PingTask, 200)
            if($id -eq -1) {
                continue
            }
            $target, $instance, $ping, $dns = $tasks[$id]['Target', 'Instance', 'PingTask', 'DnsTask']

            try {
                $response = $ping.GetAwaiter().GetResult()
                $latency  = [string]::Format('{0} ms', $response.RoundtripTime)
                $ping.Dispose()
            }
            catch {
                $latency  = '*'
                $response = @{
                    Address = @{ IPAddressToString = '*' }
                    Status  = $_.Exception.InnerException.InnerException.Message
                }
            }

            try {
                $dnsresol = $dns.GetAwaiter().GetResult().HostName
                $dns.Dispose()
            }
            catch {
                $dnsresol = $_.Exception.InnerException.Message
            }

            & $outObject
            $instance.Dispose()
            $tasks.RemoveAt($id)
        }

        foreach($task in $tasks) {
            $target, $instance, $ping, $dns = $task['Target', 'Instance', 'PingTask', 'DnsTask']

            try {
                $response = $ping.GetAwaiter().GetResult()
                $latency  = [string]::Format('{0} ms', $response.RoundtripTime)
                $ping.Dispose()
            }
            catch {
                $latency  = '*'
                $response = @{
                    Address = '*'
                    Status  = $_.Exception.InnerException.InnerException.Message
                }
            }

            try {
                $dnsresol = $dns.GetAwaiter().GetResult().HostName
                $dns.Dispose()
            }
            catch {
                $dnsresol = $_.Exception.InnerException.Message
            }

            & $outObject
            $instance.Dispose()
        }
    }
}

'amazon.com', 'google.com', 'facebook.com' |
    Test-ICMPConnectionAsync -TimeOut 5 |
    Format-Table -AutoSize