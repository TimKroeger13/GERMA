#!/bin/bash
sudo docker network create --driver=bridge --subnet=172.19.0.0/16 db-network
read -r -p "Database Password: " password
sudo docker run \
  -d -p 5433:5432 \
  --name postgres  \
  --env POSTGRES_PASSWORD="$password" \
  --restart always \
  --env PGDATA=/var/lib/postgresql/data/pgdata \
  -v /home/srv/postgres:/var/lib/postgresql/data \
  --ip 172.19.0.50 \
  --network db-network \
postgis/postgis