using System;
using Source.Features.SceneEditor.ScriptableObjects;
using UnityEngine;
using Object = UnityEngine.Object;

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
        
        private PlayerSpawner _playerSpawner;
        private Transform _player;

        private void OnEnable()
        {
            InitializePlayerSpawner();
            InitializeSceneLoader();
        }

        private void OnDisable()
        {
            if (_player)
                Destroy(_player.gameObject);
            SceneLoader.PlayerSpawnerFound -= OnPlayerSpawnerFound;
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
    }

    public class PlayerSpawner
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;

        public PlayerSpawner(GameObject prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }
        
        public Transform SpawnPlayer(Transform spawnPoint)
        {
            var player = Object.Instantiate(_prefab, spawnPoint.position, spawnPoint.rotation);
            player.transform.SetParent(_parent);
            
            return player.transform;
        }
    }
}