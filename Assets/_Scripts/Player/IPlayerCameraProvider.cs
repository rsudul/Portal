using UnityEngine;

public interface IPlayerCameraProvider
{
    Transform GetTransform();
    Camera GetCamera();
}