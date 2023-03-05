using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList.Demo
{
    public class ListInteraction : MonoBehaviour
    {
        [SerializeField]
        private CircularScrollingList _scrollingList;
        [SerializeField]
        private Text _toggleInteractionText;

        public void EndMovement()
        {
            _scrollingList.EndMovement();
        }

        public void ToggleListInteractable()
        {
            _scrollingList.SetInteractable(!_scrollingList.IsInteractable);

            var interactingState = _scrollingList.IsInteractable ? "ON" : "OFF";
            _toggleInteractionText.text = $"List interactable: {interactingState}";
        }
    }
}
