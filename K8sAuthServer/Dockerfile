FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY K8sAuthServer.csproj .
RUN dotnet restore

# Copy the rest of the source code and build
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime Stage ----
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published output from build stage
COPY --from=build /app/publish .

# Set environment variable for Kestrel (optional, default port 8080)
ENV ASPNETCORE_URLS=http://+:8080

# Expose port
EXPOSE 8080

# Entrypoint
ENTRYPOINT ["dotnet", "K8sAuthServer.dll"]