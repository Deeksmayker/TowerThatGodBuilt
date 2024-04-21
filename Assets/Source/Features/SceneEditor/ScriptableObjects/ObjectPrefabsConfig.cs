using Source.Features.SceneEditor.Objects;
using UnityEditor;
using UnityEngine;

namespace Source.Features.SceneEditor.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ObjectPrefabConfig", menuName = "Configs/ObjectPrefabConfig", order = 0)]
    public class ObjectPrefabsConfig : ScriptableObject
    {
        [SerializeField] private Cube[] _objectPrefabs;
        [SerializeField] private GameObject _buildingGhostCubePrefab;
        [SerializeField] private GameObject _destroyingGhostCubePrefab;

        public Cube[] GetObjectPrefabs()
        {
            return _objectPrefabs;
        }
        
        public GameObject GetBuildingGhostCubePrefab()
        {
            return _buildingGhostCubePrefab;
        }
        
        public GameObject GetDestroyingGhostCubePrefab()
        {
            return _destroyingGhostCubePrefab;
        }
    }
}