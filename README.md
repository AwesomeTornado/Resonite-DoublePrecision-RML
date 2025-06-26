# DoublePrecision

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that fixes graphical floating-point errors.
This mod intercepts the Unity-FrooxEngine connectors and moves the Unity world around the player, instead of the usual method of moving the player around the world.


## Screenshots
[Check out a video demo!](https://www.youtube.com/watch?v=uc2fgfLM9WQ)


![image](https://github.com/user-attachments/assets/a3fb1433-b430-4560-b060-bdf6e13524b4)
A screenshot of an inspector at over 10 thousand meters away from the world origin, rendering without a single floating point error.

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
1. Place [DoublePrecision.dll](https://github.com/YourGithubUsername/YourModRepoName/releases/latest/download/DoublePrecision.dll) into your `rml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader installed it will create this folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Resonite logs.

#
This is the RML Release repository. Check out the MonkeyLoader Development repository [here](https://github.com/AwesomeTornado/Resonite-DoublePrecision).
