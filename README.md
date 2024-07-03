# ERModsMerger
Simple tool to merge Elden Ring mods.

NOTE: This tool just got released and still under testing. Thanks to report if you have issues.
## Usage
Highly recommended: Use [ModEngine2](https://github.com/soulsmods/ModEngine2), place ERModsMerger.exe in the same folder and edit config_eldenring.toml as follow:
```
mods = [
    { enabled = true, name = "default", path = "MergedMods" }
]
```
Launch ERModsMerger.exe and let it guide you through the process, it will self extract and create all the folders you need, it will also give you an example and explanations to make it easy.

## Troubleshooting & Solutions

- ERModsMerger.exe don't launch: Make sure [.Net 8 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.6-windows-x64-installer) is installed on your system.

- Fail to load config or could not locate regulation bin: This likely happen when ERModsMergerConfig\\config.json is modified with invalid values / format. Here is an example of a modified config.json:

	```json
	{
	  "GamePath": "C:\\New\\path\\to the folder of\\ELDEN RING\\Game",
	  "ModsToMergeFolderPath": "ModsToMerge",
	  "MergedModsFolderPath": "MergedMods"
	}
	```
  * Respect the format presented above and dont forget to add double `\\` between each folders in paths.
  

- Vanilla game/modded regulation.bin don't load: The app might be not compatible with this regulation.bin, make sure your game/mods are up to date (working regulation version is 1.12.2)

- Game don't launch, is buggy or mods are missing: Merges can cause some troubles depending of the overwrited files, also this tool only merge internal values of regulation.bin files (for now) and overwrite fields in benefits to the highest priority order. Any other individual conflicting files (eg: emevd.dcx anibnd.dcx msb.dcx etc..) will be overwrited using the same system of priority and potentially causing more troubles.

For now it's better to use this tool to merge mods who only have conflicting regulation.bin files and no .dcx individual files conflicts.

## Automation

Run this app with /merge argument to automatically merge mods located inside ModsToMerge to MergedMods folder, no user interaction will be asked and the console will close after the merge.﻿﻿

## Credits & Thanks
* [SoulsMods](https://github.com/soulsmods)
* [Smithbox](https://github.com/vawser/Smithbox)
* Also big Thanks to all the Souls [modding community](https://discord.gg/ZCxzcxp6)
