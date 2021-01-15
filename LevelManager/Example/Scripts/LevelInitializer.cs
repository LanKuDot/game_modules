using LevelManagement;
using UnityEngine;
using UnityEngine.UI;

public class LevelInitializer : MonoBehaviour
{
    [SerializeField]
    private Text _levelDataText = null;
    [SerializeField]
    private Button _reloadLevelBtn = null;
    [SerializeField]
    private Button _nextLevelBtn = null;
    [SerializeField]
    private Button _homeBtn = null;

    private void Start()
    {
        _levelDataText.text =
            $"LevelData: {LevelManager.Instance.GetCurLevelData()}";
        _reloadLevelBtn.onClick.AddListener(LevelManager.Instance.ReloadLevel);
        _nextLevelBtn.onClick.AddListener(LevelManager.Instance.NextLevel);
        _homeBtn.onClick.AddListener(LevelManager.Instance.LoadHomeScene);
    }
}
