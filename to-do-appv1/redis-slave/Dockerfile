FROM redis:alpine3.16
MAINTAINER Johannes M. Scheuermann <johannes.scheuermann@inovex.de>

COPY start-redis-slave.sh /start-redis-slave.sh
RUN chmod +x /start-redis-slave.sh
CMD /start-redis-slave.sh
