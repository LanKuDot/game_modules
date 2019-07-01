# Circular Scrolling List

## Features

* Use finite list boxes to display infinite list items
* Use Unity's event system to detect input events
* Support all three render modes of the canvas plane
* Multiple lists can be placed in the same scene.
* Support Unity 5+ (Tested in Unity 5.6.7f1)

### List mode

* List type: Circular or Linear mode
* Control mode: Drag, Button, or Mouse wheel
* Direction: Vertical or Horizontal

[Demo video](https://youtu.be/k63eqwfj-1c)

## How to Use

### Set up the List

1. Download the scripts in the directory `CircularScrollingList` and put into a unity project.
2. Go to the tool bar -> Edit -> Project Settings -> Script Execution Order. Set the execution order of the `ListPositionCtrl.cs` prior to the `ListBox.cs`, and then click "Apply" button (or the script will be initialized in the alphabet order).\
	![Imgur](https://i.imgur.com/cyGnLQ9.png)
3. Add a Canvas plane to the scene. Set the render mode  to "Screen Space - Camera" (for example), and assign the "Main Camera" to the "Render Camera".\
	![Imgur](https://i.imgur.com/YgysLbH.png)
4. Create an empty gameobject as the child of the Canvas plane, rename it to `CircularList` (or another name you like), and attach the script `ListPositionCtrl.cs` to it.\
	![Imgur](https://i.imgur.com/5eU8kPq.png)
5. Create a Button gameobject as the child of the `CircularList`, rename it to `ListBox`, and attach the script `ListBox.cs` to it.
6. Assign the gameobject "Text" of the Button to the "Content" of the `ListBox.cs`.\
	![Imgur](https://i.imgur.com/BbZaece.png)
7. Duplicate the gameobject `ListBox` as many times as you want (4 times here, for exmaple), and assign them to the "List Boxes" of the script `ListPositionCtrl.cs`.\
	![Imgur](https://i.imgur.com/ABxPlrZ.png)
8. Set the list mode.\
	![Imgur](https://i.imgur.com/3B40zJr.png)
9. "Play" the scene to view the layout of the list, and adjust boxes to the proper size. (Exceptions will be raised, because the `ListBank` hasn't been created yet.)

### Create `ListBank`

1. Create a new script named `MyListBox.cs`, and launch the editor.
2. Inherit the abstract class `BaseListBank` (The class `BaseListBank` inherits the `MonoBehaviour`, therefore, you can initialize list contents in `Start()` and attach the script to a gameobject).
3. There are two functions which must be implemented:
	* `public string GetListContent(int index)`: Get the string representation of the specified content.
	* `public int GetListLength()`: Get the number of the list contents.
```csharp
// The example of the simplest ListBank
public class MyListBank: BaseListBank
{
    private int[] _contents = {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10
    };

    public override string GetListContent(int index)
    {
        return _contents[index].ToString();
    }

    public override int GetListLength()
    {
        return _contents.Length;
    }
}
```
4. Attach the script `MyListBank.cs` to the gameobject `CircularList` (or another gameobject), and assign the gameobject to the "List Bank" of the script `ListPositionCtrl.cs`.\
	![Imgur](https://i.imgur.com/fK47UV5.png)
5. Set the "Centered Content ID" to change the initial displaying content if needed.
6. "Play" the scene and the list works properly.\
	![Imgur](https://i.imgur.com/KHrZZ3o.gif)

Additional settings for the control mode of Button, please see the section "Control Mode: Button".

### Get the ID of the Selected Content

There are two ways to get ID of the selected content.

1. Create callback function
2. Get the centered content ID

**Create Callback Function**

When a box is clicked, the `ListPositionCtrl` will launch the event `OnBoxClick` (actually launch from the `Button.onClick` event). The callback function (or the listener) for the event must have 1 parameter for receiving the ID of the selected content.

For example, add a function `GetSelectedContentID` as the callback function to the class `MyListBank`.

```csharp
public void GetSelectedContentID(int contentID)
{
    Debug.Log("Selected content ID: " + contentID.ToString() +
        ", Content: " + GetListContent(contentID));
}
```

Then, add it to the "On Box Click (Int 32)" of the script `ListPositionCtrl.cs` in the inspector.\
![Imgur](https://i.imgur.com/EmzRYr2.png)

It will be like:\
![Imgur](https://i.imgur.com/khBJMpn.gif)

**Get the Centered Content ID**

The other way is to invoke the function `ListPositionCtrl.GetCenteredContentID()` which will find the list box closest to the center and return the content ID of it.

For example, create a function which will update the content of the centered box to the Text, and use a Button to invoke it.

```csharp
public class MyApplication: MonoBehaviour
{
    public ListPositionCtrl list;
    public Text displayText;

    public void DisplayCenteredContent()
    {
        int contentID = list.GetCenteredContentID();
        string centeredContent = list.listBank.GetListContent(contentID);
        displayText.text = "Centered content: " + centeredContent;
    }
}
```

It will be like:\
![Imgur](https://i.imgur.com/ODVSR2t.gif)

### Control Mode: Button

In this mode, you have to create two additional Button gameobjects for controlling the list.

1. Create two Buttons as the child of the gameobject `CircularList`, and rename them to `Button_NextContent` and `Button_LastContent`.
2. Place the gameobject `Button_NextContent` to the top of the list, and the other to the bottom of the list.\
	![Imgur](https://i.imgur.com/MwasBgp.png)
3. Assign the function `ListPositionCtrl.MoveOneUnitUp()` to the `onClick` event of the gameobject `Button_NextContent`, and assign the function `ListPositionCtrl.MoveOneUnitDown()` to same event of the other gameobject.\
	![Imgur](https://i.imgur.com/jWQLDpj.png)
4. Done!\
	![Imgur](https://i.imgur.com/hbQL73Q.gif)

### Layout Settings

There are some settings to modify the layout of the list:

* Box gap factor: Set the distance between the boxes. The larger, the closer.
* Box sliding frames: Set the sliding duration. The larger, the longer.
* Box sliding speed factor: Set the sliding speed. The larger, the quicker.
* List curvature: Set the whole list curving left/right (in vertical mode), or up/down (in horizontal mode).
* Position adjust: Adjust the centroid of the whole list.
* Center box scale rate: The scaling of the centered box.
