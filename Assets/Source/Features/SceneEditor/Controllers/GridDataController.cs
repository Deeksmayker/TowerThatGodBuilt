using System.IO;
using Newtonsoft.Json;
using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.Objects;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class GridDataController
    {
        private const string GRID_DATA_DIRECTION_NAME = "GridData";
        private readonly string _gridDataPath;

        public GridDataController()
        {
            _gridDataPath = Path.Combine(Application.persistentDataPath, GRID_DATA_DIRECTION_NAME);
        }

        public void Save(Cell[,] cells, int index)
        {
            var cellsData = GetCellData(cells);
            var dataJson = JsonConvert.SerializeObject(cellsData, Formatting.Indented);

            if (!Directory.Exists(_gridDataPath))
            {
                Directory.CreateDirectory(_gridDataPath);
            }

            File.WriteAllText(Path.Combine(_gridDataPath, index + ".json"), dataJson);
        }

        public CellData[,] Load(int index)
        {
            if (!Directory.Exists(_gridDataPath) || !File.Exists(Path.Combine(_gridDataPath, index + ".json")))
            {
                Debug.LogError("404: Not Found.");
                return null;
            }
            
            var dataJson = File.ReadAllText(Path.Combine(_gridDataPath, index + ".json"));
            var cellsData = JsonConvert.DeserializeObject<CellData[,]>(dataJson);

            return cellsData;
        }

        private CellData[,] GetCellData(Cell[,] cells)
        {
            var result = new CellData[cells.GetLength(0), cells.GetLength(1)];

            for (int x = 0; x < result.GetLength(0); x++)
            {
                for (int y = 0; y < result.GetLength(1); y++)
                {
                    result[x, y] = new CellData(cells[x, y].transform.position,
                        cells[x, y].transform.rotation,
                        cells[x, y].GetIndexSpawnedObject());
                }
            }

            return result;
        }
    }
}