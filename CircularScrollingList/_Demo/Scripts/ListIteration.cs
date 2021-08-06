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
            StartCoroutine(IterationLoop());
        }

        private IEnumerator IterationLoop()
        {
            while (true) {
                _list.SelectContentID(_currentID);
                _currentID =
                    (int) Mathf.Repeat(
                        _currentID + _step, _list.listBank.GetListLength());
                yield return new WaitForSeconds(_stepInterval);
            }
        }
    }
}
