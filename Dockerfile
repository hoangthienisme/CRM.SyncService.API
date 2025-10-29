# =============================
# STAGE 1: BUILD
# =============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj vào container
COPY ./CRM.SyncService.API/CRM.SyncService.API.csproj ./CRM.SyncService.API/

# Restore dependencies
RUN dotnet restore "CRM.SyncService.API/CRM.SyncService.API.csproj"

# Copy toàn bộ source code
COPY . .

# Build & publish ra thư mục /app/publish
WORKDIR /src/CRM.SyncService.API
RUN dotnet publish -c Release -o /app/publish

# =============================
# STAGE 2: RUNTIME
# =============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CRM.SyncService.API.dll"]
