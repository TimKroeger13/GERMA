#!/bin/bash
sudo docker build -f Dockerfile --no-cache -t germag:live .
read -r -p "Database Password: " dbPassword 
sudo docker rm -f germag-live
sudo docker run \
  -d -p 8443:443 \
  --name germag-live \
  --env DATABASE_CONNECTION="Host=172.19.0.50:5432;Database=germa;Username=postgres;Password=$dbPassword" \
  --restart always \
  -v /home/certs:/root/certs:ro \
  --ip 172.19.0.51 \
  --network db-network \
germa:live