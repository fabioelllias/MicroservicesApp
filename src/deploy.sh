#!/bin/bash

set -e

# Variáveis
IMAGE_NAME="fabioellias/orderservice"
TAG="latest"
DOCKERFILE_PATH="OrderService/Dockerfile"
RESOURCE_GROUP="microservices-rg"
CONTAINERAPP_NAME="orderservice"

echo "📦 Build da imagem Docker..."
docker build -t ${IMAGE_NAME}:${TAG} -f ${DOCKERFILE_PATH} .

echo "📤 Push da imagem para Docker Hub..."
docker push ${IMAGE_NAME}:${TAG}

echo "🚀 Atualizando o Azure Container App..."
az containerapp update \
  --name ${CONTAINERAPP_NAME} \
  --resource-group ${RESOURCE_GROUP} \
  --image ${IMAGE_NAME}:${TAG}

echo "✅ Deploy concluído com sucesso!"
