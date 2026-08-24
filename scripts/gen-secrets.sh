#!/usr/bin/env sh
# Generate docker secrets if absent. Idempotent - never overwrites.
set -eu
DIR="$(CDPATH= cd -- "$(dirname -- "$0")/../infra/secrets" && pwd)"

rand() { LC_ALL=C tr -dc 'A-Za-z0-9' < /dev/urandom | head -c 32; }

for f in mysql_root_password mysql_app_password redis_password; do
  if [ -s "$DIR/$f.txt" ]; then
    echo "skip  $f.txt (exists)"
  else
    printf '%s' "$(rand)" > "$DIR/$f.txt"
    chmod 600 "$DIR/$f.txt"
    echo "wrote $f.txt"
  fi
done
