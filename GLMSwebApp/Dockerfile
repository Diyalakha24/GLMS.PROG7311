# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["GLMSwebApp/GLMS.Web.csproj", "GLMSwebApp/"]
RUN dotnet restore "GLMSwebApp/GLMS.Web.csproj"

# Copy all source and publish
COPY . .
WORKDIR "/src/GLMSwebApp"
RUN dotnet publish "GLMS.Web.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Create uploads directory so PDF uploads work inside the container
RUN mkdir -p /app/wwwroot/uploads

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "GLMS.Web.dll"]