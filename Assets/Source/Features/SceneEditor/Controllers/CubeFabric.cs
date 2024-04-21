using Source.Features.SceneEditor.Objects;
using Source.Features.SceneEditor.ScriptableObjects;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class CubeFabric
    {
        private readonly ObjectPrefabsConfig _config;
        private readonly Transform _parent;
        private readonly MouseHandler _mouseHandler;

        public CubeFabric(ObjectPrefabsConfig config, Transform parent, MouseHandler mouseHandler)
        {
            _config = config;
            _parent = parent;
            _mouseHandler = mouseHandler;
        }
        
        public Cube SpawnCube(int prefabIndex, Vector3 position, Quaternion rotation)
        {
            var prefab = _config.GetObjectPrefabs()[prefabIndex];
            
            var cube = Object.Instantiate(prefab, position, rotation);
            
            cube.Construct(_mouseHandler, 
                _config.GetBuildingGhostCubePrefab(),
                _config.GetDestroyingGhostCubePrefab(), prefabIndex);
            cube.transform.SetParent(_parent);
            
            return cube;
        }

        public Transform GetParentTransform()
        {
            return _parent;
        }
    }
}