function makeiprange {
    param([string] $ip, [int] $start, [int] $end)

    $start..$end | ForEach-Object { "$ip.$_" }
}

$targets = @'
Target,Port
google.com,80
google.com,443
google.com,8080
google.com,389
google.com,636
github.com,80
github.com,8080
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
'@ | ConvertFrom-Csv

Export-ModuleMember -Function * -Variable *
