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

# Health check using bash built-in /dev/tcp (no curl/wget needed)
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD bash -c "exec 3<>/dev/tcp/127.0.0.1/8080; printf 'GET /health HTTP/1.0\r\nHost: localhost\r\n\r\n' >&3; grep -q Healthy <&3" || exit 1

EXPOSE 8080

ENTRYPOINT ["dotnet", "CoachPlatform.API.dll"]
