Hello! Thanks for taking the time to look at this project! Below are some instructions for the game, as well as some information regarding the code.

======Overview
In this game ("Please Wear A Helmet"), you play as a Jetpack test-pilot flying around a futuritic city, trying not to collide with any obstacles.

The game consists of five levels. The objective of the game is to pilot your character from the "Spawn Tube" into a portal at the end of the level. Reaching the portal will result in the next level loading.

The pilot's jetpack is fragile, and will be damaged if it collides with any of the terrain in the game. One collision will send you into a "broken" state, where controling the jetpackwill become very difficult (Though not impossible). Any subsequent collisions will result in an explosion, and you'll have to start again!

You can land on the ground at any point, so long as you land on your feet. Also take care not to hit your head, as this will put you into a stunned state for a short period of time, during which you will be unable to control the character at all.

After finishing a level successfully, you will then be able to see a replay of your successful run, played back simultaneously with all of your unsuccussful runs!

======Controls
Thrust - Space / W
Boost - Shift
Rotate Left - Left Arrow / A
Rotate Right - Right Arrow / D
Tuck in - Ctrl

- It is possible to change the controls via a binding option in the menu screen. Gamepads should be supported though this has only been tested with and Xbox One controller

======Notes
- This Unity project was built with Version 2019.2.6f1

- Changing keybindings can result in the formatting of the JSON file to become difficult to read. I have provided a formatted version of the keybindings availible at: 
PWAH\UnityProject\PWAH\Assets\Resources\GameJSONData\Keybinds_FormattedExample.json

- I have provided an example of a scripted sequence showing the functionality that can be used. This can be found at:
PWAH\UnityProject\PWAH\Assets\Resources\GameJSONData\Cutscenes\TestCutscene.json

- The above sequence can be previewed from the main menu. Part of the sequence involves a pause that waits for player Input. Press the spacebar in order to advance.

- When finishing a level in the game, it is not currently possible to skip viewing the replays


======Third Party Code
I have made use of a number of third party plugins as part of this project. In order to avoid any misconceptions, here is a list of plugins/utilities that I did not write:

*2d Toolkit Plugin - Unikron Software - Sprite Rendering / Tilemap implementaiton. Classes prefixed with "tk2dCLASSNAME"
- Note: I have made a few revisions and modifications to the source code of the above plugin. These can be found by searching the project for "//PWAH"

*DoTween Plugin - Demigant - Plugin for various types of tweening (Position, rotation, scale, colour etc.)

*Pixel Art Shader - Cole Cecil - A shader that uses a mix of Biliear Filtering and Nearest-Neighbor scaling to maintain close to "Pixel-perfect" scaling when working with resolutions that are not integer scalings of the native resolution

*JSON Object - Matt Schoen (Defective Studios) - JSON parsing implementation


======Other Code and Assets
All other code and game assets were authored by myself. In terms of c# scripts, this is any class contained within the "Scripts Folder". Most follow the naming convention "s_CLASSNAME".