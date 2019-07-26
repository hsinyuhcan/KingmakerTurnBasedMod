# Turn-Based Combat Mod for Pathfinder: Kingmaker
## Download
https://www.nexusmods.com/pathfinderkingmaker/mods/109
## Compile
This project depends on [ModMaker](https://github.com/hsinyuhcan/KingmakerModMaker), you need both repos in the same folder, and a folder called `KingmakerLib` including the Dll files. The folder structure should look like:
```
Repos
│
├── KingmakerLib
│   ├── UnityModManager
│   │   ├── 0Harmony12.dll
│   │   └── UnityModManager.dll
│   └── *.dll
│
├── KingmakerModMaker
│   ├── ModMaker
│   │   └── ModMaker.shproj
│   └── ModMaker.sln
│
└── KingmakerTurnBasedMod
    ├── TurnBased
    │   └── TurnBased.csproj
    └── TurnBased.sln
```
