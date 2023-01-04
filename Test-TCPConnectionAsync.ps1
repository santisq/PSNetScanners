using namespace System.Diagnostics
using namespace System.Collections.Generic
using namespace System.Net.Sockets
using namespace System.Threading.Tasks

<#
.DESCRIPTION
PowerShell Function that leverages the ConnectAsync(...) Method from the TcpClient Class to send the async TCP connection requests.

.EXAMPLE
'google.com', 'cisco.com', 'amazon.com' | Test-TCPConnectionAsync 80, 443, 8080, 389, 636

.EXAMPLE
@'
Target,Port
google.com,80
google.com,443
google.com,8080
google.com,389
google.com,636
cisco.com,80
cisco.com,443
cisco.com,8080
cisco.com,389
cisco.com,636
amazon.com,80
amazon.com,443
amazon.com,8080
amazon.com,389
amazon.com,636
'@ | ConvertFrom-Csv | Test-TCPConnectionAsync
#>

function Test-TCPConnectionAsync {
    [cmdletbinding()]
    param(
        [parameter(Mandatory, Valuefrompipeline, ValueFromPipelineByPropertyName)]
        [alias('ComputerName', 'HostName', 'Host', 'Server')]
        [string[]] $Target,

        [parameter(Mandatory, ValueFromPipelineByPropertyName)]
        [ValidateRange(1, 65535)]
        [int[]] $Port,

        [parameter()]
        [ValidateRange(5, [int]::MaxValue)]
        [int] $TimeOut = 5, # In seconds!

        [parameter()]
        [switch] $IPv6
    )

    begin {
        $timer   = [Stopwatch]::StartNew()
        $queue   = [List[hashtable]]::new()
        $TimeOut = [timespan]::FromSeconds($TimeOut).TotalMilliseconds
        if($IPv6.IsPresent) {
            $newTcp = { [TCPClient]::new([AddressFamily]::InterNetworkV6) }
            return
        }
        $newTcp = { [TCPClient]::new() }
    }
    process {
        foreach($item in $Target) {
            foreach($i in $Port) {
                $tcp = & $newTcp
                $queue.Add(@{
                    Instance = $tcp
                    Task     = $tcp.ConnectAsync($item, $i)
                    Output   = [ordered]@{
                        Source       = $env:COMPUTERNAME
                        Destination  = $item
                        Port         = $i
                    }
                })
            }
        }
    }
    end {
        while($queue -and $timer.ElapsedMilliseconds -le $timeout) {
            try {
                $id = [Task]::WaitAny($queue.Task, 200)
                if($id -eq -1) {
                    continue
                }
                $instance, $task, $output = $queue[$id]['Instance', 'Task', 'Output']
                if($instance) {
                    $instance.Dispose()
                }
                $output['Success'] = $task.Status -eq [TaskStatus]::RanToCompletion
                $queue.RemoveAt($id)
                [pscustomobject] $output
            }
            catch {
                $PSCmdlet.WriteError($_)
            }
        }

        foreach($item in $queue) {
            try {
                $instance, $task, $output = $item['Instance', 'Task', 'Output']
                $output['Success'] = $task.Status -eq [TaskStatus]::RanToCompletion
                if($instance) {
                    $instance.Dispose()
                }
                [pscustomobject] $output
            }
            catch {
                $PSCmdlet.WriteError($_)
            }
        }
    }
}