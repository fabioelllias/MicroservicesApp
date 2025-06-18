FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Contracts/Contracts.csproj ./Contracts/
COPY PaymentService/PaymentService.csproj ./PaymentService/

RUN dotnet restore ./PaymentService/PaymentService.csproj

COPY Contracts ./Contracts
COPY PaymentService ./PaymentService

WORKDIR /src/PaymentService
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app .

EXPOSE 80

HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=3 \
  CMD curl --silent --fail http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "PaymentService.dll"]