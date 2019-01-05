@ECHO OFF

CALL git fetch https://github.com/YilianSource/brackeys-bot.git

SET UPSTREAM=%1

git rev-parse @ > local.txt
SET /P LOCAL=<local.txt
DEL local.txt

git rev-parse %UPSTREAM% > remote.txt
SET /P REMOTE=<remote.txt
DEL remote.txt

git merge-base @ %UPSTREAM% > base.txt
SET /P BASE=<base.txt
DEL base.txt

REM TODO: Make this an if-else
if %LOCAL% == %REMOTE% (ECHO Up-to-date)
if %LOCAL% == %BASE% (ECHO Need to pull)