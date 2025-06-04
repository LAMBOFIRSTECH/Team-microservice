## Faire du multi-stage
## User dédié
## Analyse trivy de l'image pour nettoyer les vulnérabilités

# Image de base pour l'exécution
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8181
# Phase de préparation (SDK) - Utiliser l'image SDK pour la publication
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
# Copier les sources de l'application
COPY . . 
# Restaurer les dépendances
RUN dotnet restore
# Publier l'application en mode Release et mettre les artefacts dans /app
RUN dotnet publish teams.sln --configuration Release --output /app
# Phase finale (runtime)
FROM base AS final
WORKDIR /app
# Copier les fichiers publiés depuis la phase de build
COPY --from=build /app . 
# Copier les fichiers de configuration nécessaires
COPY teams/appsettings.* . 

# Variables d'environnement
ENV ASPNETCORE_URLS=$ASPNETCORE_URLS
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/etc/ssl/certs/localhost.pfx
# Point d'entrée de l'application
ENTRYPOINT ["dotnet", "/app/teams.dll"]