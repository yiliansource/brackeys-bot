@ECHO OFF

CALL git pull https://github.com/YilianSource/brackeys-bot.git

ECHO Latest changes pulled from git

CALL dotnet build > nul