# Level Manager

General purpose level manager

## Features

- Support single and additive loading
- Custom level data to be passed to the loaded level
- Custom level selection UI
- `OnLevelLoaded` and `OnAllLevelPassed` event
- All the related scenes sould be "addressable"
- Defined in the namespace `LevelManagement`

## Requirement

- Package "Addressables"

## Example

The example project is in the "Example" directory. The entry scene is the "LevelManager". Make sure that the package "Addressables" is installed and the scenes in the "Scenes" directory are all marked as addressable.

The simliar scripts mentioned in the "Setups" section could be found in the example project.

## Setups

### Create class `LevelDataManager`

`LevelDataManager` is the class for providing the data to the `LevelManager`. It should be derived from the class `AbstractLevelDataManager` (`AbstractLevelDataManager` is derived from `MonoBehaviour`). Here is the simplest `LevelDataManager`.

```csharp
using LevelManagement;
using System;
using UnityEngine;

public class LevelDataManager : AbstractLevelDataManager
{
    [SerializedField]
    private LevelDataItem[] _levels = null;

    // The total number of levels
    public override int Length => _levels.Length;

    // Get the scene asset of the target level id
    // The scene asset should be addressable
    public override AssetReference GetLevelScene(int id)
    {
        return _levels[id].levelScene;
    }

    // Get the level data for the loaded scene
    // It could be got from the `LevelManager.Instance.GetCurrentLevelData()` or
    // be automatically passed from the event `LevelManager.Instance.OnLevelLoaded`.
    public override object GetLevelData(int id)
    {
        return _levels[id].levelData;
    }

    // Get the preview data for creating the level selection item
    // It will be passed to the `AbstractLevelSelectionItem.SetPreview()`,
    // so you can set up the appearance of item according to the data passed here.
    //
    // The preview data could be stored in a seperated file to represent the user
    // score, grade, or lock status. Here is just returning the name of the level.
    public override object GetPreviewData(int id)
    {
        return $"Level {id}";
    }
}

[Serializable]
public class LevelDataItem
{
    public AssetReference levelScene;
    public LevelData levelData;
}

[Serializable]
public class LevelData
{
    public Vector3 spawnPoint;
    public float timeLimitation;
}
```

### Set up `LevelManager`

1. Select the scene used for the level selection
2. Mark the scene as addressable
3. Create an empty gameobject to the scene and attach `LevelManager` and `LevelDataManager` to it
4. Assign the reference of `LevelDataManager` to the "Level Data Manager" field of the `LevelManager` in the inspector
5. Assign the current scene to the "Home Scene" field of the `LevelManager` in the inspector
6. Set up the loading mode and whether to loop the levels or not
7. If the loading mode is `Additive`, assign the scene asset which will keep existing through levels to "Top Scene" field of the `LevelManager` in the inspector.

### Set up `LevelSelectionUI`

1. Create an empty gameobject under the canvas component to the scene where the `LevelManager` at
2. Create a scroll view as the child of created gameobject and adjust it to fit your needs
3. Attach `LevelSelectionUI` to the gameobject created in the step 1
4. Assign the "Content" gameobject under the scroll view gameobject to the "Content Container" field of `LevelSelectionUI` in the inspector

### Create class `LevelSelectionItem`

`LevelSelectionItem` is the class for setting up the level selection item in the level selection UI, such as setting up the appearance of the item and binding the callback to the `LevelManager`. Here is the simplest `LevelSelectionItem`:

```csharp
using LevelManagement;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionItem : AbstractLevelSelectionItem
{
    [SerializeField]
    private Button _button = null;
    [SerializeField]
    private Text _levelText = null;

    // Set up the appearance of the level selection item
    // The first argument is the ID of the specified level. This value should
    // be used for loading the level.
    // The second argument is the preview data provided from the
    // `LevelDataManager.GetPreviewData()`. You can set up the appearance of
    // the item according to the data you provided.
    public override void SetPreview(int levelID, object previewData)
    {
        _levelText.text = (string) previewData;
        _button.onClick.AddListener(
            () => LevelManager.Instance.LoadLevel(levelID)
        );
    }
}
```

### Set up `LevelSelectionItem`

1. Create a button gameobject under the "Content" gameobject of the scroll view created before
2. Attach `LevelSelectionItem` to it, assign the necessary fields, and adjust the gameobject to fit your needs
3. Create a prefab from this gameobject and delete the gameobject
4. Assign the created prefab to the "Level Item Prefab" of the `LevelSelectionUI` in the inspector
5. Set up the properties of `LevelSelectionUI`
    - Items A Row: Number of level items in a row
    - Border Item Gap: The gap between the position of the edge items and the border
    - Item Gap: The distance between each item

## Level Operations

The gameobject of `LevelManager` will keep existing through scenes after first created. You can access the instance of the `LevelManager` to perform level operations by `LevelManager.Instance`. Here are the functions:

- `GetCurLevelData()`: Get the level data of the current loaded level. The data is provided from the `LevelDataManager.GetLevelData()`

```csharp
using LevelManagement;

public class LevelInitializer
{
    [SerializeField]
    private Transform _playerTransform = null;

    private void Start()
    {
        var data = (LevelData) LevelManager.Instance.GetCurLevelData();
        _playerTransform.position = data.spawnPoint;
    }
}
```

- `LoadLevel(levelID)`: Load the target level
- `NextLevel()`: Load the next level. If "Loop Level" property of `LevelManager` is false, invoking `NextLevel()` will not load new level when the current level is the last level. Otherwise, the first level will be loaded. No matter the value of "Loop Level", the `OnAllLevelsPassed` event will be fired when invoking this function in the last level.
- `ReloadLevel()`: Reload the current level
- `LoadHomeScene()`: Load the scene specified in the "Home Scene" field of the `LevelManager` in the inspector

## Events

- `OnLevelLoaded(levelID, levelData)`: This event is fired when a level has been loaded. It is useful in the top scene when the loading mode is additive to know that a level is loaded and then initialize or reset objects.

```csharp
using LevelManagement;

public class TopSceneInitializer
{
    [SerializeField]
    private Transform _playerTransform = null;

    private void Start()
    {
        LevelManager.Instance.OnLevelLoaded += OnLevelLoaded;
    }

    // Don't forget to unregister the callback when the object is destroyed
    private void OnDestroy()
    {
        LevelManager.Instance.OnLevelLoaded -= OnLevelLoaded;
    }

    private void OnLevelLoaded(int levelID, object levelData)
    {
        var data = (LevelData) levelData;
        _playerTransform.position = data.spawnPoint;
    }
}
```

- `OnAllLevelsPassed()`: This event is fired when trying to load the next level in the last level.
