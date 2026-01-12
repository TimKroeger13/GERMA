#!/bin/sh

set -e

az acr login -n gsdatalakecr

TAG="latest"
NS="germa"

docker build -t gsdatalakecr.azurecr.io/gs-datalake/germa-backend-stage:${TAG} -f ./backend.Dockerfile . --no-cache
docker image push gsdatalakecr.azurecr.io/gs-datalake/germa-backend-stage:${TAG} 
# kubectl rollout restart deployment datalake-management -n ${NS}