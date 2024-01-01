#!/bin/sh

cd src
cd asg.identity.data.migrator
dotnet build ./asg.identity.data.migrator.csproj
dotnet ./bin/Debug/net7.0/asg.identity.data.migrator.dll -migrate