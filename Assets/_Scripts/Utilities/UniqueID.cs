using System;
using UnityEngine;

public class UniqueID : MonoBehaviour, IUniqueIDProvider
{
    private Guid uniqueID = Guid.Empty;

    private void Awake()
    {
        uniqueID = new Guid();
    }

    public Guid GetUniqueID()
    {
        return uniqueID;
    }
}