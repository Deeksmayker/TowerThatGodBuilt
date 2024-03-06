using UnityEditor;
using UnityEngine;

namespace Source.Features.SceneEditor.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ObjectPrefabConfig", menuName = "Configs/ObjectPrefabConfig", order = 0)]
    public class ObjectPrefabsConfig : ScriptableObject
    {
        [SerializeField] private GameObject[] _objectPrefabs;

        public GameObject[] GetObjectPrefabs()
        {
            return _objectPrefabs;
        }
    }
}