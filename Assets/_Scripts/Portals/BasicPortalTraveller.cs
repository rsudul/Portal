using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BasicPortalTraveller : MonoBehaviour, IPortalTraveller
{
    private Vector3 portalOffsetOld = Vector3.zero;

    private GameObject clone = null;

    private List<Material> originalMaterials = new List<Material>();
    private List<Material> cloneMaterials = new List<Material>();

    public Vector3 GetPortalOffsetOld()
    {
        return portalOffsetOld;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void SetPortalOffsetOld(Vector3 offset)
    {
        portalOffsetOld = offset;
    }

    public void Travel(Transform sourcePortal, Transform destinationPortal, Vector3 connectedPortalPosition)
    {
        transform.position = connectedPortalPosition;
    }

    public void OnEnterPortal()
    {
        if (clone == null)
        {
            clone = Instantiate(gameObject);
            clone.GetComponent<Collider>().enabled = false;
            clone.transform.localScale = transform.localScale;
            originalMaterials = GraphicsUtility.GetAllMaterialsFromGameObjects(gameObject);
            cloneMaterials = GraphicsUtility.GetAllMaterialsFromGameObjects(clone);
        }
        else
        {
            clone.SetActive(true);
        }
    }

    public void OnExitPortal()
    {
        if (clone != null)
        {
            clone.SetActive(false);

            for (int i=0; i<originalMaterials.Count; i++)
            {
                originalMaterials[i].SetVector("sliceNormal", Vector3.zero);
            }
        }
    }

    public List<Material> GetOriginalMaterials()
    {
        return originalMaterials;
    }

    public List<Material> GetCloneMaterials()
    {
        return cloneMaterials;
    }

    public void SetClonePosition(Vector3 position, Quaternion rotation)
    {
        clone.transform.position = position;
        clone.transform.rotation = rotation;
    }

    public bool ShouldClone()
    {
        return true;
    }
}
