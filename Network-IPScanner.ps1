$ErrorActionPreference = 'Stop'

# Define List of IPs / Hosts to Ping
# $list = 'host1', 'host2', 'host3'
$list = 1..254 | ForEach-Object {
    "192.168.0.$_"
}

function Test-ICMPConnection {
    [cmdletbinding(DefaultParameterSetName = 'DefaultParams')]
    param(
        [parameter(Mandatory, ValueFromPipeline, Position = 0)]
        [string[]] $Address,
        [parameter(ParameterSetName = 'DefaultParams', Position = 1)]
        [int] $Count = 4,
        [parameter(ParameterSetName = 'DefaultParams', Position = 2)]
        [int] $TimeOut = 1000,
        [parameter(ParameterSetName = 'Quiet', Position = 1)]
        [switch] $Quiet,
        [parameter(ParameterSetName = 'DefaultParams', Position = 3)]
        [string] $Buffer = 'aaaaaaaaaa'
    )

    begin {
        $ping     = [System.Net.NetworkInformation.Ping]::new()
        $options  = [System.Net.NetworkInformation.PingOptions]::new()
        $data     = [System.Text.Encoding]::Unicode.GetBytes($Buffer)
        $hostname = $env:COMPUTERNAME
        $options.DontFragment = $true
    }
    process {
        foreach($i in $Address) {
            if($Quiet.IsPresent) {
                return [bool]$ping.Send($i, $TimeOut, $data, $options).RoundtripTime
            }

            $resolver = try {
                [System.Net.Dns]::GetHostEntry($i).HostName
            }
            catch { '*' }

            1..$Count | ForEach-Object {
                $response = $ping.Send($i, $TimeOut, $data, $options)
                $latency = (
                    '*', [string]::Format('{0} ms', $response.RoundtripTime)
                )[$response.Status -eq 'Success']

                [pscustomobject]@{
                    Ping        = $_
                    Source      = $hostname
                    Address     = $response.Address
                    Destination = $resolver
                    Latency     = $latency
                    Status      = $response.Status
                }
            }
        }
    }
    end {
        $ping.ForEach('Dispose')
    }
}

# Store the function Definition
$funcDef = ${function:Test-ICMPConnection}.ToString()
$scriptBlock = {
    param([string] $ip, [string] $funcDef)

    # Load the function in this Scope
    ${function:Test-ICMPConnection} = $funcDef

    # Define which arguments will be used for Pinger
    # Default Values are:
    #
    #   -Count 1
    #   -TimeOut 1000 (Milliseconds)
    #   -Buffer 'aaaaaaaaaa' (10 bytes)
    #   -Quiet:$false

    Test-ICMPConnection -Address $ip -Count 1
}

& {
    try {
        # Change this value for tweaking
        $Threshold = 100
        $RunspacePool = [runspacefactory]::CreateRunspacePool(1, $Threshold)
        $RunspacePool.Open()

        $runspaces = foreach($ip in $list) {
            $params = @{
                IP      = $ip
                funcDef = $funcDef
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
} | Format-Table -AutoSize
