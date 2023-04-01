using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private Portal portal1;
    [SerializeField] private Portal portal2;
    public Camera cam;

    void Awake()
    {
        ServiceLocator.RegisterService<PlayerCam>(this);
    }

    private void OnPreCull()
    {
        portal1.Render();
        portal2.Render();
    }
}
