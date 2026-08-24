#!/usr/bin/env pwsh
<#
.SYNOPSIS
  Hotel Booking System infra task runner (Windows equivalent of the Makefile).
.EXAMPLE
  ./scripts/hbs.ps1 up
#>
[CmdletBinding()]
param(
  [Parameter(Position = 0)]
  [ValidateSet('help','init','secrets','up','up-tools','down','stop','restart','ps','logs','health','mysql','redis','backup','restore','nuke')]
  [string]$Task = 'help',

  [Parameter(Position = 1)]
  [string]$File
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
Push-Location $Root
try {
  $DB = if ($env:MYSQL_DATABASE) { $env:MYSQL_DATABASE } else { 'hotel_booking' }

  # Passwords are generated alnum-only, so they need no shell quoting inside sh -c.
  # Windows PowerShell 5.1 mangles embedded double quotes when passing args to native exes.
  $RootPw = '$(cat /run/secrets/mysql_root_password)'
  $RedisPw = '$(cat /run/secrets/redis_password)'

  function Invoke-DC { docker compose @args; if ($LASTEXITCODE -ne 0) { throw "docker compose failed ($LASTEXITCODE)" } }

  function New-Secrets {
    $dir = Join-Path $Root 'infra/secrets'
    foreach ($n in 'mysql_root_password','mysql_app_password','redis_password') {
      $p = Join-Path $dir "$n.txt"
      if ((Test-Path $p) -and (Get-Item $p).Length -gt 0) { Write-Host "skip  $n.txt (exists)"; continue }
      $bytes = [byte[]]::new(48)
      [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
      $pw = ([Convert]::ToBase64String($bytes) -replace '[^A-Za-z0-9]','')
      [System.IO.File]::WriteAllText($p, $pw.Substring(0, 32))
      Write-Host "wrote $n.txt"
    }
  }

  function Wait-Healthy {
    Write-Host 'waiting for healthy...'
    for ($i = 0; $i -lt 60; $i++) {
      $h = (docker compose ps --format '{{.Health}}' | Select-String -SimpleMatch 'healthy').Count
      if ($h -ge 2) { Write-Host 'mysql + redis healthy'; return }
      Start-Sleep -Seconds 2
    }
    throw 'TIMEOUT - check: ./scripts/hbs.ps1 logs'
  }

  switch ($Task) {
    'help' {
      @'
  help       show tasks
  init       secrets + .env + up
  secrets    generate infra/secrets/*.txt (idempotent)
  up         start mysql + redis
  up-tools   + cloudbeaver (DBeaver web) + redisinsight
  down       stop + remove containers (volumes kept)
  stop       stop containers
  restart    down then up
  ps         container status
  logs       tail all logs
  health     wait for healthy services
  mysql      root mysql shell
  redis      authenticated redis-cli
  backup     dump db to infra/backup/
  restore    restore -File <path.sql>
  nuke       DESTRUCTIVE: remove containers + volumes
'@ | Write-Host
    }
    'secrets'  { New-Secrets }
    'init'     {
      New-Secrets
      if (-not (Test-Path '.env')) { Copy-Item '.env.example' '.env' }
      Invoke-DC up -d; Wait-Healthy
    }
    'up'       { Invoke-DC up -d; Wait-Healthy }
    'up-tools' { Invoke-DC --profile tools up -d }
    'down'     { Invoke-DC down }
    'stop'     { Invoke-DC stop }
    'restart'  { Invoke-DC down; Invoke-DC up -d; Wait-Healthy }
    'ps'       { Invoke-DC ps }
    'logs'     { Invoke-DC logs -f --tail=100 }
    'health'   { Wait-Healthy }
    'mysql'    { docker compose exec mysql sh -c ('exec mysql -uroot -p' + $RootPw + ' ' + $DB) }
    'redis'    { docker compose exec redis sh -c ('exec redis-cli -a ' + $RedisPw + ' --no-auth-warning') }
    'backup'   {
      $ts  = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
      $out = Join-Path $Root "infra/backup/$DB-$ts.sql"
      $cmd = 'exec mysqldump -uroot -p' + $RootPw + ' --single-transaction --routines --triggers --events ' + $DB
      docker compose exec -T mysql sh -c $cmd | Set-Content -Path $out -Encoding utf8
      if ($LASTEXITCODE -ne 0) { throw 'mysqldump failed' }
      Write-Host "wrote $out"
    }
    'restore'  {
      if (-not $File) { throw 'usage: ./scripts/hbs.ps1 restore -File infra/backup/x.sql' }
      $cmd = 'exec mysql -uroot -p' + $RootPw + ' ' + $DB
      Get-Content $File -Raw | docker compose exec -T mysql sh -c $cmd
    }
    'nuke'     {
      Write-Warning 'This permanently deletes the hbs-mysql-data and hbs-redis-data volumes and every row in them. There is no undo.'
      if ((Read-Host 'type yes to confirm') -ne 'yes') { return }
      Invoke-DC down -v
    }
  }
}
finally { Pop-Location }
