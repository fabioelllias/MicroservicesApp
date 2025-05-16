FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Contracts/Contracts.csproj ./Contracts/
COPY NotificationService/NotificationService.csproj ./NotificationService/
RUN dotnet restore ./NotificationService/NotificationService.csproj

COPY Contracts ./Contracts
COPY NotificationService ./NotificationService

WORKDIR /src/NotificationService
RUN dotnet publish -c Debug -o /app /p:DebugSymbols=true /p:DebugType=portable

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

COPY --from=build /app .

RUN apt-get update && apt-get install -y unzip curl \
 && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

ENTRYPOINT ["dotnet", "NotificationService.dll"]
