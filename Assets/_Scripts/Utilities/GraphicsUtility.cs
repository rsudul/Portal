using System.Collections.Generic;
using UnityEngine;

public static class GraphicsUtility
{
    public static List<Material> GetAllMaterialsFromGameObjects(params GameObject[] gos)
    {
        List<Material> allMaterials = new List<Material>();

        List<MeshRenderer> renderers = new List<MeshRenderer>();

        foreach (GameObject go in gos)
        {
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderers.Add(renderer);
            }

            foreach (MeshRenderer rendererInChildren in go.GetComponentsInChildren<MeshRenderer>())
            {
                renderers.Add(rendererInChildren);
            }
        }

        foreach (MeshRenderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                allMaterials.Add(mat);
            }
        }

        return allMaterials;
    }
}