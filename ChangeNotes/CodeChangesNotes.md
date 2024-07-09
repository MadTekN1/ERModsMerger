# Changes for 1.3.x-alpha

## ERModsManager

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


## ERModsMerger.Core

- [x] Searches of unsuported conflicts + ask user confirmation to continue
- [x] Initial support for the merge of .msgbnd.dcx files
- [x] Partial support for the merge of .emevd.dcx files
  - [ ] Internal events instructions conflicts merges

- [x] Better Implementation for logs and user / console queries
- [x] Read content packed files in game folder instead of unpacking everything by using [BHD5Reader, thanks to Nordgaren](https://github.com/Nordgaren/ERBingoRandomizer/blob/main/src/ERBingoRandomizer/FileHandler/BHD5Reader.cs)
- [x] handle oodle location in SoulsFormats depending of scenarios (console / WPF app)
- [x] UnPack/UnZip embedded resources to AppData
  - [x] Regulations
  - [x] Dictionaries
  - [x] ParamDefs
  - [x] Pre-configured modengine as embedded resource
- [ ] Detect & Handle .dll mods, editing modengine config_eldenring.toml accordingly
- [x] (UnZip mods and) search in arborescences, align to correct paths using [EldenRingDictonnary.txt, thanks to Nordgaren again](https://github.com/Nordgaren/UXM-Selective-Unpack/blob/master/UXM/res/EldenRingDictionary.txt), ask user when multiple versions is found and delete unnecessary files
