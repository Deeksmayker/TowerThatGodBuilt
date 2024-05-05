using System.IO;
using Newtonsoft.Json;
using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.Objects;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public static class CubesDataController
    {
        #if !UNITY_EDITOR
        private static readonly string _cubesDataPath = Path.Combine(Application.dataPath, "LevelData");
        #else
        private static readonly string _cubesDataPath = Path.Combine(Application.persistentDataPath, "LevelData");
        #endif

        public static void Save(Cube[] cubes, string name)
        {
            var cubesData = GetCubeData(cubes);
            var dataJson = JsonConvert.SerializeObject(cubesData, Formatting.Indented);

            if (!Directory.Exists(_cubesDataPath))
            {
                Directory.CreateDirectory(_cubesDataPath);
            }

            File.WriteAllText(Path.Combine(_cubesDataPath, name + ".json"), dataJson);
        }

        public static CubeData[] Load(string name)
        {
            if (!Directory.Exists(_cubesDataPath) || !File.Exists(Path.Combine(_cubesDataPath, name + ".json")))
            {
                Debug.LogError("404: Not Found.");
                return null;
            }
            
            var dataJson = File.ReadAllText(Path.Combine(_cubesDataPath, name + ".json"));
            var cellsData = JsonConvert.DeserializeObject<CubeData[]>(dataJson);

            return cellsData;
        }

        public static void Delete(string name)
        {
            if (!Directory.Exists(_cubesDataPath) || !File.Exists(Path.Combine(_cubesDataPath, name + ".json")))
            {
                Debug.LogError("404: Not Found.");
                return;
            }

            File.Delete(Path.Combine(_cubesDataPath, name + ".json"));
        }

        public static bool LevelExists(string name)
        {
            return Directory.Exists(_cubesDataPath) && File.Exists(Path.Combine(_cubesDataPath, name + ".json"));
        }

        private static CubeData[] GetCubeData(Cube[] cubes)
        {
            var result = new CubeData[cubes.Length];

            for (int i = 0; i < result.GetLength(0); i++)
            {
                result[i] = cubes[i].GetData();
            }

            return result;
        }
    }
}