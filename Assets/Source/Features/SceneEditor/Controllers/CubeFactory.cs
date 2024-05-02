using Source.Features.SceneEditor.Objects;
using Source.Features.SceneEditor.ScriptableObjects;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class CubeFactory
    {
        // TODO: получать конфиг через DI
        private readonly ObjectPrefabsConfig _config;
        private readonly Transform _parent;

        public CubeFactory(ObjectPrefabsConfig config, Transform parent)
        {
            _config = config;
            _parent = parent;
        }
        
        public Cube SpawnCube(int prefabIndex, Vector3 position, Quaternion rotation)
        {
            var prefab = _config.GetObjectPrefabs()[prefabIndex];
            var cube = Object.Instantiate(prefab, position, rotation);
            cube.transform.SetParent(_parent);
            
            cube.Construct(_config.GetBuildingGhostCubePrefab(),
                _config.GetDestroyingGhostCubePrefab(), prefabIndex);
            
            return cube;
        }

        public Transform GetParentTransform()
        {
            return _parent;
        }
    }
}