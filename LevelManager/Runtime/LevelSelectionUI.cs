using UnityEngine;

namespace LevelManagement
{
    /// <summary>
    /// The UI for selecting the level
    /// </summary>
    public class LevelSelectionUI : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The container for placing the level items")]
        private RectTransform _contentContainer = null;
        [SerializeField]
        [Tooltip("The prefab of the level item")]
        private GameObject _levelItemPrefab = null;
        [SerializeField]
        [Tooltip("The number of level items in a row")]
        private int _itemsARow = 1;
        [SerializeField]
        [Tooltip(
            "The gap between the position of the edge items and the border")]
        private float _borderItemGap = 30;
        [SerializeField]
        [Tooltip("The distance between each item")]
        private Vector2 _itemGap = new Vector2(40, 40);

        private void Start()
        {
            CreateSelectionUI();
        }

        /// <summary>
        /// Create the level selection items to the selection UI
        /// </summary>
        private void CreateSelectionUI()
        {
            var numOfLevel = LevelManager.Instance.NumOfLevel;
            var widthAdjust =
                (-(_itemsARow / 2) + ((_itemsARow & 1) == 0 ? 0.5f : 0))
                * _itemGap.x;

            for (var i = 0; i < numOfLevel; ++i) {
                var newItem =
                    Instantiate(_levelItemPrefab, _contentContainer.transform);

                var itemRectTransform = newItem.GetComponent<RectTransform>();
                itemRectTransform.anchoredPosition =
                    new Vector2(
                        widthAdjust + _itemGap.x * (i % _itemsARow),
                        -(_borderItemGap + _itemGap.y * (i / _itemsARow)));

                var levelSelectionItem =
                    newItem.GetComponent<AbstractLevelSelectionItem>();
                levelSelectionItem.SetPreview(
                    i, LevelManager.Instance.GetLevelPreviewData(i));
            }

            var numOfRows =
                (numOfLevel / _itemsARow)
                + ((numOfLevel % _itemsARow != 0) ? 1 : 0);
            _contentContainer.sizeDelta =
                new Vector2(
                    _contentContainer.sizeDelta.x,
                    _borderItemGap * 2 + _itemGap.y * (numOfRows - 1));
        }
    }
}
