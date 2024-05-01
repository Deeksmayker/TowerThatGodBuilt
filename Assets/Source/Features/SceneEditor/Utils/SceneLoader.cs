using System;
using Source.Features.SceneEditor.Controllers;
using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Objects;
using Source.Features.SceneEditor.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Source.Features.SceneEditor.Utils
{
    public static class SceneLoader
    {
        public static event Action<Transform> PlayerSpawnerFound;
        
        private const string CONSTRUCTED_SCENE_LEVEL = "ConstructedSceneLevel";
        private static CubeFactory _cubeFactory; 
        
        public static void Construct(CubeFactory cubeFactory)
        {
            _cubeFactory = cubeFactory;
        }

        public static void LoadLevel(string sceneName)
        {
            var cubesDataController = new CubesDataController();

            if (cubesDataController.LevelExists(sceneName))
            {
                SceneManager.LoadSceneAsync(CONSTRUCTED_SCENE_LEVEL)
                    .completed += _ => BuildGameLevel(sceneName);
            }
            else
            {
                Debug.LogError("Could not load level " + sceneName);
            }
        }

        public static void BuildGameLevel(string sceneName)
        {
            var cubesData = Load(sceneName);
            var cubes =  BuildLevel(cubesData);
            
            for (int i = 0; i < cubes.Length; i++)
            {
                if (cubesData[i].Type == ECubeType.Player)
                {
                    PlayerSpawnerFound?.Invoke(cubes[i].transform);

                    Object.Destroy(cubes[i].gameObject);
                }
                else if (cubesData[i].Type == ECubeType.Enemy)
                {
                    // Спавним врага вместо куба
                }
                else
                {
                    Object.Destroy(cubes[i]);
                }
            }
        }
        
        public static Cube[] BuildLevel(string sceneName)
        {
            var cubesData = Load(sceneName);
            
            return BuildLevel(cubesData);
        }

        public static void ClearLevel()
        {
            foreach (Transform child in _cubeFactory.GetParentTransform())
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

                cubes[i] = _cubeFactory.SpawnCube(cubesData[i].PrefabIndex, position, rotation);
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
    }
}