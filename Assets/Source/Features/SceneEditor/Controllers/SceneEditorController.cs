using System;
using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.Objects;
using Source.Features.SceneEditor.ScriptableObjects;
using Source.Features.SceneEditor.Utils;
using UnityEngine;
using Grid = Source.Features.SceneEditor.Objects.Grid;

namespace Source.Features.SceneEditor.Controllers
{
    public class SceneEditorController : MonoBehaviour
    {
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private Grid _grid;
        
        [SerializeField] private ObjectPrefabsConfig _objectPrefabsConfig;
        
        private GameObject _currentObjectPrefab;
        private int _objectIndex;

        private void Start()
        {
            SceneLoader.SetObjectPrefabsConfig(_objectPrefabsConfig);
            
            _grid.BuildGrid();
            _grid.ShowGrid();
            OnAlphaPressed(0);
        }

        private void OnEnable()
        {
            _inputHandler.AlphaButtonPressed += OnAlphaPressed;
            _grid.CellClicked += OnCellClicked;

            _inputHandler.SpacePressed += SaveGrid;
            _inputHandler.EnterPressed += LoadGrid;
        }

        private void OnDisable()
        {
            _inputHandler.AlphaButtonPressed -= OnAlphaPressed;
            _grid.CellClicked -= OnCellClicked;
            
            _inputHandler.SpacePressed -= SaveGrid;
            _inputHandler.EnterPressed -= LoadGrid;
        }

        private void OnAlphaPressed(int objectIndex)
        {
            _objectIndex = objectIndex;

            var objectPrefabs = _objectPrefabsConfig.GetObjectPrefabs();
            
            if (objectIndex < 0 || objectIndex >= objectPrefabs.Length)
            {
                Debug.LogError($"Pressed invalid alpha button.\n" +
                               $"Current: {objectIndex}, but max: {objectPrefabs.Length - 1}");
                return;
            }

            _currentObjectPrefab = objectPrefabs[objectIndex];
        }

        private void OnCellClicked(Cell cell)
        {
            cell.SetIndexSpawnedObject(_objectIndex);
            Instantiate(_currentObjectPrefab, cell.transform);
        }

        private void SaveGrid()
        {
            SceneLoader.SaveGrid(_grid);
        }

        private void LoadGrid()
        {
            var cellsData = SceneLoader.LoadGrid();
            
            _grid.ClearGrid();
            _grid.BuildGrid();
            _grid.ShowGrid();
            
            SceneLoader.BuildLevel(cellsData, _grid);
        }
    }
}