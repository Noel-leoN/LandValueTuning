# Cities: Skylines 2 Mod - LandValueTuning

Fix landvalue bug and Tune methods of measuring landvalue to be more realistic.

# Features

- Fix 2 landvalue "designed" feature in vanilla game(or just bugs caused by carelessness and they didn't find this issue for 5 months  :-) . The vanilla landvalue system seems to calculate the impact factor of dense or sparse buildings on the road. Its a good feature but it seems to be going in the wrong direction by incorrectly defined symbols. Also they considered the situation where the landvalue at start of a road is lower than at the end, but does not take into account the opposite. As a result, landvalue on the edge of long roads without buildings are exceptionally high, also caused the high landvalue spread to everywhere. 
- Add new feature to calculate landvalue separately for each zonetype(residential,commerial,manufacturing,office,extractor,etc) , try to be more realistic.
- Modification will make landvalue decreases faster with distance (compared to vanilla).

# Requirements

- [Cities: Skylines 2](https://store.steampowered.com/app/949230/Cities_Skylines_II/) (duh)
- [BepInEx 5.4.22](https://github.com/BepInEx/BepInEx/releases)

# Usage
- Place the `*.dll` file in BepInEx `Plugins` folder.

# Compatibility & Known Issues

- Modified System:  LandValueSystem
- Not compatible: LandValueOverhaul, LandValue Rent Control, RenterandLandvaluePolicy and anyother mod which modified the LandValueSystem.
- Issues : The rent of mix comanies seems a bit low (due to RealEco's new feature of immerial solds?)

## Changelog

- v1.0.5 (2024-03-18)
 - Tweak the landvalue coefficient & residential upkeep to fit RealEco. 

- v1.0.3
 - Fix another "designed"(or bug actually :-) feature in landvalue system.In vanilla, only the value of the beginning of the road is higher than the value of the end point is considered, and the opposite is not . As a result, if a long road is extended, land prices will gradually increase along a new extended road , with or without zoning.
 - The landvalue coefficient calculated by zonetype has been adjusted to make it more balanced.
 - Add feature to reduce the upkeep cost of residential to make it more suitable for low landvalue.(Residential buildings can collapse due to low landvalue resulting in low rents that cannot afford maintenance fees)
 - Now it's more comapatible with RealEco, by manually setting prefab to "false".(When the mod effect is stacked, use the preset prefab of RealEco will cause the rent to be too low. Or you can adjust prefabs manually.)

- v1.0.0 (2024-03-13)
  - Initial build.

## Notice
- Strongly recommand save your game data before use this.

# Credits

 - Thanks to captain_on_coit for the Git repo template, and Jimmyokok for reference the code structure. 
 - Thanks to Cities Skylines 2 Unofficial Modding Discord[DISCORD](https://discord.gg/nJBfTzh7)
 - [CSLBBS](https://www.cslbbs.net): Cities: Skylines 2 community ( in chinese )


![2024-03-17 221903](https://github.com/Noel-leoN/LandValueTuning/assets/151483346/de365427-39f2-4d4b-ae92-21cab3c839fc)
fix low res zone fit

![2024-03-17 221940](https://github.com/Noel-leoN/LandValueTuning/assets/151483346/5294e4dd-b9dd-4d29-8258-2839a04930ec)
landvalue overall
