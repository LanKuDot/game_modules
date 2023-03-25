# The game modules for Unity3D

## Include the Code in the Project

1. Create a directory named "Plugins" in the "Assets" directory
2. Put the code in this repo to the subdirectory of "Plugins" by:
    * downloading the code via git
    ```
    path/to/Assets/Plugins/$ git clone https://github.com/LanKuDot/game_modules.git
    ```
    * or downloading the zip file of this repo from GitHub and then unzip it to the "Plugins" directory.

## Modules

### [Circular Scrolling List - v6](CircularScrollingList/)

Available at [Unity Asset Store](https://u3d.as/2jZ9)

<img src="https://i.imgur.com/DCom1g9.gif" width=350px />

- Use finite list boxes to display infinte number of list contents
- It's all about math!
- Various list mode
  - List type: Circular or Linear
  - Control mode: Drag, Function, or Mouse Wheel
  - Direction: Vertical or Horizontal
- Support all three rendering modes of the canvas plane
- Support Unity 2018.4+ (Tested in Unity 2018.4.15f1. The demo scenes in the project are made in Unity 2019.4.16f1)
- See [README](CircularScrollingList/README.md) for more information and how to use it in the project

### [Level Manager](LevelManager/)

- The general purpose level manager
- Manage the level scene loading
- Support single and additive loading
- Custom level data and the level selection UI
