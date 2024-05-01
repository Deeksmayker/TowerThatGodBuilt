using System;
using System.Collections.Generic;
using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Interfaces;
using Source.Features.SceneEditor.Objects;
using Source.Features.SceneEditor.ScriptableObjects;
using Source.Features.SceneEditor.UI.Inspector;
using Source.Features.SceneEditor.UI.ModePanel;
using Source.Features.SceneEditor.UI.SavePanel;
using Source.Features.SceneEditor.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    // TODO: refactoring to Bootstrapper
    public class SceneEditorManager : MonoBehaviour, IChangeStateListener<EBuildingState>
    {
        [SerializeField] private InputHandler _inputHandler;

        [SerializeField] private ObjectPrefabsConfig _objectPrefabsConfig;
        [SerializeField] private Transform _parentTransform;

        [SerializeField] private TextView _currentInstrumentTextView;
        [SerializeField] private TextView _currentModeTextView;
        [SerializeField] private SavePanelView _savePanelView;
        [SerializeField] private LoadPanelView _loadPanelView;

        [SerializeField] private CameraController _cameraController;
        
        [SerializeField] private InspectorView _inspectorView;
        
        private InspectorViewController _inspectorViewController;
        
        private SavePanelController _savePanelController;
        private LoadPanelController _loadPanelController;

        private BuildingModeViewController _buildingModeViewController;
        private InstrumentModeViewController _instrumentModeViewController;
        
        private List<Cube> _cubes;        
        
        private StateHandler<EBuildingState> _buildingStateController;
        private StateHandler<EInstrumentState> _instrumentController;
        private SelectController _selectController;

        private CubeFactory _cubeFactory;
        private int _objectIndex;

        private bool _panelOpened;

        private void Awake()
        {
            _cubes = new List<Cube>();
            _cubeFactory = new CubeFactory(_objectPrefabsConfig, _parentTransform);
            
            _savePanelController = new SavePanelController(_savePanelView);
            _loadPanelController = new LoadPanelController(_loadPanelView);

            _buildingModeViewController = new BuildingModeViewController(_currentModeTextView);
            _instrumentModeViewController = new InstrumentModeViewController(_currentInstrumentTextView);

            _inspectorViewController = new InspectorViewController(_inspectorView);
            
            InitializeBuilderStateController();
            InitializeInstrumentStateController();
            InitializeSelectController();
        }

        private void Start()
        {
            SceneLoader.Construct(_cubeFactory);
            
            OnAlphaPressed(0);
            _cubes.Add(SpawnCube(transform));

            _buildingStateController.AddListener(_buildingModeViewController);
            _instrumentController.AddListener(_instrumentModeViewController);
        }

        private void OnEnable()
        {
            _inputHandler.AlphaButtonPressed += OnAlphaPressed;
            
            _savePanelController.Opened += OnPanelOpened;
            _loadPanelController.Opened += OnPanelOpened;
            
            _savePanelController.Closed += OnPanelClosed;
            _loadPanelController.Closed += OnPanelClosed;
            
            _savePanelController.SaveButtonClicked += SaveScene;
            _loadPanelController.LoadButtonClicked += LoadScene;

            _selectController.SelectStateReset += OnSelectStateReset;
        }

        private void OnDisable()
        {
            _inputHandler.AlphaButtonPressed -= OnAlphaPressed;

            _savePanelController.Opened -= OnPanelOpened;
            _loadPanelController.Opened -= OnPanelOpened;
            
            _savePanelController.Closed -= OnPanelClosed;
            _loadPanelController.Closed -= OnPanelClosed;
            
            _savePanelController.SaveButtonClicked -= SaveScene;
            _loadPanelController.LoadButtonClicked -= LoadScene;
            
            _selectController.SelectStateReset -= OnSelectStateReset;
        }
        
        public void OnStateChange(EBuildingState state)
        {
            ResetSelectController();
        }
        
        private void OnPanelOpened()
        {
            _panelOpened = true;
            
            _cameraController.LockInput();
            _inputHandler.LockInput();
            _buildingStateController.ChangeState(EBuildingState.Disabled);
        }

        private void OnPanelClosed()
        {
            _panelOpened = false;
            
            _cameraController.UnlockInput();
            _inputHandler.UnlockInput();
        }

        private void OnAlphaPressed(int objectIndex)
        {
            _objectIndex = objectIndex;

            var objectPrefabs = _objectPrefabsConfig.GetObjectPrefabs();

            if (objectIndex < 0 || objectIndex >= objectPrefabs.Length)
            {
                Debug.LogError($"Pressed invalid alpha button.\n" +
                               $"Current: {objectIndex}, but max: {objectPrefabs.Length - 1}");
            }
        }

        private Cube SpawnCube(Transform spawnTransform)
        {
            var cube = _cubeFactory.SpawnCube(_objectIndex, spawnTransform.position, spawnTransform.rotation);
            
            cube.Construct(_objectPrefabsConfig.GetBuildingGhostCubePrefab(), 
                _objectPrefabsConfig.GetDestroyingGhostCubePrefab(),
                _objectIndex);
            
            _buildingStateController.AddListener(cube);
            cube.OnStateChange(_buildingStateController.GetCurrentState());
            
            _selectController.AddSelectListener(cube);
            cube.Selected += OnSelected;
            
            cube.BuildMouseLeftButtonClicked += OnBuildMouseLeftButtonClicked;
            cube.DestroyMouseLeftButtonClicked += OnDestroyMouseLeftButtonClicked;
            
            return cube;
        }

        private void OnSelected(ISelectListener listener)
        {
            if (_panelOpened) return;
            
            _selectController.ChangeSelected(listener);
            
            var selectedTransform = listener.GetTransform();
            _inspectorViewController.Construct(selectedTransform, listener.GetType() != ECubeType.Cube);

            _inspectorViewController.Show();
        }

        private void OnBuildMouseLeftButtonClicked(Transform cubeTransform)
        {
            _cubes.Add(SpawnCube(cubeTransform));
        }
        
        private void OnDestroyMouseLeftButtonClicked(Cube cube)
        {
            _selectController.RemoveSelectListener(cube);
            _cubes.Remove(cube);
            Destroy(cube.gameObject);
        }

        private void SaveScene(string sceneName)
        {
            SceneLoader.Save(_cubes.ToArray(), sceneName);
        }

        private void LoadScene(string sceneName)
        {
            for (int i = 0; i < _cubes.Count; i++)
            {
                _selectController.RemoveSelectListener(_cubes[i]);
            }
            
            _cubes.Clear();
            SceneLoader.ClearLevel();
            
            _cubes.AddRange(SceneLoader.BuildLevel(sceneName));

            for (int i = 0; i < _cubes.Count; i++)
            {
                _cubes[i].Construct(_objectPrefabsConfig.GetBuildingGhostCubePrefab(),
                    _objectPrefabsConfig.GetDestroyingGhostCubePrefab(), 
                    _objectIndex);
                
                _buildingStateController.AddListener(_cubes[i]);
                _cubes[i].BuildMouseLeftButtonClicked += OnBuildMouseLeftButtonClicked;
                _cubes[i].DestroyMouseLeftButtonClicked += OnDestroyMouseLeftButtonClicked;
                
                _selectController.AddSelectListener(_cubes[i]);
                _cubes[i].Selected += OnSelected;
            }
            
            ResetBuilderStateController();
            InitializeBuilderStateController();

            ResetInstrumentController();
            InitializeInstrumentStateController();

            InitializeSelectController();
            ResetSelectController();
        }

        private void ResetBuilderStateController()
        {
            _inputHandler.BuildingStateButtonPressed -= _buildingStateController.ChangeState;
            
            _buildingStateController.ChangeState(EBuildingState.Disabled);
        }

        private void InitializeBuilderStateController()
        {
            _buildingStateController ??= new StateHandler<EBuildingState>(_cubes);

            _inputHandler.BuildingStateButtonPressed += _buildingStateController.ChangeState;
            
            _buildingStateController.AddListener(this);
        }

        private void ResetInstrumentController()
        {
            _inputHandler.InstrumentStateButtonPressed -= _instrumentController.ChangeState;

            _instrumentController.ChangeState((int)EInstrumentState.Default);
        }

        private void InitializeInstrumentStateController()
        {
            _instrumentController ??= new StateHandler<EInstrumentState>(_cubes);

            _inputHandler.InstrumentStateButtonPressed += _instrumentController.ChangeState;
        }
        
        private void ResetSelectController()
        {
            _selectController.ResetSelectState();
        }

        private void OnSelectStateReset()
        {
            _inspectorViewController.Hide();
        }

        private void InitializeSelectController()
        {
            _selectController ??= new SelectController(_cubes);
        }
    }
}