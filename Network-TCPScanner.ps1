$Threshold = 100 # => Number of threads running
[int[]] $PortsToScan = 80, 443, 125, 8080
[string[]] $HostsToScan = 'google.com', 'cisco.com', 'amazon.com'

# Define a TCP Port scanner function
function Test-Port {
    [cmdletbinding()]
    param(
        [parameter(Mandatory, Valuefrompipeline)]
        [string] $Name,
        [parameter(Mandatory)]
        [int[]] $Port,
        [switch] $Detailed,
        [int] $TimeOut = 1200
    )

    process {
        foreach($i in $port) {
            $response = [ordered]@{
                Host = $name
                Port = $i
            }

            try {
                $testPort = [System.Net.Sockets.TCPClient]::new()
                $testPort.SendTimeout = $TimeOut
                $testPort.ReceiveTimeout = $TimeOut
                $testPort.Connect($name, $i)
            }
            catch {
                if($Detailed.IsPresent) {
                    $response['Message'] = $_.Exception.InnerException.Message
                }
            }
            finally {
                $response['TestConnection'] = $testPort.Connected
                $testPort.Close()
                [pscustomobject] $response
            }
        }
    }
}

& {
    try {
        # Store function definition
        $funcDef = ${function:Test-Port}.ToString()
        $RunspacePool = [runspacefactory]::CreateRunspacePool(1, $Threshold)
        $RunspacePool.Open()
        $scriptBlock = {
            param([string] $hostname, [int[]] $ports, [string] $func)

            # Load the function in this Scope
            ${function:Test-Port} = $func
            Test-Port -Name $hostname -Port $ports
        }

        $runspaces = foreach($i in $HostsToScan) {
            foreach($p in $PortsToScan) {
                $params = @{
                    hostname = $i
                    ports    = $p
                    func     = $funcDef
                }

                $psinstance = [powershell]::Create().AddScript($scriptBlock).AddParameters($params)
                $psinstance.RunspacePool = $RunspacePool

                [pscustomobject]@{
                    Instance = $psinstance
                    Handle   = $psinstance.BeginInvoke()
                }
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
