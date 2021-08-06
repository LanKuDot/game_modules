using System;
using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList.Demo
{
    public class VariableStringListBank : BaseListBank
    {
        [SerializeField]
        private InputField _contentInputField;
        [SerializeField]
        private string[] _contents = {"a", "b", "c", "d", "e"};
        [SerializeField]
        private CircularScrollingList _circularList;
        [SerializeField]
        private CircularScrollingList _linearList;

        private readonly DataWrapper _dataWrapper = new DataWrapper();

        /// <summary>
        /// Extract the contents from the input field and refresh the list
        /// </summary>
        public void ChangeContents()
        {
            _contents =
                _contentInputField.text.Split(
                    new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
            _circularList.Refresh();
            _linearList.Refresh();
        }

        public override object GetListContent(int index)
        {
            _dataWrapper.data = _contents[index];
            return _dataWrapper;
        }

        public override int GetListLength()
        {
            return _contents.Length;
        }

        /// <summary>
        /// Used for carry the data of value type to avoid boxing/unboxing
        /// </summary>
        public class DataWrapper
        {
            public string data;
        }
    }
}
