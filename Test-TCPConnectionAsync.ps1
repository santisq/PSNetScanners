using namespace System.Diagnostics
using namespace System.Collections.Generic
using namespace System.Collections.Specialized
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
        [string[]] $Target,

        [parameter(Mandatory, Position = 1, ValueFromPipelineByPropertyName)]
        [ValidateRange(1, 65535)]
        [int[]] $Port,

        [parameter(Position = 2)]
        [ValidateRange(1000, [int]::MaxValue)]
        [int] $TimeOut = 5 # In seconds!
    )

    begin {
        $timer   = [Stopwatch]::StartNew()
        $tasks   = [List[OrderedDictionary]]::new()
        $TimeOut = [timespan]::FromSeconds($TimeOut).TotalMilliseconds
    }
    process {
        foreach($t in $Target) {
            foreach($i in $Port) {
                $tcp = [TCPClient]::new()
                $tasks.Add([ordered]@{
                    Instance = $tcp
                    Task     = $tcp.ConnectAsync($t, $i)
                    Output   = [ordered]@{
                        Source       = $env:COMPUTERNAME
                        Destionation = $t
                        Port         = $i
                    }
                })
            }
        }
    }
    end {
        do {
            $id = [Task]::WaitAny($tasks.Task, 200)
            if($id -eq -1) {
                continue
            }
            $instance, $task, $output = $tasks[$id][$tasks[$id].PSBase.Keys]
            $output['Success'] = $task.Status -eq [TaskStatus]::RanToCompletion
            $instance.ForEach('Dispose') # Avoid any throws here
            $tasks.RemoveAt($id)
            [pscustomobject] $output
        } while($tasks -and $timer.ElapsedMilliseconds -le $timeout)

        foreach($t in $tasks) {
            $instance, $task, $output = $t[$t.PSBase.Keys]
            $output['Success'] = $task.Status -eq [TaskStatus]::RanToCompletion
            $instance.ForEach('Dispose') # Avoid any throws here
            [pscustomobject] $output
        }
    }
}