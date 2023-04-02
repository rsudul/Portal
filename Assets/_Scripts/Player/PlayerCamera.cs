using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : InstancedSystem, IPlayerCameraProvider, ISceneInjectee
{
    public override Type Type => typeof(IPlayerCameraProvider);

    private Camera cam;

    [Inject] private IPortalManager portalManager;

    [SerializeField] private Portal portal1;
    [SerializeField] private Portal portal2;

    void Awake()
    {
        if (!TryGetComponent<Camera>(out cam))
        {
            Debug.LogError(gameObject.name + ": Camera component not found.");
        }
    }

    private void OnPreCull()
    {
        if (portalManager == null)
        {
            return;
        }

        foreach (Portal portal in portalManager.GetAllPortals())
        {
            portal.PreRender();
        }

        foreach (Portal portal in portalManager.GetAllPortals())
        {
            portal.Render();
        }

        foreach (Portal portal in portalManager.GetAllPortals())
        {
            portal.PostRender();
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public Camera GetCamera()
    {
        return cam;
    }

    public void OnInjected()
    {

    }
}
