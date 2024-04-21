﻿using System;
using System.Collections.Generic;
using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Objects;
using Source.Features.SceneEditor.ScriptableObjects;
using Source.Features.SceneEditor.UI.SavePanel;
using Source.Features.SceneEditor.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class SceneEditorManager : MonoBehaviour
    {
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private MouseHandler _mouseHandler;

        [SerializeField] private ObjectPrefabsConfig _objectPrefabsConfig;
        [SerializeField] private Transform _parentTransform;

        [SerializeField] private SavePanelView _savePanelView;
        [SerializeField] private LoadPanelView _loadPanelView;

        [SerializeField] private CameraController _cameraController;
        
        private SavePanelController _savePanelController;
        private LoadPanelController _loadPanelController;
        
        private List<Cube> _cubes;        
        
        private StateHandler<EBuildingState> _buildingStateController;
        private StateHandler<EInstrumentState> _instrumentController;

        private CubeFabric _cubeFabric;
        private int _objectIndex;

        private void Awake()
        {
            _cubes = new List<Cube>();
            _cubeFabric = new CubeFabric(_objectPrefabsConfig, _parentTransform, _mouseHandler);
            
            _savePanelController = new SavePanelController(_savePanelView);
            _loadPanelController = new LoadPanelController(_loadPanelView);
        }

        private void Start()
        {
            SceneLoader.Construct(_cubeFabric);
            
            SceneLoader.SetObjectPrefabsConfig(_objectPrefabsConfig);
            
            InitializeBuilderStateController();
            InitializeInstrumentStateController();
            
            OnAlphaPressed(0);
            _cubes.Add(SpawnCube(transform));
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
        }
        
        private void OnPanelOpened()
        {
            _cameraController.LockInput();
            _inputHandler.LockInput();
            _buildingStateController.ChangeState(EBuildingState.Disabled);
        }

        private void OnPanelClosed()
        {
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
            var cube = _cubeFabric.SpawnCube(_objectIndex, spawnTransform.position, spawnTransform.rotation);
            
            _buildingStateController.AddListener(cube);
            cube.OnStateChange(_buildingStateController.GetCurrentState());
            
            cube.BuildMouseLeftButtonClicked += OnBuildMouseLeftButtonClicked;
            cube.DestroyMouseLeftButtonClicked += OnDestroyMouseLeftButtonClicked;
            

            return cube;
        }

        private void OnBuildMouseLeftButtonClicked(Transform cubeTransform)
        {
            _cubes.Add(SpawnCube(cubeTransform));
        }
        
        private void OnDestroyMouseLeftButtonClicked(Cube cube)
        {
            _cubes.Remove(cube);
            Destroy(cube.gameObject);
        }

        private void SaveScene(string sceneName)
        {
            SceneLoader.Save(_cubes.ToArray(), sceneName);
        }

        private void LoadScene(string sceneName)
        {
            _cubes.Clear();
            SceneLoader.ClearLevel();
            
            _cubes.AddRange(SceneLoader.BuildLevel(sceneName));

            for (int i = 0; i < _cubes.Count; i++)
            {
                _buildingStateController.AddListener(_cubes[i]);
                _cubes[i].BuildMouseLeftButtonClicked += OnBuildMouseLeftButtonClicked;
                _cubes[i].DestroyMouseLeftButtonClicked += OnDestroyMouseLeftButtonClicked;
            }
            
            ResetBuilderStateController();
            InitializeBuilderStateController();

            ResetInstrumentController();
            InitializeInstrumentStateController();
        }

        private void ResetBuilderStateController()
        {
            _inputHandler.BuildingStateButtonPressed -= _buildingStateController.ChangeState;
            
            _buildingStateController.ChangeState(EBuildingState.Build);
        }

        private void InitializeBuilderStateController()
        {
            _buildingStateController =
                new StateHandler<EBuildingState>(_cubes);

            _inputHandler.BuildingStateButtonPressed += _buildingStateController.ChangeState;
        }

        private void ResetInstrumentController()
        {
            /*_inputHandler.InstrumentStateButtonPressed -= _instrumentController.ChangeState;

            _instrumentController.ChangeState((int)EInstrumentState.Default);*/
        }

        private void InitializeInstrumentStateController()
        {
            /*_instrumentController =
                new StateHandler<EInstrumentState>(_cubes);

            _inputHandler.InstrumentStateButtonPressed += _instrumentController.ChangeState;*/
        }
    }
}