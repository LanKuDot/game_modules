# Change Log

### [CircularList_v6.2] - 2023.05.17

**Added**

- Add `OnBoxMoved` to `ListBox`

### [CircularList_v6.1] - 2023.04.06

**Added**

- Add `OnInitialized` to `ListBox`

**Fixed**

- Incorrect box state when refreshing the list with top/bottom focusing

### [CircularList_v6] - 2023.03.25

**Added**

- Add events
  - `OnBoxSelected`
  - `OnFocusingBoxChanged`
- Allow multi-control modes at the same time
- Support 3 different focusing posiiton
- Support the list to be stopped at the top or the bottom
- Support layout preview in the editor
- Support runtime setup and initialization
- Stop the movement by the script
- Toggle the interaction of the list by the script
- Get the the boxes in the list and the list which the box belongs to

**Changed**

- Code refactoring a lot
- Arrange the demo scenes
- Arrange the displaying order of options
- Rename options
  - The setting "Setting" -> "ListSetting"
  - The mode "Drag" -> "Pointer"
  - The option "Align Middle" -> "Align At Focusing Position"
  - The option "Reverse Direction" -> "Reverse Scrolling Direction"
  - The option "Centered Content ID" -> "Init Focusing Content ID"
  - The option "Center Selected Box" -> "Focus Selected Box"
  - The option "Reverse Order" -> "Reverse Content Order"
- Rename properties
  - Capitalize the first character of properties

**Fixed**

- Incorrect layout when the list moves too fast
- Incorrect aligning position for even number of boxes

**Removed**

- Remove events
  - `OnBoxClick`
  - `OnCenteredContentChanged`
- Remove "Assign References of Bank and Boxes" option from the context menu

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
