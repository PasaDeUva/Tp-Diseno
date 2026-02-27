# Use the official .NET 8 SDK as a build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the csproj and restore any dependencies (via NuGet)
COPY ["TP-DisenoDB.csproj", "./"]
RUN dotnet restore

# Copy the rest of the project files and build our release
COPY . ./
RUN dotnet publish -c Release -o out

# Generate runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "TP-DisenoDB.dll"]
