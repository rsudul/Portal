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

    void LateUpdate()
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

    public void PreRender()
    {
        foreach (KeyValuePair<Guid, IPortalTraveller> portalTraveller in portalTravellers)
        {
            if (portalTraveller.Value.ShouldClone())
            {
                UpdateSlicingForTraveller(portalTraveller.Value);
            }
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

    public void PostRender()
    {
        ProtectScreenFromClipping(playerCamera.GetTransform().position);
    }

    void SetNearClipPlane()
    {
        int dot = Math.Sign(Vector3.Dot(transform.forward, transform.position - portalCam.transform.position));

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
                    portalTraveller.OnExitPortal();
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
                Vector3 portalTravellerPositionOld = portalTraveller.Value.GetTransform().position;
                Quaternion portalTravellerRotationOld = portalTraveller.Value.GetTransform().rotation;
                portalTraveller.Value.Travel(transform, connectedPortal.transform, m.GetColumn(3));
                connectedPortal.OnTravellerEnterPortal(portalTraveller.Value);
                portalTraveller.Value.SetClonePosition(portalTravellerPositionOld, portalTravellerRotationOld);
                portalTravellersToRemove.Add(portalTraveller.Key);
            }
            else
            {
                portalTraveller.Value.SetPortalOffsetOld(portalOffset);
                portalTraveller.Value.SetClonePosition(m.GetColumn(3), m.rotation);
            }
        }

        for (int i=0; i<portalTravellersToRemove.Count; i++)
        {
            portalTravellers.Remove(portalTravellersToRemove[i]);
        }
    }

    private void UpdateSlicingForTraveller(IPortalTraveller portalTraveller)
    {
        Debug.Log("updating slicing for " + portalTraveller.GetTransform().gameObject.name);
        int side = Math.Sign(Vector3.Dot(portalTraveller.GetTransform().position - transform.position, transform.forward));
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = connectedPortal.transform.forward * side;

        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = connectedPortal.transform.position;

        float sliceOffsetDst = 0.0f;
        float cloneSliceOffsetDst = 0.0f;
        float portalRendererThickness = portalRenderer.transform.localScale.z;

        int playerSide = Math.Sign(Vector3.Dot(playerCamera.GetTransform().position - transform.position, transform.forward));
        bool isPlayerOnTheSameSideAsTraveller = playerSide == side;
        if (!isPlayerOnTheSameSideAsTraveller)
        {
            sliceOffsetDst = -portalRendererThickness;
        }

        int connectedPortalPlayerSide = Math.Sign(Vector3.Dot(playerCamera.GetTransform().position - connectedPortal.transform.position,
            connectedPortal.transform.forward));
        bool isPlayerOnTheSameSideAsTravellerConnectedPortal = connectedPortalPlayerSide == side;
        if (!isPlayerOnTheSameSideAsTravellerConnectedPortal)
        {
            cloneSliceOffsetDst = -portalRendererThickness;
        }

        List<Material> originalMaterials = portalTraveller.GetOriginalMaterials();
        List<Material> cloneMaterials = portalTraveller.GetCloneMaterials();

        Debug.Log(portalTraveller.GetTransform().gameObject.name + ": " + originalMaterials.Count);

        for (int i=0; i<originalMaterials.Count; i++)
        {
            originalMaterials[i].SetVector("sliceCenter", slicePos);
            originalMaterials[i].SetVector("sliceNormal", sliceNormal);
            originalMaterials[i].SetFloat("sliceOffsetDst", sliceOffsetDst);

            cloneMaterials[i].SetVector("sliceCenter", cloneSlicePos);
            cloneMaterials[i].SetVector("sliceNormal", cloneSliceNormal);
            cloneMaterials[i].SetFloat("sliceOffsetDst", cloneSliceOffsetDst);
        }
    }

    public void OnTravellerEnterPortal(IPortalTraveller portalTraveller)
    {
        if (portalTraveller.GetTransform().TryGetComponent<IUniqueIDProvider>(out IUniqueIDProvider uniqueIDProvider))
        {
            if (!portalTravellers.ContainsKey(uniqueIDProvider.GetUniqueID()))
            {
                portalTraveller.OnEnterPortal();
                portalTraveller.SetPortalOffsetOld(portalTraveller.GetTransform().position - transform.position);
                portalTravellers.Add(uniqueIDProvider.GetUniqueID(), portalTraveller);
            }
        }
    }

    private void ProtectScreenFromClipping(Vector3 viewPoint)
    {
        float halfHeight = playerCamera.GetCamera().nearClipPlane * Mathf.Tan(playerCamera.GetCamera().fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * playerCamera.GetCamera().aspect;
        float distanceToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCamera.GetCamera().nearClipPlane).magnitude;

        bool isCameraFacingSameDirectionAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
        portalRenderer.transform.localScale = new Vector3(portalRenderer.transform.localScale.x, portalRenderer.transform.localScale.y, distanceToNearClipPlaneCorner);
        portalRenderer.transform.localPosition = Vector3.forward * distanceToNearClipPlaneCorner * ((isCameraFacingSameDirectionAsPortal) ? 0.5f : -0.5f);
        portalRenderer.transform.localPosition = new Vector3(portalRenderer.transform.localPosition.x, 2.5f, portalRenderer.transform.localPosition.z);
    }

    public void OnInjected()
    {

    }
}
