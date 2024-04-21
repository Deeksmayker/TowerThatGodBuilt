using System.IO;
using Newtonsoft.Json;
using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.Objects;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class CubesDataController
    {
        private const string GRID_DATA_DIRECTION_NAME = "GridData";
        private readonly string _cubesDataPath;

        public CubesDataController()
        {
            _cubesDataPath = Path.Combine(Application.persistentDataPath, GRID_DATA_DIRECTION_NAME);
        }

        public void Save(Cube[] cubes, string name)
        {
            var cubesData = GetCubeData(cubes);
            var dataJson = JsonConvert.SerializeObject(cubesData, Formatting.Indented);

            if (!Directory.Exists(_cubesDataPath))
            {
                Directory.CreateDirectory(_cubesDataPath);
            }

            File.WriteAllText(Path.Combine(_cubesDataPath, name + ".json"), dataJson);
        }

        public CubeData[] Load(string name)
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

        private CubeData[] GetCubeData(Cube[] cubes)
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