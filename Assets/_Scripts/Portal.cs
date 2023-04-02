using System;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour, ISceneInjectee
{
    private RenderTexture rt;
    private bool isWalkingThroughPortal = false;

    private Dictionary<Guid, IPortalTraveller> portalTravellers = new Dictionary<Guid, IPortalTraveller>();

    [Inject] private IPlayerCameraProvider playerCamera;

    [SerializeField] private bool debug = false;
    [SerializeField] private Camera portalCam;
    [SerializeField] private Portal connectedPortal;
    [SerializeField] private MeshRenderer portalRenderer;
    public Transform portalEntrance;
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

    }

    void Update()
    {
        HandlePortalTravellers();
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

        Vector3 portalCamPos = connectedPortal.transform.TransformPoint(transform.InverseTransformPoint(playerCamera.GetTransform().position));
        connectedPortal.PortalCam.transform.position = portalCamPos;

        connectedPortal.PortalCam.transform.rotation = playerCamera.GetTransform().rotation;

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
            portalCam.projectionMatrix = playerCamera.GetCamera().CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            portalCam.projectionMatrix = playerCamera.GetCamera().projectionMatrix;
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent<IPortalTraveller>(out IPortalTraveller portalTraveller))
        {
            OnTravellerEnterPortal(portalTraveller);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.TryGetComponent<IPortalTraveller>(out IPortalTraveller portalTraveller))
        {
            if (collider.TryGetComponent<IUniqueIDProvider>(out IUniqueIDProvider uniqueIDProvider))
            {
                if (portalTravellers.ContainsKey(uniqueIDProvider.GetUniqueID()))
                {
                    portalTravellers.Remove(uniqueIDProvider.GetUniqueID());
                }
            }
        }
    }

    public void HandlePortalTravellers()
    {
        if (portalTravellers.Count == 0)
        {
            return;
        }

        List<Guid> portalTravellersToRemove = new List<Guid>();

        foreach (KeyValuePair<Guid, IPortalTraveller> portalTraveller in portalTravellers)
        {
            Matrix4x4 m = connectedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * portalTraveller.Value.GetTransform().localToWorldMatrix;

            Vector3 portalOffset = portalTraveller.Value.GetTransform().position - transform.position;
            int portalSide = Math.Sign(Vector3.Dot(portalOffset, transform.forward));
            int portalSideOld = Math.Sign(Vector3.Dot(portalTraveller.Value.GetPortalOffsetOld(), transform.forward));

            if (portalSide != portalSideOld)
            {
                portalTraveller.Value.Travel(transform, connectedPortal.transform, m.GetColumn(3));
                connectedPortal.OnTravellerEnterPortal(portalTraveller.Value);
                portalTravellersToRemove.Add(portalTraveller.Key);
            }
            else
            {
                portalTraveller.Value.SetPortalOffsetOld(portalOffset);
            }
        }

        for (int i=0; i<portalTravellersToRemove.Count; i++)
        {
            portalTravellers.Remove(portalTravellersToRemove[i]);
        }
    }

    public void OnTravellerEnterPortal(IPortalTraveller portalTraveller)
    {
        if (portalTraveller.GetTransform().TryGetComponent<IUniqueIDProvider>(out IUniqueIDProvider uniqueIDProvider))
        {
            if (!portalTravellers.ContainsKey(uniqueIDProvider.GetUniqueID()))
            {
                portalTraveller.SetPortalOffsetOld(portalTraveller.GetTransform().position - transform.position);
                portalTravellers.Add(uniqueIDProvider.GetUniqueID(), portalTraveller);
            }
        }
    }

    public void OnInjected()
    {

    }
}
