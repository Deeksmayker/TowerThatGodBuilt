using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
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