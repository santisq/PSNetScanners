<h1 align="center">PSNetScanners</h1>
<div align="center">
<sub>PowerShell ICMP and TCP async scanners</sub>
<br /><br />

[![build](https://github.com/santisq/PSNetScanners/actions/workflows/ci.yml/badge.svg)](https://github.com/santisq/PSNetScanners/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/santisq/PSNetScanners/branch/main/graph/badge.svg?token=b51IOhpLfQ)](https://codecov.io/gh/santisq/PSNetScanners)
[![PowerShell Gallery](https://img.shields.io/powershellgallery/v/PSNetScanners?label=gallery)](https://www.powershellgallery.com/packages/PSNetScanners)
[![LICENSE](https://img.shields.io/github/license/santisq/PSNetScanners)](https://github.com/santisq/PSNetScanners/blob/main/LICENSE)

</div>

PSNetScanners is a PowerShell Module that includes two cmdlets using async techniques for ICMP and TCP scanning. Essentially, like built-in cmdlets `Test-Connection` or `Test-NetConnection` with a few less options but much faster.

## Documentation

Check out [__the docs__](./docs/en-US) for information about how to use this Module.

## Installation

### Gallery

The module is available through the [PowerShell Gallery](https://www.powershellgallery.com/packages/PSNetScanners):

```powershell
Install-Module PSNetScanners -Scope CurrentUser
```

### Source

```powershell
git clone 'https://github.com/santisq/PSNetScanners.git'
Set-Location ./PSNetScanners
./build.ps1
```

## Requirements

This module has no requirements and is fully compatible with __Windows PowerShell 5.1__ and [__PowerShell Core 7+__](https://github.com/PowerShell/PowerShell).

## Contributing

Contributions are more than welcome, if you wish to contribute, fork this repository and submit a pull request with the changes.
