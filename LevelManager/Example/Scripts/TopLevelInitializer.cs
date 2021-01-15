using LevelManagement;
using UnityEngine;
using UnityEngine.UI;

public class TopLevelInitializer : MonoBehaviour
{
    [SerializeField]
    private Text _titleText = null;

    private void Start()
    {
        // Register the callback to the LevelManager
        // The level scene is loaded after the top scene is loaded
        LevelManager.Instance.OnLevelLoaded += OnLevelLoaded;
    }

    private void OnDestroy()
    {
        // Don't forget to remove the registered callback
        // when the object is destroyed
        LevelManager.Instance.OnLevelLoaded -= OnLevelLoaded;
    }

    private void OnLevelLoaded(int id, object levelData)
    {
        UpdateTitleText((string)levelData);
    }

    private void UpdateTitleText(string message)
    {
        _titleText.text += $"{message}\n";
    }
}
