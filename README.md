# BrackeysBot [![Build Status](https://travis-ci.com/YilianSource/brackeys-bot.svg?branch=master)](https://travis-ci.com/YilianSource/brackeys-bot) [![Discord Server](https://discordapp.com/api/guilds/243005537342586880/widget.png)](https://discord.gg/brackeys) [![Codacy Badge](https://api.codacy.com/project/badge/Grade/3d07c5e9ff454e998a4d5da4c591465b)](https://www.codacy.com/app/YilianSource/brackeys-bot?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=YilianSource/brackeys-bot&amp;utm_campaign=Badge_Grade)

The official Brackeys Discord bot.

The repository contains all you need to run the bot, except for the `appsettings.json` file.
To be able to run the bot, add a new file to the folder before building.

```json
{
  "prefix": "[]",
  "token": "<your token here>"
}
```

The `[]manual` and `[]scriptapi` commands for searching the Unity Documentation uses the generator (written in D) from [CodeMyst](https://github.com/CodeMyst) found [here](https://github.com/CodeMyst/UnityDocumentationGenerator).

When run, the bot will attempt to log certain events inside of daily logfiles, to backtrack errors.

If you want to report a bug or suggest an improvement or a feature request, feel free to open an issue.
