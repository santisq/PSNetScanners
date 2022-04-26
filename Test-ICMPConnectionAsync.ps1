using namespace System.Threading
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
            # [WaitHandle]::WaitAny(..) supports 62 Tasks in STA, also too many tasks can hang the session
            if($tasks.Count -eq 62) {
                do {
                    $id = [WaitHandle]::WaitAny($tasks.PingTask.AsyncWaitHandle, 200)
                    if($id -eq [WaitHandle]::WaitTimeout -or -not $tasks[$id].DnsTask.IsCompleted) {
                        continue
                    }
                    $target, $instance, $ping, $dns = $tasks[$id][$tasks[$id].PSBase.Keys]
                    $instance.ForEach('Dispose')
                    $tasks.RemoveAt($id)

                    $response = $ping.GetAwaiter().GetResult()

                    $latency = (
                        '*', [string]::Format('{0} ms', $response.RoundtripTime)
                    )[$response.Status -eq 'Success']

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
                } while($tasks)
            }

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
}

1..254 | ForEach-Object { "192.168.0.$_" } | Test-ICMPConnectionAsync | Format-Table
