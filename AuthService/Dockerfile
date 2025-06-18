FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Contracts/Contracts.csproj ./Contracts/
COPY AuthService/AuthService.csproj ./AuthService/

RUN dotnet restore ./AuthService/AuthService.csproj

COPY Contracts ./Contracts
COPY AuthService ./AuthService

WORKDIR /src/AuthService
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app .

EXPOSE 80

HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=3 \
  CMD curl --silent --fail http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "AuthService.dll"]