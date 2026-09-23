# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy the entire solution
COPY . .

# Restore the API project and its project references
RUN dotnet restore "EnterpriseTaskManagement.API/EnterpriseTaskManagement.API.csproj"

# Build and publish the API
RUN dotnet publish "EnterpriseTaskManagement.API/EnterpriseTaskManagement.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# ASP.NET Core will listen on port 8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EnterpriseTaskManagement.API.dll"]