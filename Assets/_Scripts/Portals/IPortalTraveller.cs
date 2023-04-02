using System.Collections.Generic;
using UnityEngine;

public interface IPortalTraveller
{
    Transform GetTransform();
    void Travel(Transform sourcePortal, Transform destinationPortal, Vector3 connectedPortalPosition);
    void SetPortalOffsetOld(Vector3 offset);
    Vector3 GetPortalOffsetOld();
    void OnEnterPortal();
    List<Material> GetOriginalMaterials();
    List<Material> GetCloneMaterials();
    void SetClonePosition(Vector3 position, Quaternion rotation);
    void OnExitPortal();
    bool ShouldClone();
}