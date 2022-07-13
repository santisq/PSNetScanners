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
        [int] $TimeOut = 1000,

        [parameter(Position = 2)]
        [int] $BufferSize = 32
    )

    begin {
        $tasks = [List[OrderedDictionary]]::new()
        $data  = [byte[]]::new($BufferSize)
    }
    process {
        foreach($addr in $Address) {
            $ping     = [Ping]::new()
            $options  = [PingOptions]::new()
            $options.DontFragment = $true
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
            else { $dnsresol = '*' }

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