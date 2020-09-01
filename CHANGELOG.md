# Change Log

### [1.4.0] - 2020.09.01

Update the circular scrolling list to version 4

**Changed**

- Use `AnimationCurve` to define the layout and the movement of the list
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
