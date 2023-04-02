using UnityEngine;

public interface IPlayerMovementController
{
    void UpdateMovement();
    Vector2 GetMoveInput();
    void PrepareForTeleport();
    void AfterTeleport();
    void SetMovement(Vector3 n_movement);
    Vector3 GetMovement();
}