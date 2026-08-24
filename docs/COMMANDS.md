# Command Reference — Hotel Booking System

All commands run from the repo root: `C:\Users\Sedra\Desktop\HotelMangementSystem`.
The `sh -c` payloads read passwords from `/run/secrets/*` inside the container — no credential lands in shell history.

---

## 0. First run (once)

```powershell
.\scripts\hbs.ps1 secrets                  # generate infra/secrets/*.txt
Copy-Item .env.example .env                # port / db-name overrides
docker compose build mysql                 # bake my.cnf into hbs/mysql:8.4.6
docker compose up -d                       # mysql + redis
docker compose ps                          # wait for (healthy)
```

One-shot: `.\scripts\hbs.ps1 init` — POSIX: `make init`

---

## 1. Run the stack

```powershell
docker compose up -d                       # core: mysql + redis
docker compose --profile tools up -d       # core + cloudbeaver + redisinsight
docker compose up -d --build               # rebuild hbs/mysql first
docker compose up -d --force-recreate      # recreate containers, keep volumes
docker compose up                          # foreground, Ctrl+C stops
```

Single service:

```powershell
docker compose up -d mysql
docker compose up -d redis
docker compose --profile tools up -d cloudbeaver
```

## 2. Status / logs

```powershell
docker compose ps
docker compose --profile tools ps                       # incl. tool containers
docker compose ps --format '{{.Service}} {{.Health}}'
docker compose logs -f --tail=100                       # all, follow
docker compose logs -f mysql
docker compose logs cloudbeaver --tail=50
docker inspect hbs-mysql --format '{{.State.Health.Status}}'
docker stats hbs-mysql hbs-redis --no-stream
```

## 3. Stop

```powershell
docker compose stop                        # stop, keep containers
docker compose start                       # start them again
docker compose restart mysql               # restart one service
docker compose down                        # stop + REMOVE containers, volumes kept
```

Warning: `docker compose down -v` also deletes the volumes `hbs-mysql-data`, `hbs-redis-data`, `hbs-cloudbeaver-workspace`, and `hbs-redisinsight-data`. Every table, row, cached key, and saved CloudBeaver connection is permanently destroyed. Take a backup first (section 5). There is no undo.

---

## 4. Database init

`infra/mysql/initdb/*` runs **only** against an empty `hbs-mysql-data` volume, in filename order:

| file               | does                                                  |
| ------------------ | ----------------------------------------------------- |
| `01-schema.sql`    | 7 tables, FKs, CHECK constraints, indexes             |
| `02-seed-dev.sql`  | 2 hotels, 3 room types, 5 rooms, 1 guest              |
| `03-app-user.sh`   | creates `hbs_app` with DML-only grants                |

Confirm it ran:

```powershell
docker compose logs mysql | Select-String 'initdb.d|app user'
```

### Re-run init from scratch

Warning: this deletes the `hbs-mysql-data` volume and every row in the database, then rebuilds the schema from `initdb/`. Back up first if the data matters.

```powershell
.\scripts\hbs.ps1 backup
docker compose down
docker volume rm hbs-mysql-data
docker compose up -d mysql
```

### Apply SQL to a running DB (no fresh volume needed)

```powershell
# interactive root shell
docker compose exec mysql sh -c 'exec mysql -uroot -p$(cat /run/secrets/mysql_root_password) hotel_booking'

# app-user shell (DML only)
docker compose exec mysql sh -c 'exec mysql -uhbs_app -p$(cat /run/secrets/mysql_app_password) hotel_booking'

# one-off query
docker compose exec -T mysql sh -c 'exec mysql -uroot -p$(cat /run/secrets/mysql_root_password) hotel_booking -e "SHOW TABLES;"'

# pipe a migration file in
Get-Content .\migrations\001.sql -Raw | docker compose exec -T mysql sh -c 'exec mysql -uroot -p$(cat /run/secrets/mysql_root_password) hotel_booking'
```

Shortcut: `.\scripts\hbs.ps1 mysql` / `make mysql`

### Redis

```powershell
docker compose exec redis sh -c 'exec redis-cli -a $(cat /run/secrets/redis_password) --no-auth-warning'
docker compose exec -T redis sh -c 'redis-cli -a $(cat /run/secrets/redis_password) --no-auth-warning DBSIZE'
docker compose exec -T redis sh -c 'redis-cli -a $(cat /run/secrets/redis_password) --no-auth-warning --scan --pattern "hbs:*"'
```

Shortcut: `.\scripts\hbs.ps1 redis` / `make redis`

`FLUSHALL`, `FLUSHDB`, and `CONFIG` are renamed to `""` in `redis.conf` → `ERR unknown command`. Clear dev keys by pattern instead:

```powershell
docker compose exec -T redis sh -c 'PW=$(cat /run/secrets/redis_password); redis-cli -a $PW --no-auth-warning --scan --pattern "hbs:*" | xargs -r redis-cli -a $PW --no-auth-warning DEL'
```

---

## 5. Backup / restore

```powershell
.\scripts\hbs.ps1 backup                             # infra/backup/hotel_booking-<ts>.sql
.\scripts\hbs.ps1 restore -File infra\backup\x.sql
```

Raw equivalents:

```powershell
docker compose exec -T mysql sh -c 'exec mysqldump -uroot -p$(cat /run/secrets/mysql_root_password) --single-transaction --routines --triggers --events hotel_booking' | Set-Content infra\backup\dump.sql -Encoding utf8

Get-Content infra\backup\dump.sql -Raw | docker compose exec -T mysql sh -c 'exec mysql -uroot -p$(cat /run/secrets/mysql_root_password) hotel_booking'
```

Warning: restoring overwrites existing rows in the target tables. Restore into a scratch database first if you are unsure of the dump's contents.

---

## 6. DB UI — CloudBeaver (DBeaver, dockerized)

Adminer is removed. CloudBeaver is DBeaver's server edition — same SQL editor, ER diagrams, and data grid, served over HTTP.

```powershell
docker compose --profile tools up -d cloudbeaver
Start-Process http://127.0.0.1:8979
```

First visit shows the admin-setup wizard. Create the admin account there; the password policy is min 8 chars, mixed case, at least 1 digit. Anonymous access is disabled (`CLOUDBEAVER_APP_ANONYMOUS_ACCESS_ENABLED=false`), so nothing is reachable until you do.

Two connections are pre-seeded from `infra/cloudbeaver/initial-data-sources.conf`:

| connection             | user      | privileges                       |
| ---------------------- | --------- | -------------------------------- |
| `HBS MySQL (app user)` | `hbs_app` | SELECT/INSERT/UPDATE/DELETE only |
| `HBS MySQL (root)`     | `root`    | full, for migrations             |

Passwords are deliberately **not** in that file (`save-password: false`) — CloudBeaver prompts on first connect. Read the value:

```powershell
Get-Content infra\secrets\mysql_app_password.txt
Get-Content infra\secrets\mysql_root_password.txt
```

`mysql-connector-j-8.2.0.jar` ships inside the image, so connecting needs no internet access.

Re-seed the connection list (drops CloudBeaver UI state only — no DB data):

```powershell
docker compose --profile tools down cloudbeaver
docker volume rm hbs-cloudbeaver-workspace
docker compose --profile tools up -d cloudbeaver
```

Redis UI: `http://127.0.0.1:5541`. `CONFIG` is renamed out, so RedisInsight's config and metrics panes error out. Comment `rename-command CONFIG ""` in `infra/redis/redis.conf` if you need them locally.

---

## 7. Networking

The network is created by `docker compose up` — declared, not hand-made:

```yaml
networks:
  hotel-booking-net:
    name: hotel-booking-net
    driver: bridge
    ipam:
      config:
        - subnet: 172.29.0.0/16
```

Inspect:

```powershell
docker network ls
docker network inspect hotel-booking-net
docker network inspect hotel-booking-net --format '{{range .Containers}}{{.Name}} {{.IPv4Address}}{{"\n"}}{{end}}'
```

### Visibility

**Inside the network** — service name as hostname, container port:

| target       | resolves to | note                        |
| ------------ | ----------- | --------------------------- |
| `mysql:3306` | 172.29.0.2  | any network member          |
| `redis:6379` | 172.29.0.3  | any network member          |

**From the host** — only via published ports:

| service      | host address     | container |
| ------------ | ---------------- | --------- |
| mysql        | `127.0.0.1:3306` | 3306      |
| redis        | `127.0.0.1:6379` | 6379      |
| cloudbeaver  | `127.0.0.1:8979` | 8978      |
| redisinsight | `127.0.0.1:5541` | 5540      |

Both tool UIs are bound to `127.0.0.1` explicitly, so they are unreachable from the LAN. Do not drop that prefix on a shared or public network: an unbound CloudBeaver exposes a full SQL client, with saved credentials, to anyone who can route to the host.

Test from inside:

```powershell
docker run --rm --network hotel-booking-net busybox:1.37 nslookup mysql
docker run --rm --network hotel-booking-net busybox:1.37 nc -zv mysql 3306
docker run --rm --network hotel-booking-net busybox:1.37 nc -zv redis 6379
docker compose exec -T cloudbeaver getent hosts mysql
```

### Attach another container / project

```powershell
docker network connect hotel-booking-net <container>       # attach a running container
docker network disconnect hotel-booking-net <container>
docker run -d --name api --network hotel-booking-net my/api:1.0
```

From another compose file:

```yaml
services:
  api:
    networks: [hotel-booking-net]
    environment:
      DB_HOST: mysql
      REDIS_HOST: redis
networks:
  hotel-booking-net:
    external: true
```

`external: true` is required — without it Docker creates a second, isolated network and `mysql` will not resolve.

---

## 8. Remove containers, images, volumes, network

Ordered least → most destructive.

### Containers

```powershell
docker compose down                              # this project's containers
docker compose --profile tools down              # incl. cloudbeaver + redisinsight
docker compose rm -f cloudbeaver                 # one stopped service container
docker rm -f hbs-cloudbeaver                     # by container name, force
docker container prune -f                        # ALL stopped containers, machine-wide
```

Warning: every `prune` command in this section acts on the whole Docker host, not just this project. Other stacks on this machine (`karbon-*`, `db-migration-*`) are in scope and will be removed too. Prefer the `docker compose` forms.

### Images

```powershell
docker compose images                            # images this project uses
docker rmi hbs/mysql:8.4.6                       # the locally built mysql image
docker rmi dbeaver/cloudbeaver:25.2.3
docker rmi -f <image>                            # force, even if a stopped container references it
docker image prune -f                            # dangling (untagged) images only
docker image prune -a -f                         # ALL unused images, machine-wide
```

`docker rmi` fails with `image is being used by stopped container` until the container is gone — run `docker compose down` first, or add `-f`.

Rebuild after removing:

```powershell
docker compose build --no-cache mysql
docker compose up -d
```

### Volumes

Warning: removing a volume permanently deletes its data. `hbs-mysql-data` holds every hotel, room, booking, and payment row; `hbs-redis-data` holds the AOF file. There is no undo and no recycle bin. Run `.\scripts\hbs.ps1 backup` first.

```powershell
docker volume ls --filter name=hbs-
docker compose down -v                           # containers + ALL project volumes
docker volume rm hbs-mysql-data                  # one volume (container must be gone first)
docker volume rm hbs-cloudbeaver-workspace       # safe: UI state only, no DB data
docker volume prune -f                           # unused volumes, machine-wide
```

### Network

```powershell
docker network rm hotel-booking-net              # fails while containers are attached
docker network prune -f                          # unused networks, machine-wide
```

`docker compose down` removes the network once its own containers are gone. It refuses while a foreign container is still attached — disconnect it first (section 7).

### Full reset of this project

Warning: this destroys all database and cache data for this project. Back up first.

```powershell
.\scripts\hbs.ps1 backup
docker compose --profile tools down -v
docker rmi hbs/mysql:8.4.6
docker compose build mysql
docker compose up -d
```

Guarded shortcut (prompts for `yes`): `.\scripts\hbs.ps1 nuke` / `make nuke`

---

## 9. Troubleshooting

| symptom                                                           | cause → fix                                                                                     |
| ----------------------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| `Bind for 0.0.0.0:<port> failed: port is already allocated`        | another container holds it → `docker ps --format '{{.Names}} {{.Ports}}'`, change port in `.env` |
| `unknown variable 'default-authentication-plugin'`                 | removed in MySQL 8.4 → use `--authentication-policy`                                            |
| `World-writable config file '/etc/mysql/conf.d/my.cnf' is ignored` | bind-mounted `.cnf` from NTFS → already fixed by `infra/mysql/Dockerfile`                        |
| `ERROR 1142 ... command denied to user 'hbs_app'`                  | by design — `hbs_app` has no DDL. Use the root connection.                                      |
| `ERROR 1062 Duplicate entry '<room>-<date>'`                       | overbooking guard fired on `room_night_allocations` → return 409                                 |
| `NOAUTH Authentication required`                                   | `redis-cli` without `-a` → pass the secret                                                      |
| `ERR unknown command 'CONFIG'`                                     | renamed out in `redis.conf` for hardening                                                        |
| initdb scripts never ran                                           | volume was not empty → `docker volume rm hbs-mysql-data` (destroys data), then `up -d`            |
| CloudBeaver shows no connections                                   | workspace volume pre-dates the seed file → remove `hbs-cloudbeaver-workspace`, recreate          |
