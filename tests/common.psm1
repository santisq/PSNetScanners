function makeiprange {
    param([string] $ip, [int] $start, [int] $end)

    $start..$end | ForEach-Object { "$ip.$_" }
}
