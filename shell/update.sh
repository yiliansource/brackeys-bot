#!/bin/sh

# Updates the bot by killing the process, pulling from master and restarting the bot
# The first argument should be the pid of the bot application

echo "Updating..."

kill $1

git pull https://github.com/YilianSource/brackeys-bot.git

echo "Update completed!"

dotnet run