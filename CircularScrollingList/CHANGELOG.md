# Change Log

### [CircularList_v5.1] - 2021.11.11

**Fixed**

- Fix the incorrect behaviour of the linear-function mode

### [CircularList_v5] - 2021.08.05

**Added**

- Support custom type of the list box and list bank
- Support dynamic list contents, also 0 content
- Add new options
  - Reverse scrolling direction
  - Reverse order
  - Center selected box
  - Initialize on start
- Add new events
  - `OnCenteredContentChanged`
  - `OnMovementEnd`
- Select the list content from the script
- Image sorting - The centered box will be in front of the others.

**Changed**

- All the related classes are in the namespace `AirFishLab.ScrollingList`.
- Change setup class from `ListPositionCtrl` to `CircularScrollingList`
- Change the x range of the list position curve and list scale curve from [0, 1] to [-1, 1]
- Separate the curve used for list velocity and list movement
- The layout of the list is decided by the width/height of the rect transform.
- Improve the calculation of the dragging velocity
- Code refactoring
- The supported Unity version is 2018.4+ (C# 6.0+)

### [CircularList_v4] - 2020.09.01

**Added**

- Use `AnimationCurve` to define the layout and the movement of the list

**Changed**

- Improve the movement
- Improve the inspector view for the list
- Rename mode "Button" to "Function"

### [CircularList_v3] - 2019.06.09

**Added**

- Use Unity's event system to detect the input events
- Add linear mode
- Add control mode: Mouse Wheel (thanks to @aledg)
- Add OnBoxClick event for receiving the content ID of the selected box
- Add BaseListBank for creating independent list banks
- Support multiple lists in the same scene

### [CircularList_v2.2] - 2019.01.08

**Added**

- Able to scroll the list by the mouse wheel
- Able to set the initial ID of the centered content

**Fixed**

- Fix the list won't be centered in the Button Mode when the up/down button is clicked too quickly.

### [CircularList_v2] - 2017.09.30

First release of circular scrolling list
