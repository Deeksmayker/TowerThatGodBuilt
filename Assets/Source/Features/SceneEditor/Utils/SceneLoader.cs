using System;
using Source.Features.SceneEditor.Controllers;
using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.Objects;
using Source.Features.SceneEditor.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Source.Features.SceneEditor.Utils
{
    public static class SceneLoader
    {
        private static ObjectPrefabsConfig _objectPrefabsConfig;

        private static CubeFabric _cubeFabric; 
        
        public static void Construct(CubeFabric cubeFabric)
        {
            _cubeFabric = cubeFabric;
        }
        
        public static Cube[] BuildLevel(string sceneName)
        {
            var cubesData = Load(sceneName);
            
            return BuildLevel(cubesData);
        }

        public static void ClearLevel()
        {
            foreach (Transform child in _cubeFabric.GetParentTransform())
            {
                Object.Destroy(child.gameObject);
            }
        }
        
        private static Cube[] BuildLevel(CubeData[] cubesData)
        {
            var cubes = new Cube[cubesData.Length];
            
            for (int i = 0; i < cubesData.Length; i++)
            {
                var position = new Vector3(cubesData[i].X, cubesData[i].Y, cubesData[i].Z);
                var rotation = Quaternion.Euler(cubesData[i].XRotation, cubesData[i].YRotation, cubesData[i].ZRotation);

                cubes[i] = _cubeFabric.SpawnCube(cubesData[i].PrefabIndex, position, rotation);
            }

            return cubes;
        }
        
        public static void Save(Cube[] cubes, string name)
        {
            var cubesDataController = new CubesDataController();
            
            cubesDataController.Save(cubes, name);
        }

        private static CubeData[] Load(string name)
        {
            var cubesDataController = new CubesDataController();
            var cubesData = cubesDataController.Load(name);

            if (cubesData == null)
            {
                throw new NullReferenceException();
            }
            
            return cubesData;
        }

        public static void SetObjectPrefabsConfig(ObjectPrefabsConfig objectPrefabsConfig)
        {
            _objectPrefabsConfig = objectPrefabsConfig;
        }
    }
}