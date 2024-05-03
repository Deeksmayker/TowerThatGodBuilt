using System;
using System.IO;
using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Source.Features.SceneEditor.Controllers
{
    public class SceneBootstrapper : MonoBehaviour
    {
        [Header("Scene Loader Data")]
        [SerializeField] private ObjectPrefabsConfig _objectPrefabsConfig;
        [SerializeField] private Transform _spawnedObjectsParent;

        [Header("Player Spawn Data")] 
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform _playerParentObject;

        [Header("Buttons Data")] 
        [SerializeField] private InputHandler _inputHandler;
        
        private PlayerSpawner _playerSpawner;
        private Transform _player;

        private void OnEnable()
        {
            InitializePlayerSpawner();
            InitializeSceneLoader();
            InitializeButtons();
        }

        private void OnDisable()
        {
            if (_player)
                Destroy(_player.gameObject);
            
            SceneLoader.PlayerSpawnerFound -= OnPlayerSpawnerFound;
            _inputHandler.BackspaceButtonPressed -= OnBackspaceButtonPressed;
        }

        private void InitializePlayerSpawner()
        {
            _playerSpawner = new PlayerSpawner(_playerPrefab, _playerParentObject);

            SceneLoader.PlayerSpawnerFound += OnPlayerSpawnerFound;
        }

        private void OnPlayerSpawnerFound(Transform spawnPoint)
        {
            _player = _playerSpawner.SpawnPlayer(spawnPoint);
        }

        private void InitializeSceneLoader()
        {
            var cubeFactory = new CubeFactory(_objectPrefabsConfig, _spawnedObjectsParent);
            SceneLoader.Construct(cubeFactory);
        }

        private void InitializeButtons()
        {
            _inputHandler.BackspaceButtonPressed += OnBackspaceButtonPressed;
        }

        private void OnBackspaceButtonPressed()
        {
            if (CubesDataController.LevelExists(SceneEditorConstants.QUICK_PLAY_SCENE_NAME))
            {
                SceneManager.LoadSceneAsync(SceneEditorConstants.SCENE_EDITOR_SCENE_NAME);
            }
        }
    }
}