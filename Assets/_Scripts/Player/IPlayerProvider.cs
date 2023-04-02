using UnityEngine;

public interface IPlayerProvider
{
    Transform GetTransform();
    bool IsMoving();
}