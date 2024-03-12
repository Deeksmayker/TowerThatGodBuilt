using System;
using Source.Features.SceneEditor.Controllers;
using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.Objects;
using Source.Features.SceneEditor.ScriptableObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Source.Features.SceneEditor.Utils
{
    public static class SceneLoader
    {
        private static ObjectPrefabsConfig _objectPrefabsConfig;

        public static void BuildLevel(GridController gridController)
        {
            var cellsData = LoadGrid();
            
            BuildLevel(cellsData, gridController);
        }
        
        public static void BuildLevel(CellData[,] cellsData, GridController gridController)
        {
            var width = cellsData.GetLength(0);
            var height = cellsData.GetLength(1);
            
            var cells = gridController.GetCells();

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
        
        public static void SaveGrid(GridController gridController)
        {
            var gridDataController = new GridDataController();
            
            gridDataController.Save(gridController.GetCells(), 0);
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