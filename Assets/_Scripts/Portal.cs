using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private RenderTexture rt;
    private bool isWalkingThroughPortal = false;

    private Player player;
    private PlayerCam playerCam;

    [SerializeField] private bool debug = false;
    [SerializeField] private Camera portalCam;
    [SerializeField] private Portal connectedPortal;
    [SerializeField] private MeshRenderer portalRenderer;
    [SerializeField] private Vector3 teleportPosition;
    [SerializeField] private float nearClipOffset = 0.05f;
    [SerializeField] private float nearClipLimit = 0.2f;

    public Camera PortalCam { get { return portalCam; } }
    public RenderTexture RT { get { return rt; } }

    void Awake()
    {
        CreateRenderTexture();
    }

    void Start()
    {
        player = ServiceLocator.GetService<Player>();
        playerCam = ServiceLocator.GetService<PlayerCam>();
    }

    void Update()
    {
        /*Vector3 dir = transform.position - player.transform.position;
        dir.y = 0.0f;

        connectedPortal.PortalCam.transform.rotation = Quaternion.LookRotation(dir);

        float distance = Mathf.Abs(transform.position.z - player.transform.position.z);
        if (debug)
        {
            Debug.Log("distance: " + distance);
        }

        Vector3 portalCamTargetPos = connectedPortal.transform.position + (-connectedPortal.PortalCam.transform.forward * distance);
        portalCamTargetPos.y = connectedPortal.PortalCam.transform.position.y;
        connectedPortal.PortalCam.transform.position = portalCamTargetPos;

        if (debug)
        {
            //Debug.Log("Direction: " + dir);
            //Debug.DrawLine(connectedPortal.PortalCam.transform.position, connectedPortal.PortalCam.transform.position + (dir * 100.0f), Color.red, 10.0f);
        }*/
    }

    void CreateRenderTexture()
    {
        if (rt == null || rt.width != Screen.width || rt.height != Screen.height)
        {
            if (rt != null)
            {
                rt.Release();
            }

            rt = new RenderTexture(Screen.width, Screen.height, 0);

            connectedPortal.PortalCam.targetTexture = rt;

            portalRenderer.material.SetTexture("_MainTex", rt);
            portalRenderer.material.SetInt("displayMask", 1);
        }
    }

    public void Render()
    {
        portalRenderer.enabled = false;

        CreateRenderTexture();

        Vector3 portalCamPos = connectedPortal.transform.TransformPoint(transform.InverseTransformPoint(playerCam.transform.position));
        connectedPortal.PortalCam.transform.position = portalCamPos;

        connectedPortal.PortalCam.transform.rotation = playerCam.transform.rotation;

        SetNearClipPlane();

        portalCam.Render();

        portalRenderer.enabled = true;
    }

    void SetNearClipPlane()
    {
        int dot = System.Math.Sign(Vector3.Dot(transform.forward, transform.position - portalCam.transform.position));

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(transform.position);
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector(transform.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        if (Mathf.Abs(camSpaceDst) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);
            portalCam.projectionMatrix = playerCam.cam.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            portalCam.projectionMatrix = playerCam.cam.projectionMatrix;
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.CompareTag("Player"))
        {
            player.Teleport(connectedPortal.PortalCam.transform.position);

            if (debug)
            {
                Debug.Log(gameObject.name + ": Walking through portal.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            if (debug)
            {
                Debug.Log(gameObject.name + ": Exiting portal.");
            }
        }
    }
}
