#!/bin/bash

set -e

# Variáveis
IMAGE_NAME="fabioellias/notificationservice"
TAG="latest"
DOCKERFILE_PATH="NotificationService/Dockerfile"
RESOURCE_GROUP="microservices-rg"
CONTAINERAPP_NAME="notificationservice"
REVISION_SUFFIX="rev-$(date +%s)" # timestamp único

echo "🕒 Nova tag gerada: $TAG"

echo "📦 Build da imagem Docker..."
docker build -t ${IMAGE_NAME}:${TAG} -f ${DOCKERFILE_PATH} .

echo "📤 Push da imagem para Docker Hub..."
docker push ${IMAGE_NAME}:${TAG}

echo "🚀 Atualizando o Azure Container App..."
az containerapp update \
  --name ${CONTAINERAPP_NAME} \
  --resource-group ${RESOURCE_GROUP} \
  --image ${IMAGE_NAME}:${TAG} \
  --revision-suffix ${REVISION_SUFFIX}

echo "✅ Deploy concluído com sucesso!"
