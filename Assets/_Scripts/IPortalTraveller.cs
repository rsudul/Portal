using UnityEngine;

public interface IPortalTraveller
{
    Transform GetTransform();
    void Travel(Transform sourcePortal, Transform destinationPortal, Vector3 connectedPortalPosition);
    void SetPortalOffsetOld(Vector3 offset);
    Vector3 GetPortalOffsetOld();
}