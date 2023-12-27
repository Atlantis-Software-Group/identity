#!/bin/sh

update-ca-certificates
dotnet build asg.data.migrator/asg.data.migrator.csproj
dotnet asg.data.migrator/bin/Debug/net7.0/asg.data.migrator.dll -migrate