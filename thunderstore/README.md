# Rich Discord Presence
Rich Presence in Discord based on your ingame state.

## Features
* Rich Presence for local Discord application
* Set different messages for 
  - regular roaming
  - boss fighting
  - staying at home
  - sailing
  - being in dungeon
  - being at Trader
* Set biome specific roaming messages
* Set detailed state
  - server name and capacity
  - show or not Time elapsed
  - show singleplayer or multiplayer state (also being alone in multiplayer)
* Configure custom images for different states

## Messages
* When you are fighting a boss the details string will be like "Boss name: {random string from Fighting boss messages}"
* When you are in a dungeon the details string will be like "Dungeon name: {random string from Dungeon messages}"
* When you are roaming around the biome the details string will be like "Biome name: {random string from Roaming messages}"
* When you are on the ship the details string will be like "Biome name: {random string from Sailing messages}"
* When you are at your base the details string will be like "Biome name: {random string from Safe at home messages}"
* When you are near a Trader the details string will be like "Trader name: {random string from Trader messages}"

Boss names, biome names, trader names, dungeon names will be localized on the game language automatically.

You can set biome specific message for Roaming messages.

You can set different images to be shown when the condition is met.

## Setting up

### 1 - Get Application ID
At first you need Application ID. For that you need to create your own application at https://discord.com/developers/applications. Application name is crucial because it will be shown as name of the game you play.

Best practice is to set name "Valheim".

On the General Information you will find your APPLICATION ID. Copy it and set as ApplicationID in the mod settings.

### 2 - Upload application logo
Get the [Valheim logo file](https://github.com/shudnal/RichDiscordPresence/blob/master/thunderstore/nexus/Valheim%20logo.png) and upload it to Rich Presence -> Art Assets -> Add Image(s) with name "logo". The file name can differ if you need it as the default Large image is configurable on the mod settings.

### 3 - Activate Rich Presence
On the Discord client press the cog (User settings) near your name in left bottom part.

At "Activity Settings" check for "Display current activity as a status message" to be on.

### 4 - Basic settings
That's basically it. Now your local Discord app will change your Rich Presence while you are playing Valheim with this mod enabled.

You may want to localize the strings from the mod settings to your language. There is category "Messages" which contains details and state strings. Each message settings is Semicolon separated string.

You can use localization key strings like "$ancient_chest". They will be localized on the game language automatically. 
See more localization keys at https://valheim-modding.github.io/Jotunn/data/localization/translations/English.html

## Default and custom images
https://discord.com/developers/docs/rich-presence/how-to#updating-presence-update-presence-payload-fields

The mod have default images keys and texts configurable.

There are Large image and Small image keys can be set to show different images based on current ingame state.

To customize images you should use "Large images descriptions" and "Small images descriptions" settings.

Both settings consists of semicolon separated list of "key-text" strings.

### The key format
The key format is the first part (before ":") of details string with spaces replaced by "_". 

In English for boss Elder("$enemy_gdking") name "The Elder" the file name should be "the_elder". For boss Queen("$enemy_seekerqueen") localized name "The Queen" the file name should be "the_queen".

It works the same for biome names, dungeon names and trader names.

For example when you go to black forest chambers. Their nonlocalized name is "$location_forestcrypt", in English "Burial chambers" and the file name should be "burial_chambers".

### The text format
Settings string will be split to separate strings by semicolon character. After that string should have format key-text. 

This string can contain more that one "-" character as it will be split only by first one.

### Example
Imagine you want custom image when fighting boss Elder instead of Valheim logo and custom hover text for that image. 

The key for the Elder will be "the_elder". So upload an imageto "Art Assets" on your application settings, name it "the_elder".
Then you can set "Large images descriptions" setting to "the_elder-The Elder".

If you want Queen boss to then upload file named "the_queen" and extend "Large images descriptions" to "the_elder-The Elder;the_queen-The Queen".

### Small images description
For small images the second part (after ":") of details string is used. The key and text format have the same rules: key is lowercase, replaced " " by "_".

## Installation (manual)
extract RichDiscordPresence\ folder to your BepInEx\Plugins\ folder.
You can place it in another folder just check RichDiscordPresence.dll and discord-rpc.dll are next to each other.

## Known issues
* Linux is not supported

## Configurating
The best way to handle configs is configuration manager. Choose one that works for you:

https://www.nexusmods.com/site/mods/529

https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/

## Mirrors

[Nexus](https://www.nexusmods.com/valheim/mods/2555)

## Changelog

v 1.0.3
* got rid of steam id entirely

v 1.0.2
* non steam PC version support

v 1.0.1
* option to localize boss, biome and location names on selected language

v 1.0.0
* Initial release