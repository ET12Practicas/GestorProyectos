# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GestorProyecto.slnx ./
COPY Aplicacion/Aplicacion.csproj Aplicacion/
COPY Dominio/Dominio.csproj Dominio/
COPY Persistencia/Persistencia.csproj Persistencia/
COPY Presentacion/Presentacion.csproj Presentacion/
RUN dotnet restore GestorProyecto.slnx

COPY . .
ARG VERSION=1.0.0
RUN dotnet publish Presentacion/Presentacion.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false \
    -p:Version="$VERSION"

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Presentacion.dll"]
