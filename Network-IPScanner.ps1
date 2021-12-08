# Define List of IPs / Hosts to Ping
# $list = 'host1', 'host2', 'host3'
$list = 1..254 | ForEach-Object {
    "10.10.1.$_"
}

function Pinger {
[cmdletbinding(DefaultParameterSetName = 'DefaultParams')]
param(
    [parameter(
        Mandatory,
        ValueFromPipeline,
        Position = 0
    )]
    [string]$Address,
    [parameter(
        ParameterSetName = 'DefaultParams',
        Position = 1
    )]
    [int]$Count = 1,
    [parameter(
        ParameterSetName = 'DefaultParams',
        Position = 2
    )]
    [int]$TimeOut = 1000,
    [parameter(
        ParameterSetName = 'Quiet',
        Position = 1
    )]
    [switch]$Quiet,
    [parameter(
        ParameterSetName = 'DefaultParams',
        Position = 3
    )]
    [string]$Buffer = 'aaaaaaaaaa'
)

    process
    {
        $ping = [System.Net.NetworkInformation.Ping]::new()
        $options = [System.Net.NetworkInformation.PingOptions]::new()
        $options.DontFragment = $true
        $data = [System.Text.Encoding]::Unicode.GetBytes($Buffer)
        $hostname = [System.Net.Dns]::GetHostName()

        if($Quiet)
        {
            return [bool]$ping.Send(
                $Address,
                $TimeOut,
                $data,
                $options
            ).RoundtripTime
        }

        1..$Count | ForEach-Object {

            $response = $ping.Send($Address, $TimeOut, $data, $options)
            
            $latency = (
                '*',
                [string]::Format(
                    '{0} ms',
                    $response.RoundtripTime
                )
            )[$response.Status -eq 'Success']

            $resolver = try
            {
                [System.Net.Dns]::GetHostEntry($Address).HostName
            }
            catch
            {
                '*'
            }

            [pscustomobject]@{
                Ping     = $_
                Source   = $hostname
                Address  = $Address
                HostName = $resolver
                Latency  = $latency
                Status   = $response.Status
            }
        }
    }
}

# Store the function as String
$funcDef = "function Pinger { $function:Pinger }"

# Change this value for tweaking
$Threshold = 100
$RunspacePool = [runspacefactory]::CreateRunspacePool(1, $Threshold)
$RunspacePool.Open()

$scriptBlock = {
    param([string]$ip, [string]$Pinger)

    # Load the function in this Scope
    . ([scriptblock]::Create($Pinger))

    # Define which arguments will be used for Pinger
    # Default Values are:
    #
    #   -Count 1
    #   -TimeOut 1000 (Milliseconds)
    #   -Buffer 'aaaaaaaaaa' (10 bytes)
    #   -Quiet:$false

    Pinger -Address $ip
}

$runspaces = foreach($ip in $list)
{
    $params = @{
        IP = $ip
        Pinger = $funcDef
    }

    $psinstance = [powershell]::Create().AddScript($scriptBlock).AddParameters($params)
    $psinstance.RunspacePool = $RunspacePool
    
    [pscustomobject]@{
        Instance = $psinstance
        Handle = $psinstance.BeginInvoke()
    }
}

while($runspaces.Handle.IsCompleted -contains $false)
{
    Start-Sleep -Milliseconds 500
}

$results = $runspaces.ForEach({
    $_.Instance.EndInvoke($_.Handle)
    $_.Instance.Dispose()
})

$runspaces.Clear()
$RunspacePool.Dispose()
