#!/bin/bash
cd /app/Server || exit
dotnet publish -c Release --self-contained true -r linux-x64 "$1" -o dist