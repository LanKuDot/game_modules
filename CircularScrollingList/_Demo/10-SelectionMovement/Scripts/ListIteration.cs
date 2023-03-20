using System.Collections;
using UnityEngine;

namespace AirFishLab.ScrollingList.Demo
{
    public class ListIteration : MonoBehaviour
    {
        [SerializeField]
        private CircularScrollingList _list;
        [SerializeField]
        private int _step = 1;
        [SerializeField]
        private float _stepInterval = 0.1f;

        private int _currentID;

        private void Start()
        {
            // Make the list not interactable while it is controlled by the script
            _list.SetInteractable(false);
            StartCoroutine(IterationLoop());
        }

        private IEnumerator IterationLoop()
        {
            while (true) {
                // The selection movement still works even if the list is not interactable.
                // The default value of 'notToIgnore' parameter is true.
                _list.SelectContentID(_currentID);
                _currentID =
                    (int) Mathf.Repeat(
                        _currentID + _step, _list.ListBank.GetContentCount());
                yield return new WaitForSeconds(_stepInterval);
            }
        }
    }
}
