#!/bin/sh

git pull https://github.com/YilianSource/brackeys-bot.git

echo "Latest changes pulled from git"

dotnet build > /dev/null