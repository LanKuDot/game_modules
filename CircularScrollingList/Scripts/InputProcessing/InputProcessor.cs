using UnityEngine;
using UnityEngine.EventSystems;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// The class for processing the input value
    /// </summary>
    public class InputProcessor
    {
        #region Private Members

        /// <summary>
        /// The target detecting rect transform
        /// </summary>
        private readonly RectTransform _rectTransform;
        /// <summary>
        /// The maximum coordinate of the target rect transform
        /// </summary>
        private readonly Vector2 _maxRectPos;
        /// <summary>
        /// The camera which the canvas is referenced
        /// </summary>
        private readonly Camera _canvasRefCamera;
        /// <summary>
        /// The time of the last input event
        /// </summary>
        private float _lastInputTime;
        /// <summary>
        /// The last input position in the space of the target rect transform
        /// </summary>
        private Vector2 _lastLocalInputPos;

        #endregion

        public InputProcessor(RectTransform rectTransform, Camera canvasRefCamera)
        {
            _rectTransform = rectTransform;
            _maxRectPos = _rectTransform.rect.max;
            Debug.Log(_maxRectPos);
            _canvasRefCamera = canvasRefCamera;
        }

        /// <summary>
        /// Get the input information according to the input event data
        /// </summary>
        public InputInfo GetInputInfo(PointerEventData eventData, InputPhase phase)
        {
            var deltaPos =
                phase == InputPhase.Scrolled
                    ? eventData.scrollDelta
                    : GetDeltaPos(eventData.position, phase);
            var deltaPosNormalized =
                new Vector2(
                    deltaPos.x / _maxRectPos.x,
                    deltaPos.y / _maxRectPos.y);

            var curTime = Time.realtimeSinceStartup;
            if (phase == InputPhase.Began || phase == InputPhase.Scrolled)
                _lastInputTime = curTime;
            var deltaTime = curTime - _lastInputTime;
            _lastInputTime = curTime;

            return new InputInfo {
                Phase = phase,
                DeltaLocalPos = deltaPos,
                DeltaLocalPosNormalized = deltaPosNormalized,
                DeltaTime = deltaTime
            };
        }

        /// <summary>
        /// Calculate the local delta position
        /// </summary>
        private Vector2 GetDeltaPos(Vector2 screenInputPos, InputPhase phase)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform, screenInputPos, _canvasRefCamera, out var localPos);

            if (phase == InputPhase.Began) {
                _lastLocalInputPos = localPos;
                return Vector2.zero;
            }

            var deltaPos = localPos - _lastLocalInputPos;
            _lastLocalInputPos = localPos;
            return deltaPos;
        }
    }
}
