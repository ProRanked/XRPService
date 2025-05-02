# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["XRPService.csproj", "./"]
RUN dotnet restore "XRPService.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "XRPService.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "XRPService.csproj" -c Release -o /app/publish

# Runtime Stage - Ubuntu based
FROM ubuntu:22.04 AS final

# Install .NET Runtime dependencies
RUN apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        ca-certificates \
        libc6 \
        libgcc-s1 \
        libicu70 \
        libssl3 \
        libstdc++6 \
        tzdata \
        curl \
    && rm -rf /var/lib/apt/lists/*

# Install .NET 9.0 Runtime
ENV DOTNET_VERSION=9.0.0-preview.2.24128.5
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin -Channel 9.0 -Runtime aspnet -Version $DOTNET_VERSION -InstallDir /usr/share/dotnet \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

# Set working directory
WORKDIR /app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 80
EXPOSE 443

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Health check
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Set the entry point for the application
ENTRYPOINT ["dotnet", "XRPService.dll"]