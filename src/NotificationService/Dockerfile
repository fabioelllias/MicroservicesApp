FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Contracts/Contracts.csproj ./Contracts/
COPY NotificationService/NotificationService.csproj ./NotificationService/

RUN dotnet restore ./NotificationService/NotificationService.csproj

COPY Contracts ./Contracts
COPY NotificationService ./NotificationService

WORKDIR /src/NotificationService
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app .

EXPOSE 80

HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=3 \
  CMD curl --silent --fail http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "NotificationService.dll"]