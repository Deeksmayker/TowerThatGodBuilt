using UnityEngine;
using UnityEngine.Rendering;

public class MeshCombiner : MonoBehaviour{
    public MeshFilter[] meshFilters;
    public MeshFilter targetMeshFilter;
    
    [ContextMenu("CombineMeshes")]
    public void CombineMeshes(){
    
        var combine = new CombineInstance[meshFilters.Length];
        
        for (int i = 0; i < meshFilters.Length; i++){
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        
        var mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.CombineMeshes(combine);
        targetMeshFilter.sharedMesh = null;
        targetMeshFilter.sharedMesh = mesh;
        targetMeshFilter.mesh = mesh;
    }
}
