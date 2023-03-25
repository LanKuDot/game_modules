using UnityEngine;

namespace AirFishLab.ScrollingList.Demo
{
    public class LateInitialization : MonoBehaviour
    {
        [SerializeField]
        private CircularScrollingList _list;
        [SerializeField]
        private BaseListBank _listBankSource;
        [SerializeField]
        private ListBox _listBoxSource;
        [SerializeField]
        private int _numOfBoxes;

        public void InitializeTheList()
        {
            _list.SetListBank(_listBankSource);

            var boxSetting = _list.BoxSetting;
            boxSetting.SetBoxPrefab(_listBoxSource);
            boxSetting.SetNumOfBoxes(_numOfBoxes);

            var listSetting = _list.ListSetting;
            listSetting.SetListType(CircularScrollingList.ListType.Linear);
            listSetting.SetAlignAtFocusingPosition(true);
            listSetting.SetFocusSelectedBox(true);
            listSetting.AddOnBoxSelectedCallback(OnBoxSelected);

            _list.Initialize();
        }

        private void OnBoxSelected(ListBox box)
        {
            var intListBox = (IntListBox)box;
            Debug.Log($"The selected content: {intListBox.Content}");
        }
    }
}
