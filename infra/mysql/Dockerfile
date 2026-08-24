# Baked config: bind-mounted .cnf from a Windows/NTFS host is world-writable,
# and mysqld silently ignores world-writable config files. COPY fixes perms.
FROM mysql:8.4.6

COPY conf.d/my.cnf /etc/mysql/conf.d/my.cnf

USER root
RUN chmod 0444 /etc/mysql/conf.d/my.cnf && chown root:root /etc/mysql/conf.d/my.cnf
USER mysql
