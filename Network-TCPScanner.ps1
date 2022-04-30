$Threshold = 100 # => Number of threads running
[int[]] $PortsToScan = 80, 443, 125, 8080
[string[]] $HostsToScan = 'google.com', 'cisco.com', 'amazon.com'

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
        $timer = [System.Diagnostics.Stopwatch]::StartNew()
        $tasks = [System.Collections.Generic.List[System.Collections.Specialized.OrderedDictionary]]::new()
    }
    process {
        foreach($t in $Target) {
            foreach($i in $Port) {
                if($tasks.Count -eq 62) {
                    Wait-Tasks
                }

                $tcp = [System.Net.Sockets.TcpClient]::new()
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
            $id = [System.Threading.Tasks.Task]::WaitAny($tasks.Task, 200)
            if($id -eq -1) {
                continue
            }
            $instance, $task, $output = $tasks[$id][$tasks[$id].PSBase.Keys]
            $output['Success'] = $task.Status -eq [System.Threading.Tasks.TaskStatus]::RanToCompletion
            $instance.ForEach('Dispose') # Avoid any throws here
            $tasks.RemoveAt($id)
            [pscustomobject] $output
        } while($tasks -and $timer.ElapsedMilliseconds -le $timeout)

        foreach($t in $tasks) {
            $instance, $task, $output = $t[$t.PSBase.Keys]
            $output['Success'] = $task.Status -eq [System.Threading.Tasks.TaskStatus]::RanToCompletion
            $instance.ForEach('Dispose') # Avoid any throws here
            [pscustomobject] $output
        }
    }
}

& {
    try {
        # Store function definition
        $funcDef = ${function:Test-TCPConnectionAsync}.ToString()
        $RunspacePool = [runspacefactory]::CreateRunspacePool(1, $Threshold)
        $RunspacePool.Open()
        $scriptBlock = {
            param([string] $hostname, [int[]] $ports, [string] $func)

            # Load the function in this Scope
            ${function:Test-TCPConnectionAsync} = $func
            Test-TCPConnectionAsync -Target $hostname -Port $ports -TimeOut 2000
        }

        $runspaces = foreach($i in $HostsToScan) {
            $params = @{
                hostname = $i
                ports    = $PortsToScan
                func     = $funcDef
            }

            $psinstance = [powershell]::Create().AddScript($scriptBlock).AddParameters($params)
            $psinstance.RunspacePool = $RunspacePool

            [pscustomobject]@{
                Instance = $psinstance
                Handle   = $psinstance.BeginInvoke()
            }
        }

        foreach($r in $runspaces) {
            $r.Instance.EndInvoke($r.Handle)
            $r.Instance.foreach('Dispose')
        }
    }
    catch {
        Write-Warning $_.Exception.Message
    }
    finally {
        $runspaces.foreach('Clear')
        $RunspacePool.foreach('Dispose')
    }
}
