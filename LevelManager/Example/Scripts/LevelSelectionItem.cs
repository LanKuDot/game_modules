using LevelManagement;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelSelectionItem : AbstractLevelSelectionItem
{
    [SerializeField]
    private Button _button = null;
    [SerializeField]
    private Text _levelText = null;

    private void Reset()
    {
        _button = GetComponent<Button>();
    }

    public override void SetPreview(int levelID, object previewData)
    {
        _levelText.text = (string) previewData;
        _button.onClick.AddListener(
            () => LevelManager.Instance.LoadLevel(levelID));
    }
}
