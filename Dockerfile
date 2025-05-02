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

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 80
EXPOSE 443

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

# Set the entry point for the application
ENTRYPOINT ["dotnet", "XRPService.dll"]