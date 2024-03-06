using System;
using Source.Features.SceneEditor.ScriptableObjects;
using Source.Features.SceneEditor.Utils;
using UnityEngine;
using Grid = Source.Features.SceneEditor.Objects.Grid;

namespace Source
{
    public class EnvironmentController : MonoBehaviour
    {
        [SerializeField] private Grid _grid;
        [SerializeField] private ObjectPrefabsConfig _objectPrefabsConfig;
        
        private void Start()
        {
            _grid.BuildGrid();
            _grid.transform.position = new Vector3(
                -_grid.GetWidth() / 2f * _grid.GetCellSize(),
                0,
                -_grid.GetHeight() / 2f * _grid.GetCellSize());
            
            SceneLoader.SetObjectPrefabsConfig(_objectPrefabsConfig);
            SceneLoader.BuildLevel(_grid);
        }
    }
}