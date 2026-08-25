# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as a separate layer (better caching)
COPY ["GLMS.API/GLMS.API.csproj", "GLMS.API/"]
RUN dotnet restore "GLMS.API/GLMS.API.csproj"

# Copy all source and publish
COPY . .
WORKDIR "/src/GLMS.API"
RUN dotnet publish "GLMS.API.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl so the healthcheck works
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "GLMS.API.dll"]