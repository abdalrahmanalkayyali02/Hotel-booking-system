# Architecture

## Components

| service       | image                  | role                                       |
| ------------- | ---------------------- | ------------------------------------------ |
| mysql         | mysql:8.4.6            | source of truth: hotels, rooms, bookings   |
| redis         | redis:7.4.6-alpine     | cache, booking locks, sessions, rate limit |
| cloudbeaver   | dbeaver/cloudbeaver:25.2.3 | dev DB UI — DBeaver web (profile `tools`), 127.0.0.1:8979 |
| redisinsight  | redis/redisinsight:2.70| dev Redis UI (profile `tools`)             |

All on bridge network `hotel-booking-net`.

## Booking flow

```
POST /bookings
  → SET hbs:lock:<room>:<date> NX PX 30000     (redis, fast reject)
  → BEGIN
      INSERT bookings
      INSERT room_night_allocations (one row per night)   ← PK blocks overbooking
    COMMIT
  → DEL hbs:lock:*
  → DEL hbs:avail:<hotel>:*                    (invalidate cache)
```

Lock failure → 409. Duplicate-key on allocations → 409 (lock raced or expired).

## Data model

`hotels → room_types → rooms → bookings → room_night_allocations`, plus `guests` and `payments`.
`transaction-isolation = READ-COMMITTED` — avoids gap locks on the allocation range inserts.

## Adding a service

```yaml
services:
  api:
    build: ./services/api
    networks: [hotel-booking-net]
    environment:
      DB_HOST: mysql
      REDIS_HOST: redis
    secrets: [mysql_app_password, redis_password]
    # network is created by this project; external consumers declare it as external
    depends_on:
      mysql:  { condition: service_healthy }
      redis:  { condition: service_healthy }
```

## Verified behaviours

| check                                        | result                                             |
| -------------------------------------------- | -------------------------------------------------- |
| `hbs_app` DDL                                | `ERROR 1142 DROP command denied`                   |
| duplicate room-night                         | `ERROR 1062 Duplicate entry '1-2026-09-01'`        |
| `check_out = check_in`                       | `ERROR 3819 chk_bookings_dates is violated`        |
| second `SET hbs:lock:... NX`                 | nil (rejected), first holds ~30s TTL               |
| `redis-cli PING` without auth                | `NOAUTH Authentication required`                   |
| `FLUSHALL` / `CONFIG`                        | `ERR unknown command` (renamed out)                |
| redis process user                           | `uid=999(redis)`                                   |
| in-network DNS                               | `mysql` → 172.29.0.2, `redis` → 172.29.0.3         |
| `my.cnf` applied                             | `READ-COMMITTED`, `utf8mb4`, `slow_query_log=1`    |
