#!/usr/bin/env bash
set -e

echo '🚀 1/3 : Création du projet Sandbox jetable...'
mkdir -p ScaffoldSandbox
cd ScaffoldSandbox
dotnet new classlib --force

# Spécifier la version 8.* pour correspondre au SDK .NET 8
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version "8.*"
dotnet add package Microsoft.EntityFrameworkCore.Design --version "8.*"

echo '🚀 2/3 : Scaffold EF Core (Tables + Vues)...'
dotnet ef dbcontext scaffold "$ConnectionStrings__DefaultConnection" \
  Npgsql.EntityFrameworkCore.PostgreSQL \
  -o Generated \
  -c LegacyDbContext \
  --no-onconfiguring \
  --schema teams \
  --force

echo '🚀 3/3 : Extraction SQL des Fonctions, Stored Procedures et Triggers...'
OUT_FILE="Generated/Procedures_and_Triggers.sql"

{
  echo '-- ============================================='
  echo '-- STORED PROCEDURES & FUNCTIONS (Schema: teams)'
  echo '-- ============================================='
  psql -h postgres -U admin -d teamsdb -t -A -c "
    SELECT pg_get_functiondef(p.oid) || ';' 
    FROM pg_proc p 
    JOIN pg_namespace n ON p.pronamespace = n.oid 
    WHERE n.nspname = 'teams';
  "
  echo ''
  echo '-- ============================================='
  echo '-- TRIGGERS (Schema: teams)'
  echo '-- ============================================='
  psql -h postgres -U admin -d teamsdb -t -A -c "
    SELECT pg_get_triggerdef(t.oid) || ';' 
    FROM pg_trigger t 
    JOIN pg_class c ON t.tgrelid = c.oid 
    JOIN pg_namespace n ON c.relnamespace = n.oid 
    WHERE n.nspname = 'teams' AND NOT t.tgisinternal;
  "
} > "$OUT_FILE"

echo '✅ Terminé ! Tout a été généré dans src/ScaffoldSandbox/Generated/'