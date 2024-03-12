using System.Linq;
using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Objects;
using Source.Features.SceneEditor.ScriptableObjects;
using Source.Features.SceneEditor.Utils;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class SceneEditorManager : MonoBehaviour
    {
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private GridController _gridController;

        [SerializeField] private ObjectPrefabsConfig _objectPrefabsConfig;

        private StateHandler<EBuildingState> _buildingStateController;
        private StateHandler<EInstrumentState> _instrumentController;

        private GameObject _currentObjectPrefab;
        private int _objectIndex;

        private void Start()
        {
            SceneLoader.SetObjectPrefabsConfig(_objectPrefabsConfig);

            _gridController.BuildGrid();
            _gridController.ShowGrid();

            InitializeBuilderStateController();
            InitializeInstrumentStateController();

            OnAlphaPressed(0);
        }

        private void OnEnable()
        {
            _inputHandler.AlphaButtonPressed += OnAlphaPressed;
            _gridController.CellClicked += OnCellClicked;

            _inputHandler.SpacePressed += SaveGrid;
            _inputHandler.EnterPressed += LoadGrid;
        }

        private void OnDisable()
        {
            _inputHandler.AlphaButtonPressed -= OnAlphaPressed;
            _gridController.CellClicked -= OnCellClicked;

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
            SceneLoader.SaveGrid(_gridController);
        }

        private void LoadGrid()
        {
            var cellsData = SceneLoader.LoadGrid();

            _gridController.ClearGrid();
            _gridController.BuildGrid();
            _gridController.ShowGrid();

            ResetBuilderStateController();
            InitializeBuilderStateController();

            ResetInstrumentController();
            InitializeInstrumentStateController();

            SceneLoader.BuildLevel(cellsData, _gridController);
        }

        private void ResetBuilderStateController()
        {
            _inputHandler.BuildingStateButtonPressed -= _buildingStateController.ChangeState;
            
            _buildingStateController.ChangeState((int)EBuildingState.Build);
        }

        private void InitializeBuilderStateController()
        {
            _buildingStateController =
                new StateHandler<EBuildingState>(ArrayUtils.GetFlatArray(_gridController.GetCells()));

            _inputHandler.BuildingStateButtonPressed += _buildingStateController.ChangeState;
        }

        private void ResetInstrumentController()
        {
            _inputHandler.InstrumentStateButtonPressed -= _instrumentController.ChangeState;

            _instrumentController.ChangeState((int)EInstrumentState.Default);
        }

        private void InitializeInstrumentStateController()
        {
            _instrumentController =
                new StateHandler<EInstrumentState>(ArrayUtils.GetFlatArray(_gridController.GetCells()));

            _inputHandler.InstrumentStateButtonPressed += _instrumentController.ChangeState;
        }
    }
}