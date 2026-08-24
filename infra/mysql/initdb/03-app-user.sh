#!/usr/bin/env bash
# Create the app user with least privilege.
# MYSQL_USER is deliberately NOT set in compose: the entrypoint would grant it
# ALL PRIVILEGES on the schema, and that grant cannot be cleanly revoked
# (escaped-identifier mismatch on `db\_name`). Create it here instead.
set -euo pipefail

ROOT_PW="$(cat /run/secrets/mysql_root_password)"
APP_PW="$(cat /run/secrets/mysql_app_password)"
APP_USER="${APP_DB_USER:?APP_DB_USER not set}"
DB="${MYSQL_DATABASE:?MYSQL_DATABASE not set}"

mysql -uroot -p"$ROOT_PW" <<SQL
CREATE USER IF NOT EXISTS '${APP_USER}'@'%'
  IDENTIFIED WITH caching_sha2_password BY '${APP_PW}';
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON \`${DB}\`.* TO '${APP_USER}'@'%';
FLUSH PRIVILEGES;
SQL

echo "app user ${APP_USER}@% created with DML-only privileges on ${DB}"
