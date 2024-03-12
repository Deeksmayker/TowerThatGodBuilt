using System;
using Source.Features.SceneEditor.Controllers;
using Source.Features.SceneEditor.Objects;
using Source.Features.SceneEditor.ScriptableObjects;
using Source.Features.SceneEditor.Utils;
using UnityEngine;

namespace Source
{
    public class EnvironmentController : MonoBehaviour
    {
        [SerializeField] private GridController _gridController;
        [SerializeField] private ObjectPrefabsConfig _objectPrefabsConfig;
        
        private void Start()
        {
            _gridController.BuildGrid();
            _gridController.transform.position = new Vector3(
                -_gridController.GetWidth() / 2f * _gridController.GetCellSize(),
                0,
                -_gridController.GetHeight() / 2f * _gridController.GetCellSize());
            
            SceneLoader.SetObjectPrefabsConfig(_objectPrefabsConfig);
            SceneLoader.BuildLevel(_gridController);
        }
    }
}