<h1 align="center">PSNetScanners</h1>
<div align="center">
  <sub>High-performance parallel ICMP and TCP scanners for PowerShell</sub>
  <br /><br />

[![build](https://github.com/santisq/PSNetScanners/actions/workflows/ci.yml/badge.svg)](https://github.com/santisq/PSNetScanners/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/santisq/PSNetScanners/branch/main/graph/badge.svg?token=b51IOhpLfQ)](https://codecov.io/gh/santisq/PSNetScanners)
[![PowerShell Gallery](https://img.shields.io/powershellgallery/v/PSNetScanners?label=gallery)](https://www.powershellgallery.com/packages/PSNetScanners)
[![LICENSE](https://img.shields.io/github/license/santisq/PSNetScanners)](https://github.com/santisq/PSNetScanners/blob/main/LICENSE)

</div>

**PSNetScanners** is a lightweight PowerShell module that provides fast, parallel network scanning capabilities using modern async .NET APIs.

It includes two main cmdlets:

- **`Test-PingAsync`** – Parallel ICMP echo requests (Ping)
- **`Test-TcpAsync`** – Parallel TCP port scanning

These cmdlets are designed as high-performance alternatives to `Test-Connection` and `Test-NetConnection -Port`, sacrificing some advanced options for significantly better speed when scanning multiple targets or ports.

## Features

- True parallel execution with configurable throttling
- Async pattern using `Ping.SendPingAsync` and `TcpClient.ConnectAsync`
- Never throws terminating errors (result-oriented design)
- Excellent pipeline support (great for CSV input)
- Compatible with Windows PowerShell 5.1 and PowerShell 7+

## Documentation

Check out [**the docs**](./docs/en-US) for information about how to use this Module.

## Installation

### Gallery

This module is available through the [PowerShell Gallery](https://www.powershellgallery.com/packages/PSNetScanners):

```powershell
Install-Module PSNetScanners -Scope CurrentUser
```

### Source

```powershell
git clone 'https://github.com/santisq/PSNetScanners.git'
cd ./PSNetScanners
./build.ps1
```

## Requirements

- Windows PowerShell 5.1 or PowerShell 7+

## Quick Examples

```powershell
# Fast ping sweep
1..254 | ForEach-Object { "192.168.1.$_" } | Test-PingAsync -ThrottleLimit 100

# Fast port scan
Test-TcpAsync google.com 80,443,3389 -ConnectionTimeout 2000

# Bulk scan from CSV
Import-Csv .\targets.csv | Test-TcpAsync
```

## Contributing

Contributions are welcome! Feel free to fork the repository and submit a Pull Request.
