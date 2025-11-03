# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Create a completely clean build folder
WORKDIR /build

# Copy all project files
COPY src/ ./AuthForge/

WORKDIR /build/AuthForge/AuthForge.Api

# Restore dependencies
RUN dotnet restore

# Publish to a totally separate folder
RUN dotnet publish -c Release -o /publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /publish .
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 5255
EXPOSE 7217
ENTRYPOINT ["dotnet", "AuthForge.Api.dll"]
