using System;
using Source.Features.SceneEditor.Objects;
using UnityEngine;
using Grid = Source.Features.SceneEditor.Objects.Grid;

namespace Source.Features.SceneEditor.Controllers
{
    public class SpawningController : MonoBehaviour
    {
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private Grid _grid;
        
        [SerializeField] private GameObject[] _objectPrefabs;

        private GridDataController _gridDataController;
        private GameObject _currentObjectPrefab;
        private int _objectIndex;

        private void Awake()
        {
            _gridDataController = new GridDataController();
        }

        private void Start()
        {
            _grid.BuildGrid();
            
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
            
            if (objectIndex < 0 || objectIndex >= _objectPrefabs.Length)
            {
                Debug.LogError($"Pressed invalid alpha button.\n" +
                               $"Current: {objectIndex}, but max: {_objectPrefabs.Length - 1}");
                return;
            }

            _currentObjectPrefab = _objectPrefabs[objectIndex];
        }

        private void OnCellClicked(Cell cell)
        {
            cell.SetIndexSpawnedObject(_objectIndex);
            Instantiate(_currentObjectPrefab, cell.transform);
        }

        private void SaveGrid()
        {
            _gridDataController.Save(_grid.GetCells(), 0);
        }

        private void LoadGrid()
        {
            var cellsData = _gridDataController.Load(0);

            if (cellsData == null)
                return;
            
            _grid.ClearGrid();
            _grid.BuildGrid();
            
            var cells = _grid.GetCells();

            for (int x = 0; x < cellsData.GetLength(0); x++)
            {
                for (int y = 0; y < cellsData.GetLength(1); y++)
                {
                    if (cellsData[x, y].IndexSpawnedObject != -1)
                    {
                        cells[x, y].SetIndexSpawnedObject(cellsData[x, y].IndexSpawnedObject);
                        cells[x, y].Hide();
                        
                        Instantiate(_objectPrefabs[cellsData[x, y].IndexSpawnedObject], cells[x, y].transform);
                    }
                }
            }
        }
    }
}