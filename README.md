# BrackeysBot [![Build Status](https://travis-ci.com/YilianSource/brackeys-bot.svg?branch=master)](https://travis-ci.com/YilianSource/brackeys-bot) [![Discord Server](https://discordapp.com/api/guilds/243005537342586880/widget.png)](https://discord.gg/brackeys)

The official Brackeys discord bot.

The repository contains all you need to run the bot, except for the `appsettings.json` file.
To be able to run the bot, add a new file to the folder before building.

```json
{
  "prefix": "[]",
  "token": "<your token here>"
}
```

The `[]manual` and `[]scriptapi` commands for searching the Unity Documentation uses the generator (written in D) from [CodeMyst](https://github.com/CodeMyst) found here: https://github.com/CodeMyst/UnityDocumentationGenerator

If you want to report a bug or suggest an improvement or a feature request, feel free to open an issue.
