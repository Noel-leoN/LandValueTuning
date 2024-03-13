# Cities: Skylines 2 Mod - LandValueTuning

Fix landvalue bug and Tune methods of measuring landvalue to be more realistic.

# Features

- Fix landvalue bug (or maybe "designed" :-) in vanilla game.The vanilla landvalue system seems to calculate the impact factor of dense or sparse buildings on the road.Its a good feature but it seems to be going in the wrong direction by an incorrectly defined symbol. As a result, landvalue on the edge of some roads without buildings are exceptionally high, also caused the overall landvalue to be too high.
- Add new feature to calculate landvalue separately for each zonetype (residential,commerial,manufacturing,office,extractor,etc) , try to be more realistic.
- Modification will make landvalue decreases faster with distance (compared to vanilla).

# Requirements

- [Cities: Skylines 2](https://store.steampowered.com/app/949230/Cities_Skylines_II/) (duh)
- [BepInEx 5.4.22](https://github.com/BepInEx/BepInEx/releases)

# Usage
- Place the `*.dll` file in BepInEx `Plugins` folder.

# Compatibility & Known Issues

- Modified System:  LandValueSystem
- Not compatible: LandValueOverhaul, LandValue Rent Control, RenterandLandvaluePolicy and anyother mod which modified the LandValueSystem.
- Issues : When used with RealEco, it may cause the office area to be abnormal due to extra-low landprice.

## Changelog
- v1.0.0 (2024-03-13)
  - Initial build.

## Notice
- Strongly recommand save your game data before use this.

# Credits

 - Thanks to captain_on_coit for the Git repo template, and Jimmyokok for reference the code structure. 
 - Thanks to Cities Skylines 2 Unofficial Modding Discord[DISCORD](https://discord.gg/nJBfTzh7)
 - [CSLBBS](https://www.cslbbs.net): Cities: Skylines 2 community ( in chinese )
