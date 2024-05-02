using System.Collections.Generic;
using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Interfaces;
using Source.Features.SceneEditor.Objects;
using Source.Features.SceneEditor.ScriptableObjects;
using Source.Features.SceneEditor.UI.Inspector;
using Source.Features.SceneEditor.UI.ModePanel;
using Source.Features.SceneEditor.UI.ObjectsPanel;
using Source.Features.SceneEditor.UI.QuickPlay;
using Source.Features.SceneEditor.UI.SavePanel;
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
        [SerializeField] private QuickPlayView _quickPlayView;

        [SerializeField] private CameraController _cameraController;
        
        [SerializeField] private InspectorView _inspectorView;
        [SerializeField] private ObjectsPanel _objectsPanel;
        
        private InspectorViewController _inspectorViewController;
        
        private SavePanelViewController _savePanelViewController;
        private LoadPanelViewController _loadPanelViewController;
        private QuickPlayViewController _quickPlayViewController;

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
            
            _savePanelViewController = new SavePanelViewController(_savePanelView);
            _loadPanelViewController = new LoadPanelViewController(_loadPanelView);
            _quickPlayViewController = new QuickPlayViewController(_quickPlayView);

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
            
            _savePanelViewController.Opened += OnPanelViewOpened;
            _loadPanelViewController.Opened += OnPanelViewOpened;
            
            _savePanelViewController.Closed += OnPanelViewClosed;
            _loadPanelViewController.Closed += OnPanelViewClosed;
            
            _savePanelViewController.SaveButtonClicked += SaveScene;
            _loadPanelViewController.LoadButtonClicked += LoadScene;
            _quickPlayViewController.ButtonClicked += OnQuickPlayButtonClicked;

            _selectController.SelectStateReset += OnSelectStateReset;
        }

        private void OnDisable()
        {
            _inputHandler.AlphaButtonPressed -= OnAlphaPressed;

            _savePanelViewController.Opened -= OnPanelViewOpened;
            _loadPanelViewController.Opened -= OnPanelViewOpened;
            
            _savePanelViewController.Closed -= OnPanelViewClosed;
            _loadPanelViewController.Closed -= OnPanelViewClosed;
            
            _savePanelViewController.SaveButtonClicked -= SaveScene;
            _loadPanelViewController.LoadButtonClicked -= LoadScene;
            _quickPlayViewController.ButtonClicked -= OnQuickPlayButtonClicked;
            
            _selectController.SelectStateReset -= OnSelectStateReset;
        }
        
        public void OnStateChange(EBuildingState state)
        {
            ResetSelectController();
        }

        private void OnQuickPlayButtonClicked()
        {
            var tempName = _cubes.GetHashCode().ToString();
            
            SaveScene(tempName);
            SceneLoader.LoadLevel(tempName);
        }
        
        private void OnPanelViewOpened()
        {
            _panelOpened = true;
            
            _cameraController.LockInput();
            _inputHandler.LockInput();
            _buildingStateController.ChangeState(EBuildingState.Disabled);
        }

        private void OnPanelViewClosed()
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
            _buildingStateController.AddListener(_objectsPanel);
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