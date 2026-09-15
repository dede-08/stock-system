# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["api-gestion-productos.csproj", "./"]
RUN dotnet restore "api-gestion-productos.csproj"
COPY . .
RUN dotnet publish "api-gestion-productos.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Railway/Render/Fly asignan PORT; respeta $PORT si existe.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "api-gestion-productos.dll"]
