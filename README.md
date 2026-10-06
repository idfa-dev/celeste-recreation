# Celeste Recreation

This is a reconstruction of my old 2D platformer project, made with C# and MonoGame. I started it to practice building game features like movement, jumping, dashing, climbing, animation, menu, tile-based levels, permanent data handling and basically everything game related, using Celeste as my inspiration and my goal.

This is an independent project, created solely by me without the use of AI of any form. It is not an official Celeste game and is not affiliated with the original game's creators.

The only use of AI was my use of Copilot, wherein I simply made it take my pre-existing code out of an old Microsoft Word file, and paste it into Visual Studio, as it would've been hell for me to manually format it and replace indentation and make it readable  otherwise. All code is human generated, solely by me, AI simply restored it's original state and did not provide any code of its own.

## Why the original assets aren't included

The art, music, fonts, and other assets from Celeste are copyrighted by their respective rights holders, so I can't share them in this repository. The project code refers to some assets I used while working on it, but those files are not included. To run the game, you'll need to provide your own assets or replacements that you have permission to use.

## What works so far

The project has a MonoGame game loop, a player controller with movement and animation states, main and options menus, and basic map and level classes. It is still unfinished: some systems are placeholders, and the required content assets are missing ( for legal reasons )

## Requirements

- Windows
- .NET 9 SDK


## Build and run

From the repository root:

```powershell
dotnet tool restore --tool-manifest .\Celes\.config\dotnet-tools.json
dotnet build .\Celes\Celes.csproj
dotnet run --project .\Celes\Celes.csproj
```

The project builds, but it currently fails at startup because the `Renogare` font asset is missing, which I would add but like since all the other assets can't be added whats the point.

## Still to do

- Nothing, this project is effectively dead unless I create my own new assets to replace the formerly used Celeste assets

I may revisit it sometime however I'm currently working on training AI models.

### Add game content

If you try to run it, you'll notice it still needs a MonoGame SpriteFont named `Renogare` to show its menus. It also loads these sprite sheets `IdleSheet`, `RunSheet`, `JumpSheet`, `DashSheet`, `ClimbSheet` which currently don't exist. Good luck.

Add suitable source assets and entries for them to `Content/Content.mgcb`, then build the content with MonoGame's content tools. If using replacement assets, check that the sprite-sheet frame layouts match what `Player.cs` expects.

### Add level data and tilesets

`Leveldata.txt` is optional at startup: without it, the game creates an empty default level. which you'd be able to run around in. To load designed levels, add valid level data and make sure each tileset name in that data refers to a texture included in the content pipeline.

I made test maps with Ogmo Editor. The level data is read in this order:

```text
OriginX | OriginY | LevelWidth | LevelHeight | LevelExitBorder (T/B/L/R) | TilesetTextureName | Comma-Separated TileArray
```

Each additional tileset is represented by another texture-name and tile-array pair. See `Map.cs` for how the data is parsed. Once again, the tileset and map are missing, so like enjoy I guess.

### Finish gameplay systems

- Sound loading and sound updates are currently non operational placeholders.