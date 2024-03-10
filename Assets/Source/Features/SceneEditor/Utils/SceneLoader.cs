using System;
using Source.Features.SceneEditor.Controllers;
using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.ScriptableObjects;
using UnityEngine;
using Grid = Source.Features.SceneEditor.Objects.Grid;
using Object = UnityEngine.Object;

namespace Source.Features.SceneEditor.Utils
{
    public static class SceneLoader
    {
        private static ObjectPrefabsConfig _objectPrefabsConfig;

        public static void BuildLevel(Grid grid)
        {
            var cellsData = LoadGrid();
            
            BuildLevel(cellsData, grid);
        }
        
        public static void BuildLevel(CellData[,] cellsData, Grid grid)
        {
            var width = cellsData.GetLength(0);
            var height = cellsData.GetLength(1);
            
            var cells = grid.GetCells();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cellsData[x, y].IndexSpawnedObject != -1)
                    {
                        cells[x, y].SetIndexSpawnedObject(cellsData[x, y].IndexSpawnedObject);
                        cells[x, y].ResetMaterial();
                        
                        var spawnedObject = 
                            Object.Instantiate(_objectPrefabsConfig.GetObjectPrefabs()[cellsData[x, y].IndexSpawnedObject],
                                cells[x, y].transform);
                        spawnedObject.layer = 6;
                    }
                }
            }
        }
        
        public static void SaveGrid(Grid grid)
        {
            var gridDataController = new GridDataController();
            
            gridDataController.Save(grid.GetCells(), 0);
        }

        public static CellData[,] LoadGrid()
        {
            var gridDataController = new GridDataController();
            var cellsData = gridDataController.Load(0);

            if (cellsData == null)
            {
                throw new NullReferenceException();
            }
            
            return cellsData;
        }

        public static void SetObjectPrefabsConfig(ObjectPrefabsConfig objectPrefabsConfig)
        {
            _objectPrefabsConfig = objectPrefabsConfig;
        }
    }
}