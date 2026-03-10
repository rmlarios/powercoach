# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["CoachPlatform.sln", "."]
COPY ["src/CoachPlatform.Domain/CoachPlatform.Domain.csproj", "src/CoachPlatform.Domain/"]
COPY ["src/CoachPlatform.Application/CoachPlatform.Application.csproj", "src/CoachPlatform.Application/"]
COPY ["src/CoachPlatform.Infrastructure/CoachPlatform.Infrastructure.csproj", "src/CoachPlatform.Infrastructure/"]
COPY ["src/CoachPlatform.API/CoachPlatform.API.csproj", "src/CoachPlatform.API/"]

# Restore dependencies
RUN dotnet restore "src/CoachPlatform.API/CoachPlatform.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/CoachPlatform.API"
RUN dotnet build "CoachPlatform.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CoachPlatform.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup --gid 1000 appgroup && adduser --uid 1000 --gid 1000 --disabled-password --gecos "" appuser
USER appuser

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

EXPOSE 8080

ENTRYPOINT ["dotnet", "CoachPlatform.API.dll"]
