# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and projects
COPY *.sln .
COPY Shared/ Shared/
COPY NotificationService/ NotificationService/

# Restore dependencies
RUN dotnet restore NotificationService/NotificationService.csproj

# Build
WORKDIR /src/NotificationService
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "NotificationService.dll"]