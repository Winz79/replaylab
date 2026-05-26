# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and shared props for restore
COPY ReplayLab.sln .
COPY Directory.Build.props .
COPY global.json .

# Copy csproj files for restore layer caching
COPY src/ReplayLab.Web/ReplayLab.Web.csproj src/ReplayLab.Web/
COPY src/ReplayLab.Web.Hosting/ReplayLab.Web.Hosting.csproj src/ReplayLab.Web.Hosting/
COPY src/ReplayLab.Adapters.Mock/ReplayLab.Adapters.Mock.csproj src/ReplayLab.Adapters.Mock/
COPY src/ReplayLab.Core/ReplayLab.Core.csproj src/ReplayLab.Core/
COPY src/ReplayLab.Parsers.Csv/ReplayLab.Parsers.Csv.csproj src/ReplayLab.Parsers.Csv/

RUN dotnet restore src/ReplayLab.Web/ReplayLab.Web.csproj

# Copy all source
COPY src/ ./src/

# Build and publish
RUN dotnet publish src/ReplayLab.Web/ReplayLab.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5213
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5213

ENTRYPOINT ["dotnet", "ReplayLab.Web.dll"]
