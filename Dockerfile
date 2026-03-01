# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file
COPY ["MentalHealthSupport.csproj", "."]

# Restore dependencies
RUN dotnet restore "MentalHealthSupport.csproj"

# Copy source code
COPY . .

# Build the application
RUN dotnet build "MentalHealthSupport.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MentalHealthSupport.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 80
EXPOSE 443

# Set environment
ENV ASPNETCORE_URLS=http://+:80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

# Start application
ENTRYPOINT ["dotnet", "MentalHealthSupport.dll"]
