#!/bin/sh

dotnet run /seed #seed the db
update-ca-certificates #update the certificates for the container
dotnet watch run --no-launch-profile