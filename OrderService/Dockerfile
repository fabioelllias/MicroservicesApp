FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Contracts/Contracts.csproj ./Contracts/
COPY OrderService/OrderService.csproj ./OrderService/

RUN dotnet restore ./OrderService/OrderService.csproj

COPY Contracts ./Contracts
COPY OrderService ./OrderService

WORKDIR /src/OrderService
RUN dotnet publish -c Debug -o /app /p:DebugSymbols=true /p:DebugType=portable

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

COPY --from=build /app .

RUN apt-get update && apt-get install -y unzip curl \
 && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

ENTRYPOINT ["dotnet", "OrderService.dll"]
