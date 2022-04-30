using namespace System.Diagnostics
using namespace System.Collections.Generic
using namespace System.Collections.Specialized
using namespace System.Net.Sockets
using namespace System.Threading.Tasks

function Test-TCPConnectionAsync {
    [cmdletbinding()]
    param(
        [parameter(Mandatory, Valuefrompipeline)]
        [string[]] $Target,

        [parameter(Mandatory, Position = 1)]
        [ValidateRange(1, 65535)]
        [int[]] $Port,

        # 1 second minimum, reasonable for TCP connection
        [parameter(Position = 2)]
        [ValidateRange(1000, [int]::MaxValue)]
        [int] $TimeOut = 1200
    )

    begin {
        $timer = [Stopwatch]::StartNew()
        $tasks = [List[OrderedDictionary]]::new()
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

'google.com' + 'cisco.com' + 'amazon.com' | Test-TCPConnectionAsync 80, 443, 8080, 389, 636
