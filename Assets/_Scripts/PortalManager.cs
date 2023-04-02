using System;
using System.Collections.Generic;
using System.Linq;

public class PortalManager : InstancedSystem, IPortalManager
{
    public override Type Type => typeof(IPortalManager);

    private List<Portal> portals = new List<Portal>();

    private void Start()
    {
        portals = FindObjectsOfType<Portal>().ToList();
    }

    public List<Portal> GetAllPortals()
    {
        return portals;
    }
}
