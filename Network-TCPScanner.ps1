$Threshold = 100 # => Number of threads running
[int[]] $PortsToScan = 80, 443, 125, 8080
[string[]] $HostsToScan = 'google.com', 'cisco.com', 'amazon.com'

function Test-TCPConnection {
    [cmdletbinding()]
    param(
        [parameter(Mandatory, Valuefrompipeline)]
        [string] $Name,

        [parameter(Mandatory, Position = 1)]
        [ValidateRange(1, 65535)]
        [int[]] $Port,

        # 1 second minimum, reasonable for TCP connection
        [ValidateRange(1000, [int]::MaxValue)]
        [int] $TimeOut = 1200
    )

    begin {
        $timer = [System.Diagnostics.Stopwatch]::StartNew()
        $tasks = [System.Collections.Generic.List[Object]]::new()
    }
    process {
        foreach($i in $Port) {
            $tcp  = [System.Net.Sockets.TCPClient]::new()
            $task = [ordered]@{
                Source       = $env:COMPUTERNAME
                Destionation = $Name
                Port         = $i
                TCP          = [ordered]@{
                    Instance = $tcp
                    Task     = $tcp.ConnectAsync($Name, $i)
                }
            }
            $tasks.Add($task)
        }
    }
    end {
        do {
            $id = [System.Threading.WaitHandle]::WaitAny($tasks[0..62].TCP.Task.AsyncWaitHandle, 200)
            if($id -eq [System.Threading.WaitHandle]::WaitTimeout) {
                continue
            }
            $thisTask = $tasks[$id]
            $instance, $task = $thisTask.TCP[0, 1]
            $thisTask['Success'] = $task.Status -eq 'RanToCompletion'
            $instance.foreach('Dispose') # Avoid any throws here
            $thisTask.Remove('TCP')
            [pscustomobject] $thisTask
            $tasks.RemoveAt($id)
        } while($tasks -and $timer.ElapsedMilliseconds -le $timeout)

        # clear remaining tasks, if any
        foreach($task in $tasks) {
            $task.TCP.Instance.foreach('Dispose')
            $task.Remove('TCP')
            $task['Success'] = $false
            [pscustomobject] $task
        }
    }
}

& {
    try {
        # Store function definition
        $funcDef = ${function:Test-TCPConnection}.ToString()
        $RunspacePool = [runspacefactory]::CreateRunspacePool(1, $Threshold)
        $RunspacePool.Open()
        $scriptBlock = {
            param([string] $hostname, [int[]] $ports, [string] $func)

            # Load the function in this Scope
            ${function:Test-TCPConnection} = $func
            Test-TCPConnection -Name $hostname -Port $ports
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
