# Hotel Booking System — Infrastructure

Docker infra baseline: MySQL 8.4 + Redis 7.4 on network `hotel-booking-net`.

## Quick start

Windows (no `make`):

```powershell
.\scripts\hbs.ps1 init        # secrets + .env + up + healthcheck
.\scripts\hbs.ps1 ps
.\scripts\hbs.ps1 up-tools    # + CloudBeaver :8979, RedisInsight :5541
```

Linux / macOS / WSL:

```sh
make init
make ps
make up-tools
```

Both wrap the same `docker compose` calls. `.\scripts\hbs.ps1 help` / `make help` lists tasks.

## Layout

```
docker-compose.yml              # mysql, redis, tools profile
docker-compose.override.yml     # dev-only overrides (auto-loaded)
hotel-booking-system.code-workspace
Makefile                        # POSIX task runner
scripts/hbs.ps1                 # Windows task runner
scripts/gen-secrets.sh
infra/mysql/Dockerfile          # bakes my.cnf (see note below)
infra/mysql/conf.d/my.cnf       # utf8mb4, READ-COMMITTED, slow log
infra/mysql/initdb/01-schema.sql
infra/mysql/initdb/02-seed-dev.sql
infra/mysql/initdb/03-app-user.sh
infra/redis/redis.conf          # AOF, allkeys-lru, destructive cmds disabled
infra/cloudbeaver/initial-data-sources.conf   # pre-seeded DBeaver connections (no passwords)
infra/secrets/*.txt             # docker secrets (gitignored)
infra/backup/                   # mysqldump output (gitignored)
services/                       # app services go here
```

## Network

| item    | value               |
| ------- | ------------------- |
| name    | `hotel-booking-net` |
| driver  | bridge              |
| subnet  | `172.29.0.0/16`     |
| DNS     | `mysql`, `redis`    |

Other compose projects join it as external:

```yaml
networks:
  hotel-booking-net:
    external: true
```

## Ports

| service      | host bind        | container |
| ------------ | ---------------- | --------- |
| mysql        | `0.0.0.0:3306`   | 3306      |
| redis        | `0.0.0.0:6379`   | 6379      |
| cloudbeaver  | `127.0.0.1:8979` | 8978      |
| redisinsight | `127.0.0.1:5541` | 5540      |

Override any of them in `.env`. Tool UIs bind to loopback only — see [docs/COMMANDS.md](docs/COMMANDS.md) §7.

## DB UI — CloudBeaver

CloudBeaver is DBeaver's server edition (same SQL editor, ER diagrams, data grid) over HTTP. Adminer was dropped.

```powershell
.\scripts\hbs.ps1 up-tools
Start-Process http://127.0.0.1:8979
```

First visit runs the admin-setup wizard (password: min 8, mixed case, ≥1 digit). Anonymous access is off. Two connections come pre-seeded from `infra/cloudbeaver/initial-data-sources.conf` — `HBS MySQL (app user)` and `HBS MySQL (root)` — with `save-password: false`, so CloudBeaver prompts and no credential is committed. The `mysql8` JDBC driver is bundled in the image; no internet needed.

## Credentials

File-based docker secrets under `/run/secrets/`. No passwords in env vars, none in git.

| secret                      | used by                    |
| --------------------------- | -------------------------- |
| `mysql_root_password.txt`   | MySQL root, admin tasks    |
| `mysql_app_password.txt`    | app user `hbs_app`         |
| `redis_password.txt`        | Redis `requirepass`        |

```
mysql://hbs_app:<infra/secrets/mysql_app_password.txt>@127.0.0.1:3306/hotel_booking
redis://:<infra/secrets/redis_password.txt>@127.0.0.1:6379/0
```

`hbs_app` holds `SELECT, INSERT, UPDATE, DELETE, EXECUTE` only — no DDL, no `GRANT`. `MYSQL_USER` is intentionally absent from compose: the entrypoint would grant it `ALL PRIVILEGES`, and that grant cannot be cleanly revoked (escaped-identifier mismatch on `` `db\_name` ``). `initdb/03-app-user.sh` creates the user with exact privileges instead.

## Redis keyspaces

| prefix       | purpose                  | TTL  |
| ------------ | ------------------------ | ---- |
| `hbs:lock:`  | booking lock (SET NX PX) | 30s  |
| `hbs:avail:` | availability cache       | 300s |
| `hbs:sess:`  | session                  | 24h  |
| `hbs:rate:`  | rate-limit counters      | 60s  |

`FLUSHALL`, `FLUSHDB`, and `CONFIG` are renamed to `""` in `redis.conf` — they return `ERR unknown command`. RedisInsight loses its config/metrics panes as a result; comment the `rename-command CONFIG` line out if you need them locally.

## Overbooking guard

`room_night_allocations (room_id, stay_date)` PK — one row per room-night. A second booking for the same room-night fails with `ERROR 1062 Duplicate entry`. Redis `hbs:lock:` is the fast reject path; the PK is the source of truth.

`bookings` also enforces `CHECK (check_out > check_in)` and `CHECK (total_amount >= 0)`.

## Ops

```powershell
.\scripts\hbs.ps1 mysql                                  # root shell
.\scripts\hbs.ps1 redis                                  # authenticated redis-cli
.\scripts\hbs.ps1 backup                                 # infra/backup/<db>-<ts>.sql
.\scripts\hbs.ps1 restore -File infra\backup\x.sql
```

Warning: `nuke` runs `docker compose down -v`, permanently deleting volumes `hbs-mysql-data` and `hbs-redis-data` and every row in them. Take a backup first. There is no undo.

## Notes

- All image tags pinned (no `:latest`) → reproducible builds.
- MySQL config is baked via `infra/mysql/Dockerfile`, not bind-mounted: a `.cnf` bind-mounted from an NTFS host is world-writable, and `mysqld` silently ignores world-writable config files.
- MySQL 8.4 removed `--default-authentication-plugin`; use `--authentication-policy`.
- Redis runs as uid `999` and does not need a chown — a fresh named volume inherits `/data` ownership from the image.
- Healthchecks on mysql + redis; dependents use `condition: service_healthy`.
- Log rotation: 10 MB × 3 per container.
- `initdb/*` runs only against an empty `hbs-mysql-data` volume. Later schema changes need a migration tool.
