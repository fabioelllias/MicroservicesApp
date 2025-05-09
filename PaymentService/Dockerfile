FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Contracts/Contracts.csproj ./Contracts/
COPY PaymentService/PaymentService.csproj ./PaymentService/

RUN dotnet restore ./PaymentService/PaymentService.csproj

COPY Contracts ./Contracts
COPY PaymentService ./PaymentService

WORKDIR /src/PaymentService
RUN dotnet publish -c Debug -o /app /p:DebugSymbols=true /p:DebugType=portable

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

COPY --from=build /app .

RUN apt-get update && apt-get install -y unzip curl \
 && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

ENTRYPOINT ["dotnet", "PaymentService.dll"]
