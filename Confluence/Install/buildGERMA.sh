#!/bin/bash
sudo docker build -f DOCKERFILE --no-cache -t germa:live .
read -r -p "Database Password: " dbPassword 
sudo docker rm -f germa-live
sudo docker run \
  -d -p 8443:443 \
  --name germa-live \
  --env DATABASE_CONNECTION="Host=172.19.0.50:5432;Database=germa;Username=postgres;Password=$dbPassword" \
  --restart always \
  -v /etc/letsencrypt:/etc/letsencrypt:ro \
  --ip 172.19.0.51 \
  --network db-network \
germa:live