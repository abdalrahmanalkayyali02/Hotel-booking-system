SHELL := /bin/sh
DC    := docker compose
DB    ?= hotel_booking
TS    := $(shell date -u +%Y%m%dT%H%M%SZ)

.DEFAULT_GOAL := help
.PHONY: help init secrets up up-tools down stop restart ps logs health mysql redis backup restore nuke

help: ## show targets
	@grep -hE '^[a-zA-Z_-]+:.*?## ' $(MAKEFILE_LIST) | awk 'BEGIN{FS=":.*?## "}{printf "  \033[36m%-12s\033[0m %s\n",$$1,$$2}'

init: secrets ## first-run: secrets + .env + start stack
	@[ -f .env ] || cp .env.example .env
	@$(MAKE) up

secrets: ## generate infra/secrets/*.txt (idempotent)
	@sh scripts/gen-secrets.sh

up: ## start mysql + redis
	$(DC) up -d
	@$(MAKE) health

up-tools: ## start stack + cloudbeaver + redisinsight
	$(DC) --profile tools up -d

down: ## stop + remove containers (volumes kept)
	$(DC) down

stop: ## stop containers
	$(DC) stop

restart: down up ## down then up

ps: ## container status
	$(DC) ps

logs: ## tail all logs
	$(DC) logs -f --tail=100

health: ## wait for healthy services
	@echo "waiting for healthy..."
	@for i in $$(seq 1 60); do \
	  s=$$($(DC) ps --format '{{.Service}} {{.Health}}' | grep -c 'healthy' || true); \
	  [ "$$s" -ge 2 ] && echo "mysql + redis healthy" && exit 0; \
	  sleep 2; \
	done; echo "TIMEOUT - check: make logs"; exit 1

mysql: ## root mysql shell
	$(DC) exec mysql sh -c 'exec mysql -uroot -p"$$(cat /run/secrets/mysql_root_password)" $(DB)'

redis: ## authenticated redis-cli
	$(DC) exec redis sh -c 'exec redis-cli -a "$$(cat /run/secrets/redis_password)" --no-auth-warning'

backup: ## dump db to infra/backup/
	$(DC) exec -T mysql sh -c 'exec mysqldump -uroot -p"$$(cat /run/secrets/mysql_root_password)" \
	  --single-transaction --routines --triggers --events $(DB)' > infra/backup/$(DB)-$(TS).sql
	@echo "wrote infra/backup/$(DB)-$(TS).sql"

restore: ## restore FILE=path.sql
	@[ -n "$(FILE)" ] || { echo "usage: make restore FILE=infra/backup/x.sql"; exit 1; }
	$(DC) exec -T mysql sh -c 'exec mysql -uroot -p"$$(cat /run/secrets/mysql_root_password)" $(DB)' < $(FILE)

nuke: ## DESTRUCTIVE: remove containers + volumes (all data lost)
	@printf 'Delete ALL mysql + redis data? type yes: '; read a; [ "$$a" = yes ] || exit 1
	$(DC) down -v
