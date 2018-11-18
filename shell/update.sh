# Updates the bot by killing the process, pulling from master and restarting the bot

kill $1

git pull origin master

echo "Update completed!"

cd ../../../
dotnet run

echo "New instance started."