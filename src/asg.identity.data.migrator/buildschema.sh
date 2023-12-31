#!/bin/bash

rm -rf Migrations

dotnet ef migrations add Grants -c PersistedGrantDbContext -o Migrations/PersistedGrantDb -- -ef
dotnet ef migrations add Configuration -c ConfigurationDbContext -o Migrations/ConfigurationDb -- -ef
dotnet ef migrations add Users -c ApplicationDbContext -o Migrations/ApplicationDb -- -ef

# dotnet ef migrations script -c PersistedGrantDbContext -o Migrations/PersistedGrantDb.sql
# dotnet ef migrations script -c ConfigurationDbContext -o Migrations/ConfigurationDb.sql
