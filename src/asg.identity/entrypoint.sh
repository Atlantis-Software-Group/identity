#!/bin/sh

update-ca-certificates #update the certificates for the container
dotnet watch ./bin/Debug/net7.0/asg.identity.dll