using namespace System.Threading.Tasks
using namespace System.Collections.Generic
using namespace System.Net.NetworkInformation
using namespace System.Net
using namespace System.Collections.Specialized

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
        $tasks   = [List[OrderedDictionary]]::new()
        $data    = [byte[]]::new($BufferSize)
        $options = [PingOptions]::new()
        $TimeOut = [timespan]::FromSeconds($TimeOut).TotalMilliseconds
        $options.DontFragment = $true
    }
    process {
        foreach($addr in $Address) {
            $ping = [Ping]::new()
            $tasks.Add([ordered]@{
                Target   = $addr
                Instance = $ping
                PingTask = $ping.SendPingAsync($addr, $TimeOut, $data, $options)
                DnsTask  = [Dns]::GetHostEntryAsync($addr)
            })
        }
    }
    end {
        $null = [Task]::WaitAll($tasks.PingTask)
        foreach($task in $tasks) {
            $target, $instance, $ping, $dns = $task[$task.PSBase.Keys]
            $instance.ForEach('Dispose')
            $response = $ping.GetAwaiter().GetResult()

            if($response.Status -eq 'Success') {
                $latency = [string]::Format('{0} ms', $response.RoundtripTime)
            }
            else {
                $latency = '*'
            }

            if($dns.Status -eq 'RanToCompletion') {
                $dnsresol = $dns.GetAwaiter().GetResult().HostName
            }
            else {
                $dnsresol = '*'
            }

            [pscustomobject]@{
                Source     = $env:COMPUTERNAME
                Target     = $target
                Address    = $response.Address
                DnsName    = $dnsresol
                Latency    = $latency
                BufferSize = $data.Length
                Status     = $response.Status
            }
        }
    }
}

'amazon.com', 'google.com', 'facebook.com' | Test-ICMPConnectionAsync | Format-Table