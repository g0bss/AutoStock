FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy solution and project files first for better caching
COPY *.sln ./
COPY src/ src/

# Restore only the production projects (exclude tests)
RUN dotnet restore "src/DealershipInventorySystem.WebAPI/DealershipInventorySystem.WebAPI.csproj"

# Publish the WebAPI project
RUN dotnet publish "src/DealershipInventorySystem.WebAPI/DealershipInventorySystem.WebAPI.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy published application files
COPY --from=build /app/publish .

# Explicitly copy wwwroot static files to ensure they are included
COPY --from=build /source/src/DealershipInventorySystem.WebAPI/wwwroot ./wwwroot

ENTRYPOINT ["dotnet", "DealershipInventorySystem.WebAPI.dll"]