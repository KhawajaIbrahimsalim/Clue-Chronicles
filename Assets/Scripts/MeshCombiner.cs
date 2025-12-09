using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    [SerializeField] private List<MeshFilter> sourceMeshFilter;
    [SerializeField] private MeshFilter targetMeshFilter;

    [ContextMenu("Combine Meshes")]
    public void CombineAllMeshes()
    {
        MeshFilter[] meshFilters = sourceMeshFilter.ToArray();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = Instantiate(meshFilters[i].sharedMesh);
            combine[i].transform =
                targetMeshFilter.transform.worldToLocalMatrix *
                meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // FIX

        mesh.CombineMeshes(combine, true, true);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        targetMeshFilter.sharedMesh = mesh;
        mesh.name = "CombinedMesh";

        AssetDatabase.CreateAsset(mesh, "Assets/CombinedMesh.asset");

        Debug.Log("Combined mesh created, vertex count = " + mesh.vertexCount);
    }
}
