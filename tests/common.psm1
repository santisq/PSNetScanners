function makeiprange {
    param([string] $ip, [int] $start, [int] $end)

    $start..$end | ForEach-Object { "$ip.$_" }
}

function Test-CmdletCancellation {
    param(
        [Parameter(Mandatory)]
        [string] $Script,

        [Parameter(Mandatory)]
        [string] $ModulePath,

        [Parameter(Mandatory)]
        [System.Management.Automation.PSDataCollection[psobject]] $InvocationInput,

        [timespan] $DelayBeforeStop = '00:00:01')

    $iss = [initialsessionstate]::CreateDefault2()
    $iss.ImportPSModulesFromPath($path)
    $ps = [powershell]::Create($iss).AddScript($Script)

    Measure-Command {
        $task = $ps.BeginInvoke($InvocationInput)
        [System.Threading.Thread]::Sleep($DelayBeforeStop)
        $ps.Stop()
        try { $ps.EndInvoke($task) }
        catch [System.Management.Automation.PipelineStoppedException] { } # expected
        finally { $ps.Dispose() }
    }
}

$targets = @'
Target,Port
google.com,80
github.com,80
cisco.com,80
amazon.com,80
google.com,443
cisco.com,443
amazon.com,443
github.com,443
google.com,389
cisco.com,389
amazon.com,389
google.com,8080
github.com,8080
amazon.com,8080
cisco.com,8080
google.com,636
cisco.com,636
amazon.com,636
'@ | ConvertFrom-Csv

$moduleName = (Get-Item ([Path]::Combine($PSScriptRoot, '..', 'module', '*.psd1'))).BaseName
$manifestPath = [Path]::Combine($PSScriptRoot, '..', 'output', $moduleName)
$manifestPath, $targets | Out-Null

Export-ModuleMember -Function * -Variable *
