#!/bin/sh

dotnet run /seed #seed the db
update-ca-certificates #update the certificates for the container
dotnet watch ./bin/Debug/net7.0/asg.identity.dll