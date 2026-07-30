# 08 — Offline & Sync, Explained

Answers to two questions: *how do we sync again when we get signal?* and *would we then want two applications?*

## Short answers

1. **Sync = an outbox queue.** While offline, every transaction (sale, harvest, wastage) is saved into the browser's local database (IndexedDB) with a client-generated GUID. When signal returns, a background sender POSTs each queued item to the API. The API is *idempotent*: if it has seen that GUID before it ignores the duplicate, otherwise it saves. Retry until acknowledged, then remove from queue. That's the whole mechanism.
2. **No second application.** One Angular app. "Offline capable" is a *property* you add to the existing web app (a PWA — Progressive Web App), not a separate program. A future mobile app would also be the *same API's* client, so even that doesn't fork the backend.

## How the sync actually works, step by step

### While online (normal life)
```
POS screen → POST /api/v1/sales {clientGuid: "3f2a...", lines: [...]}
API: "never seen 3f2a... → insert → 201 Created"
```

### Connection drops (market stall, load shedding)
```
1. POS screen works as normal — products & prices were already
   downloaded and cached locally (IndexedDB).
2. Each sale is written to the local "outbox" table instead:
   { clientGuid, payload, createdAt, status: Pending }
3. Cashier sees a small "3 sales waiting to sync" badge. Selling continues.
```

### Signal returns
```
4. A background sync service (Angular service + browser online event)
   walks the outbox oldest-first:
      POST /api/v1/sales {clientGuid: "3f2a...", ...}
5. API checks: does a Sale with ClientGuid '3f2a...' exist?
      no  → insert, deplete stock batches, return 201
      yes → do nothing, return 200 (it's a retry — maybe the response
            got lost last time even though the save worked)
6. On 2xx: outbox row marked Synced (and later purged).
   On 5xx/timeout: stays Pending, retried with backoff.
   On 400 (validation): marked Failed and shown to the user — never
   silently dropped.
```

The GUID is the trick. Because the *client* invents the ID, retrying is always safe: the operation can be sent 1 or 10 times with the same result. This is why `Sale.ClientGuid UNIQUE` is in the data model (doc 02).

### What syncs in which direction

| Data | Direction | When |
|------|-----------|------|
| Products, grades, pack sizes, price lists | server → client | on app load + every reconnect |
| Stock-on-hand snapshot (advisory only) | server → client | on reconnect |
| Sales, payments | client → server (outbox) | immediately, or on reconnect |
| Harvests, wastage, activities | client → server (outbox) | same pattern |

### Conflicts — why there mostly aren't any
Sales, harvests, and wastage entries are **append-only facts** — two devices can't "edit the same row", so classic sync conflicts don't exist. The one soft spot: while offline you might sell stock the server thinks is already gone. We deliberately allow it (the customer is standing in front of you holding the fruit — reality wins) and let the batch balance go briefly negative until the next stock take/adjustment reconciles it. **Never** attempt distributed stock locking; that's the road to madness.

Editing *master data* (prices, products) offline is where real conflicts live → simply **don't allow it offline**. Master data is edited online, in the office. Cheap rule, removes the whole problem class.

## One application: what "PWA" means here

A PWA is the normal Angular website plus:
- a **manifest** (name, icon) so the browser offers "Install" — it then opens in its own window like an app, on desktop or phone;
- a **service worker** that caches the app shell so it opens without a network;
- **IndexedDB** (via the Dexie.js library) for the cached master data + the outbox.

Same repo, same build, same deployment. The admin/reporting screens simply require a connection; only POS + capture screens get the offline treatment.

### Where a future mobile app fits
If a native/mobile app ever happens, it talks to the same versioned API and implements the same outbox pattern with the same ClientGuid contract (this is exactly why doc 03 mandates API-first: JWT, versioning, use-case endpoints). The backend never knows or cares which client type called it.

## Phasing (per the v1 = desktop website decision)

- **Now (Phase 0–2):** put `ClientGuid` on transaction tables and make the POST endpoints idempotent. This costs almost nothing and is the part that's hard to retrofit.
- **When market-day POS becomes real:** add the service worker, IndexedDB cache, and outbox service to the existing app. This is additive work, no rewrite — *because* the idempotent API already exists.
