FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and csproj files
COPY FundWise.slnx ./
COPY src/FundWise.Domain/*.csproj ./src/FundWise.Domain/
COPY src/FundWise.Application/*.csproj ./src/FundWise.Application/
COPY src/FundWise.Infrastructure/*.csproj ./src/FundWise.Infrastructure/
COPY src/FundWise.Persistence/*.csproj ./src/FundWise.Persistence/
COPY src/FundWise.Shared/*.csproj ./src/FundWise.Shared/
COPY src/FundWise.Contracts/*.csproj ./src/FundWise.Contracts/
COPY src/FundWise.API/*.csproj ./src/FundWise.API/

RUN dotnet restore src/FundWise.API/FundWise.API.csproj

# Copy all source files
COPY src/ ./src/

# Build and publish
RUN dotnet publish src/FundWise.API/FundWise.API.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FundWise.API.dll"]
