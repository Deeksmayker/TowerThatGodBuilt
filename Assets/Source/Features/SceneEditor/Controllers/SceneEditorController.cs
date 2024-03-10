using Source.Features.SceneEditor.Enums;
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

        private BuildingStateController _buildingStateController;

        private GameObject _currentObjectPrefab;
        private int _objectIndex;

        private void Start()
        {
            SceneLoader.SetObjectPrefabsConfig(_objectPrefabsConfig);

            _grid.BuildGrid();
            _grid.ShowGrid();
            
            _buildingStateController =
                new BuildingStateController(_inputHandler, ArrayUtils.GetFlatArray(_grid.GetCells()));

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

        private void OnCellClicked(Cell cell, EBuildingState buildingState)
        {
            if (buildingState == EBuildingState.Build)
            {
                if (cell.GetIndexSpawnedObject() != -1)
                    return;
                
                cell.SetIndexSpawnedObject(_objectIndex);
                Instantiate(_currentObjectPrefab, cell.transform);
            }
            else
            {
                if (cell.GetIndexSpawnedObject() == -1)
                    return;
                
                cell.SetIndexSpawnedObject(-1);

                foreach (Transform x in cell.transform)
                {
                    Destroy(x.gameObject);
                }
            }
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

            _buildingStateController.ChangeState((int) EBuildingState.Build);
            _buildingStateController.Dispose();
            _buildingStateController =
                new BuildingStateController(_inputHandler, ArrayUtils.GetFlatArray(_grid.GetCells()));

            SceneLoader.BuildLevel(cellsData, _grid);
        }
    }
}