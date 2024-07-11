# Changes for 1.3.x-alpha

## ERModsManager
<details>
<summary><h3>1.3.0-alpha</h3></summary>
<br>

- [x] Creation of the visual app
- [x] File(s) Drag & Drop
- [x] Arrange mod priority by dragging them up or down
- [x] Enable/Disable/Delete mod(s) via checkboxes and buttons
- [x] First launch scenario: extract embedded files to app data folder, ask user game path if not found
- [x] Config:
  - [x] Re-set game path (file dialog)
  - [x] Open local app data folder
- [x] Help & Credits
- [x] Save actives mods & arrangements (priorities) in config

</details>

<details open>
<summary><h3>1.3.1-alpha</h3></summary>
<br>

- [x] Config:
  - [ ] Custom location for ModsToMerge and MergedMods
- [ ] Enable / Disable all mods
- [ ] Expandable mods
  - [ ] Add / modify note
  - [ ] Show files / folders tree
    - [ ] Enable / Disable particular file / folder
	- [ ] Highlight red / orange when file conflict is found (red = not supported and will be overwritten, orange = supported and internal merge will occur)
- [ ] Re-Merge using precedent / saved merge(s)
  - [ ] Save the content of MergedMods in SavesMergedMods after a merge and add in it a json object that relates merge details (mods, conflicts)
- [ ] Profiles
  - [ ] Dropdown list for loading or add new merge / config profiles
  - [ ] Inform user when he try to launch the game before the selected profile is ready (not merged)
- [ ] Better logs
  - [ ] Logs presented in a list instead of textblock
  - [ ] Colorized icons for different log types
  - [ ] Progress bar log type
  - [ ] Expandable logs (logs groups per merging file type)
  


### Fixes

- [ ] Crash when local files / folders are modified / deleted
  - [ ] Check files / folders at app launch (also before edits events) and update concerned config / profile accordingly

</details>


## ERModsMerger.Core

<details>
<summary><h3>1.3.0-alpha</h3></summary>
<br>

- [x] Searches of unsuported conflicts + ask user confirmation to continue
- [x] Initial support for the merge of .msgbnd.dcx files
- [x] Partial support for the merge of .emevd.dcx files

- [x] Better implementation for logs and user / console queries
- [x] Read content packed files in game folder instead of unpacking everything by using [BHD5Reader, thanks to Nordgaren](https://github.com/Nordgaren/ERBingoRandomizer/blob/main/src/ERBingoRandomizer/FileHandler/BHD5Reader.cs)
- [x] handle oodle location in SoulsFormats depending of scenarios (console / WPF app)
- [x] UnPack/UnZip embedded resources to AppData
  - [x] Regulations
  - [x] Dictionaries
  - [x] ParamDefs
  - [x] Pre-configured modengine as embedded resource
  
</details>

<details open>
<summary><h3>1.3.1-alpha</h3></summary>
<br>

- [x] Better implementation for logs and user / console queries
  - [ ] Logs grouping
- [ ] Internal events instructions conflicts merges
- [ ] Detect & Handle .dll mods, editing modengine config_eldenring.toml accordingly
  - [ ] ini integration
- [x] (UnZip mods and) search in arborescences, align to correct paths using [EldenRingDictonnary.txt, thanks to Nordgaren again](https://github.com/Nordgaren/UXM-Selective-Unpack/blob/master/UXM/res/EldenRingDictionary.txt)
  - [ ] Ask user when multiple versions (duplicates) is found
  - [ ] Ignore files that are not present in dictionary (readme, json project files, etc)
- [ ] .csv merge support
- [ ] Upgrade old regulation bin files using stored reg bin vanilla file of the same version
- [ ] Better implementation for conflict merging
  - [ ] Load and check conflicts for each files, when conflict is found add it to a class object
  - [ ] When all files are loaded and checked, apply modifications


### Fixes

- [ ] Vanilla dcx not loading

</details>