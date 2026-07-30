# SQL Guide — Step by Step

Your database work has a twist: because we chose **EF Core code-first** ([doc 11](<../AI Guide/11-coding-standards.md>)), you will *not* hand-write `CREATE TABLE` for the app — C# entity classes generate the schema. So the SQL skills you actually need are: **connecting, exploring, querying, writing report views, and backups**. That's this guide.

---

## Step 1 — Prove LocalDB is alive (5 min)

Open PowerShell and run each line, reading the output:

```powershell
sqllocaldb info                    # lists instances → expect MSSQLLocalDB
sqllocaldb start MSSQLLocalDB      # starts it (fine if "already running")
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT @@VERSION"
```

*What's happening:* LocalDB is a full SQL engine that runs as a little background process under **your** Windows user — no service, no password; your Windows login is the admin. `(localdb)\MSSQLLocalDB` is its address.

✅ **Checkpoint:** the last command prints "Microsoft SQL Server 2019 … Express Edition".

## Step 2 — Install SSMS (15 min, mostly waiting)

SSMS (SQL Server Management Studio) is the free GUI you'll live in.

1. Search the web for **"download SSMS"** — take the link on `learn.microsoft.com` (Microsoft's site, free).
2. Install with defaults. Big download; get coffee.
3. Open SSMS → connect dialog: Server name `(localdb)\MSSQLLocalDB`, Authentication **Windows Authentication** → Connect.

✅ **Checkpoint:** Object Explorer shows your server with **Databases → System Databases** (master, model, msdb, tempdb) — engine plumbing; you never touch these, but seeing them means you're in.

## Step 3 — SQL warm-up on a scratch database (30–60 min)

Even though EF creates the real schema, you must be comfortable *talking* to a database. Practice where mistakes are free. In SSMS click **New Query** and type (don't paste) each block, pressing F5 to run:

```sql
CREATE DATABASE Scratch;
```
```sql
USE Scratch;   -- "point my queries at Scratch from now on"

CREATE TABLE Fruit (
    FruitId   INT IDENTITY(1,1) PRIMARY KEY,  -- auto-numbering ID
    Name      NVARCHAR(50) NOT NULL,
    PricePerKg DECIMAL(18,2) NOT NULL
);

INSERT INTO Fruit (Name, PricePerKg) VALUES ('Peach', 34.50), ('Plum', 28.00), ('Fig', 55.00);

SELECT * FROM Fruit;
SELECT Name FROM Fruit WHERE PricePerKg > 30 ORDER BY PricePerKg DESC;
```

Now the one that matters most for this project — a **JOIN** (reports are 90% joins):

```sql
CREATE TABLE Sale (
    SaleId  INT IDENTITY PRIMARY KEY,
    FruitId INT NOT NULL REFERENCES Fruit(FruitId),  -- foreign key
    Kg      DECIMAL(18,3) NOT NULL
);
INSERT INTO Sale (FruitId, Kg) VALUES (1, 2.5), (1, 1.0), (3, 0.75);

SELECT f.Name, SUM(s.Kg) AS TotalKg, SUM(s.Kg * f.PricePerKg) AS TotalRand
FROM Sale s
JOIN Fruit f ON f.FruitId = s.FruitId
GROUP BY f.Name;
```

Read that result until it makes complete sense — it is literally a mini version of the month-end sales report ([doc 04](<../AI Guide/04-reporting.md>)). Then clean up:

```sql
USE master;
DROP DATABASE Scratch;
```

✅ **Checkpoint:** the JOIN query showed Peach 3.5 kg / R120.75 and Fig 0.75 kg / R41.25, and you can say *why* Plum isn't in the result. (Answer: no sales rows → an INNER JOIN drops it. A `LEFT JOIN` from Fruit would keep it with NULLs — try it.)

## Step 4 — How to *read* the schema EF will create (do after backend M2)

When EF has created the `FarmApp` database, explore it like this:

- Object Explorer → FarmApp → Tables → expand Columns/Keys on a table. Compare against the entity class in C# — same thing, two views.
- Ask the database about itself:
```sql
USE FarmApp;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME;
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Grade';
SELECT TOP 5 * FROM __EFMigrationsHistory;  -- EF's diary of applied migrations
```
- **Rule for the whole project: SSMS is read-only for app tables.** Changing data/schema by hand behind EF's back causes drift and pain. Look with SSMS; change through the app/migrations.

✅ **Checkpoint:** you can find the `__EFMigrationsHistory` rows and explain what they are (one row per migration that has been applied to *this* database).

## Step 5 — Report views (your real SQL authorship — from milestone M8)

Per [doc 04](<../AI Guide/04-reporting.md>), reports read from **SQL views** in a `Reporting` schema. A view is a saved SELECT that pretends to be a table. This is where your JOIN skills earn their keep:

```sql
CREATE SCHEMA Reporting;
```
```sql
CREATE VIEW Reporting.StockOnHand AS
SELECT p.Name AS Product, g.Name AS Grade,
       SUM(m.Qty) AS QtyOnHand,                   -- movements are signed → SUM = balance
       SUM(m.Qty * b.UnitCost) AS ValueOnHand
FROM StockMovement m
JOIN StockBatch b ON b.StockBatchId = m.StockBatchId
JOIN Product p    ON p.ProductId    = b.ProductId
LEFT JOIN Grade g ON g.GradeId      = b.GradeId    -- LEFT: grade is nullable now
GROUP BY p.Name, g.Name
HAVING SUM(m.Qty) <> 0;
```

Workflow for every view: write the SELECT alone until it's right → wrap in `CREATE VIEW` → **save the script in the repo** under `db\views\` (views are code; they get versioned) → the API reads it with plain SQL/Dapper.

✅ **Checkpoint (when you get here):** `SELECT * FROM Reporting.StockOnHand` matches what the app's stock screen shows.

## Step 6 — Backups: do one restore before you trust anything (from first real data)

```sql
BACKUP DATABASE FarmApp
TO DISK = 'H:\FarmApp\backups\FarmApp.bak' WITH INIT;
```
(Create the folder first.) Now **prove it restores** — an untested backup is a hope, not a backup ([doc 10](<../AI Guide/10-go-live-controls.md>)):

```sql
RESTORE DATABASE FarmApp_RestoreTest
FROM DISK = 'H:\FarmApp\backups\FarmApp.bak'
WITH MOVE 'FarmApp'     TO 'H:\FarmApp\backups\rt.mdf',
     MOVE 'FarmApp_log' TO 'H:\FarmApp\backups\rt.ldf';
```
Query a table in `FarmApp_RestoreTest`, smile, then `DROP DATABASE FarmApp_RestoreTest;`.
Later, put the BACKUP command in a `.ps1` on Task Scheduler (nightly) + copy the `.bak` off the machine.

✅ **Checkpoint:** you restored a backup and read data from it. You are now ahead of most professionals.

## Habits that prevent disasters

- Before any hand-written `UPDATE`/`DELETE` (rare, but life happens): run the same statement as a `SELECT` first to see exactly which rows you'll hit. Then wrap the change in `BEGIN TRAN;` … check … `COMMIT;` (or `ROLLBACK;` to undo).
- An `UPDATE` or `DELETE` **without a WHERE clause** hits every row in the table. Read twice, run once.
- `NULL` is "unknown", not zero: `WHERE GradeId = NULL` never matches — use `IS NULL`.
