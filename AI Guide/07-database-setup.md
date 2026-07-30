# 07 — Database Setup (this machine)

## Scan result (2026-07-26): a free SQL Server IS installed and working

**SQL Server 2019 Express LocalDB** is installed on this machine, instance name `MSSQLLocalDB`.
Verified working: instance starts, accepts connections, `sqlcmd` is installed at
`C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE`.

```
Version:   Microsoft SQL Server 2019 (RTM-CU27-GDR) 15.0.4382.1 (X64), Express Edition
Instance:  (localdb)\MSSQLLocalDB
Auth:      Windows Authentication (your Windows login is sysadmin — no password needed)
Databases: master, tempdb, model, msdb (system only — clean slate)
```

### Connection details to use

```
Server name:        (localdb)\MSSQLLocalDB
Authentication:     Windows Authentication
Database (planned): FarmApp
```

**appsettings.Development.json:**
```json
{
  "ConnectionStrings": {
    "FarmApp": "Server=(localdb)\\MSSQLLocalDB;Database=FarmApp;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  }
}
```
(The `FarmApp` database itself will be created by the first EF Core migration — nothing to pre-create.)

**Connecting with tools:**
- SSMS / Azure Data Studio: server name `(localdb)\MSSQLLocalDB`, Windows auth.
- Command line: `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT @@VERSION"`

### Managing LocalDB

```powershell
sqllocaldb info                 # list instances
sqllocaldb info MSSQLLocalDB    # instance details (state, version)
sqllocaldb start MSSQLLocalDB   # start (starts automatically on first connection too)
sqllocaldb stop MSSQLLocalDB    # stop
```
Database files live under `C:\Users\Andri\` (default LocalDB data path) unless a path is specified at CREATE DATABASE time — EF will use the default, which is fine for dev.

### What LocalDB is (and its limits)
- The full SQL Server Express engine, but run **on demand as your user process** — no Windows service, starts when you connect, stops when idle. Perfect for development.
- Limits (same as Express): 10 GB per database, limited CPU/RAM — irrelevant at this scale.
- **Not suitable for production/multi-user**: it runs under one Windows user and isn't reachable over the network by default. Fine while building; production needs one of the options below.

## Other findings from the machine/H:\ scan

- **No full SQL Server or SQL Express service** is installed (only the SQL VSS Writer helper service).
- **Nothing is listening on port 1433.** Old projects on H:\ (Packsys/Citrii) reference `Server=localhost,1433` with an `sa` login — that pattern matches a **Docker SQL Server container** from those projects. Docker Desktop is installed but the engine isn't running, and no such container is currently available.
- Other connection strings found on H:\ point to **remote work servers** (192.168.3.250, 197.189.231.218) — work infrastructure, not for this project. Note: those dev credentials sit in plain text in old appsettings files (including in the Recycle Bin); worth cleaning up/rotating at some point.

## Decision

**Use `(localdb)\MSSQLLocalDB` for development.** Zero setup, already verified. Revisit at go-live.

## Production options (for later, in order of likely preference)

1. **Cloud SQL** (Azure SQL Basic/S0 or SQL on a small VM) — pairs with the cloud-hosted API from doc 03; backups largely managed for you.
2. **SQL Server 2022 Express as a service** on a farm PC — free, proper service, network-accessible:
   1. Download "SQL Server 2022 Express" from Microsoft (Basic install).
   2. During/after install, enable Mixed Mode or keep Windows auth; note instance name (default `.\SQLEXPRESS`).
   3. In SQL Server Configuration Manager: enable TCP/IP protocol, restart service.
   4. Open firewall port 1433; set a fixed port.
   5. Create an SQL login for the app (least privilege, `db_owner` on FarmApp only).
   6. Connection string: `Server=MACHINE\SQLEXPRESS;Database=FarmApp;User ID=farmapp;Password=***;TrustServerCertificate=True`.
   7. Set up scheduled backups (doc 10) — Express has no SQL Agent, so use Task Scheduler + a backup script.
3. **Docker SQL container** (matches your old projects' pattern) — fine for dev parity, weakest option for production on Windows.
