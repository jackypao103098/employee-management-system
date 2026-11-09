# Build context: employee-management-dotnet/
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY Directory.Build.props .editorconfig ./
COPY .config/ .config/
COPY src/EmployeeManagement.Api/EmployeeManagement.Api.csproj src/EmployeeManagement.Api/
RUN dotnet restore src/EmployeeManagement.Api/EmployeeManagement.Api.csproj
COPY src/ src/
RUN dotnet publish src/EmployeeManagement.Api/EmployeeManagement.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM build AS migrate-build
RUN dotnet tool restore \
    && dotnet tool run dotnet-ef migrations bundle \
        --project src/EmployeeManagement.Api \
        --startup-project src/EmployeeManagement.Api \
        --configuration Release \
        --output /app/efbundle \
        --force

# Applies pending EF Core migrations. Build with: docker build --target migrate .
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migrate
LABEL org.opencontainers.image.source="https://github.com/jackypao103098/employee-management-system"
WORKDIR /app
COPY --from=migrate-build /app/efbundle ./efbundle
USER $APP_UID
ENTRYPOINT ["/bin/sh", "-c", "exec /app/efbundle --connection \"$MIGRATION_CONNECTION\""]

# Default target: the API.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
LABEL org.opencontainers.image.source="https://github.com/jackypao103098/employee-management-system"
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "EmployeeManagement.Api.dll"]
