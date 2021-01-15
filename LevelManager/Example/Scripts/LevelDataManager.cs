using LevelManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LevelDataManager : AbstractLevelDataManager
{
    [SerializeField]
    private LevelData _levelData = null;

    public override int Length => _levelData.Length;

    public override AssetReference GetLevelScene(int id)
    {
        return _levelData[id].levelScene;
    }

    public override object GetLevelData(int id)
    {
        return _levelData[id].levelData;
    }

    public override object GetPreviewData(int id)
    {
        return $"{id}";
    }
}
