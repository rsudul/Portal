using UnityEngine;

public class Headbob : MonoBehaviour, ISceneInjectee
{
    private Vector3 startPos = Vector3.zero;
    private Quaternion startRot = Quaternion.identity;

    private Vector3 smoothDampVel = Vector3.zero;

    private float theta = 0.0f;
    private float distance = 0.0f;

    [Inject] private IPlayerCameraProvider playerCamera;
    [Inject] private IPlayerProvider player;

    [SerializeField] private float amplitude = 10.0f;
    [SerializeField] private float period = 5.0f;

    private void Update()
    {
        if (player.IsMoving())
        {
            Play();
        }
        else
        {
            Stop();
        }
    }

    public void Play()
    {
        theta = Time.timeSinceLevelLoad / period;
        distance = amplitude * Mathf.Sin(theta);

        playerCamera.GetTransform().localPosition = startPos + Vector3.up * distance;
    }

    public void Stop()
    {
        playerCamera.GetTransform().localPosition = Vector3.SmoothDamp(playerCamera.GetTransform().localPosition, startPos, ref smoothDampVel, 1 / period * Time.fixedDeltaTime);
    }

    public void OnInjected()
    {
        startPos = playerCamera.GetTransform().localPosition;
        startRot = playerCamera.GetTransform().localRotation;
    }
}