#!/bin/sh

dotnet restore ./src/asg.identity/asg.identity.csproj
dotnet build ./src/asg.identity/asg.identity.csproj

cd src
cd asg.identity
dotnet watch ./bin/Debug/net7.0/asg.identity.dll