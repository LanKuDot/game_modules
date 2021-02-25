using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    [SerializeField]
    private int _targetFPS = 60;

    private void Start()
    {
        Application.targetFrameRate = _targetFPS;
    }
}
