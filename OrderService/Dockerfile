FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Contracts/Contracts.csproj ./Contracts/
COPY OrderService/OrderService.csproj ./OrderService/

RUN dotnet restore ./OrderService/OrderService.csproj

COPY Contracts ./Contracts
COPY OrderService ./OrderService

WORKDIR /src/OrderService
RUN dotnet publish -c Debug -o /app /p:DebugSymbols=true /p:DebugType=portable

# Fase final
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

COPY --from=build /app .

# 🧰 Instalações auxiliares (já existentes)
RUN apt-get update && apt-get install -y unzip curl \
 && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

# ✅ Healthcheck: espera que o SEQ esteja acessível
HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=3 \
  CMD curl --silent --fail http://seq:80 || exit 1

ENTRYPOINT ["dotnet", "OrderService.dll"]
