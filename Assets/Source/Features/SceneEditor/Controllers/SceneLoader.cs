using System;
using System.Linq;
using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Objects;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

using static Source.Utils.Utils;

namespace Source.Features.SceneEditor.Controllers
{
    public static class SceneLoader
    {
        public static event Action<Transform> PlayerSpawnerFound;
        public static event Action<Transform> BallSpawnerFound;
        public static event Action<EnemyType, Transform> EnemySpawnerFound;
        
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
        
        private static void BuildGameLevel(string sceneName)
        {
            var cubesData = Load(sceneName);
            var cubes =  BuildLevel(cubesData);

            InitializePlayerCube(cubesData, cubes);
            
            for (int i = 0; i < cubes.Length; i++)
            {
                if (cubesData[i].Type == ECubeType.Enemy)
                {
                    EnemySpawnerFound?.Invoke(cubesData[i].EnemyType, cubes[i].transform);
                    
                    Object.Destroy(cubes[i].gameObject);
                }
                else if (cubesData[i].Type == ECubeType.Gate)
                {
                    // TODO: Вынести в другой класс
                    var gatePrefab = GetPrefab("WinGate");
                    Object.Instantiate(gatePrefab, cubes[i].transform.position, cubes[i].transform.rotation);
                    
                    Object.Destroy(cubes[i].gameObject);
                }
                else if (cubesData[i].Type == ECubeType.Ball)
                {
                    // TODO: Вынести в другой класс
                    /*var ballPrefab = GetPrefab("PlayerBall");
                    Object.Instantiate(ballPrefab, cubes[i].transform.position, cubes[i].transform.rotation);*/
                    
                    BallSpawnerFound?.Invoke(cubes[i].transform);
                    
                    Object.Destroy(cubes[i].gameObject);
                }
                else
                {
                    Object.Destroy(cubes[i]);
                }
            }
        }
        
        private static void InitializePlayerCube(CubeData[] cubesData, Cube[] cubes)
        {
            var playerCube = cubesData.First(x => x.Type == ECubeType.Player);
            var playerCubeIndex = cubesData.ToList().IndexOf(playerCube);
            
            PlayerSpawnerFound?.Invoke(cubes[playerCubeIndex].transform);
            
            Object.Destroy(cubes[playerCubeIndex].gameObject);
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