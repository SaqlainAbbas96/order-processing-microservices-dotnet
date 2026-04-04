# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and projects
COPY *.sln .
COPY Shared/ Shared/
COPY OrderService/ OrderService/

# Restore dependencies
RUN dotnet restore OrderService/OrderService.csproj

# Build
WORKDIR /src/OrderService
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderService.dll"]