using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IPlayerMovementController))]
public class Player : InstancedSystem, IPlayerProvider, IPortalTraveller
{
    public override Type Type => typeof(IPlayerProvider);

    private IPlayerMovementController playerMovementController;

    private Vector3 portalOffsetOld = Vector3.zero;

    private bool shouldPortalTravellerClone = false;

    private void Awake()
    {
        if (!TryGetComponent<IPlayerMovementController>(out playerMovementController))
        {
            Debug.LogError(gameObject.name + ": IPlayerMovementController not found.");
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public bool IsMoving()
    {
        return playerMovementController.GetMoveInput().x != 0.0f || playerMovementController.GetMoveInput().y != 0.0f;
    }

    public void Travel(Transform sourcePortal, Transform destinationPortal, Vector3 connectedPortalPosition)
    {
        //playerMovementController.PrepareForTeleport();
        transform.position = connectedPortalPosition;
        playerMovementController.SetMovement(destinationPortal.TransformVector(sourcePortal.InverseTransformVector(playerMovementController.GetMovement())));
        Physics.SyncTransforms();
        //playerMovementController.AfterTeleport();
    }

    public void SetPortalOffsetOld(Vector3 offset)
    {
        portalOffsetOld = offset;
    }

    public Vector3 GetPortalOffsetOld()
    {
        return portalOffsetOld;
    }

    public void OnEnterPortal()
    {

    }

    public void OnExitPortal()
    {

    }

    public List<Material> GetOriginalMaterials()
    {
        return null;
    }

    public List<Material> GetCloneMaterials()
    {
        return null;
    }

    public void SetClonePosition(Vector3 position, Quaternion rotation)
    {

    }

    public bool ShouldClone()
    {
        return shouldPortalTravellerClone;
    }
}