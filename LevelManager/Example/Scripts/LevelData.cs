using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(
    fileName = "LevelData",
    menuName = "Level Data")]
public class LevelData : ScriptableObject
{
    [SerializeField]
    private LevelDataItem[] _levels = null;

    public int Length => _levels.Length;

    public LevelDataItem this[int index]
    {
        get {
            if (index < 0 || index >= _levels.Length)
                throw new IndexOutOfRangeException(
                    "The specified level ID is out of range");

            return _levels[index];
        }
    }
}

[Serializable]
public class LevelDataItem
{
    [SerializeField]
    private AssetReference _levelScene = null;
    [SerializeField]
    private string _levelData = null;

    public AssetReference levelScene => _levelScene;
    public string levelData => _levelData;
}
