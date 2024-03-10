using System;
using Source.Features.SceneEditor.Configs;
using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace Source.Features.SceneEditor.Objects
{
    public class Cell : MonoBehaviour, IBuildingStateListener
    {
        public event Action<Cell, EBuildingState> Clicked;

        [SerializeField] private CellMaterialConfig _cellMaterialConfig;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private BoxCollider _collider;

        private Material _highlightMaterial;

        private EBuildingState _buildingState;
        private int _indexSpawnedObject = -1;

        private void OnMouseEnter()
        {
            switch (_buildingState)
            {
                case EBuildingState.Build:
                    if (_indexSpawnedObject == -1)
                        _renderer.material = _cellMaterialConfig.HighlightAddingMaterial;
                    break;
                case EBuildingState.Destroy:
                    if (_indexSpawnedObject != -1)
                        _renderer.material = _cellMaterialConfig.HighlightRemovingMaterial;
                    break;
            }
        }

        private void OnMouseExit()
        {
            ResetMaterial();
        }

        private void OnMouseUpAsButton()
        {
            _collider.enabled = false;
            
            Clicked?.Invoke(this, _buildingState);
        }

        public void Show()
        {
            _renderer.enabled = true;
        }

        public void Hide()
        {
            _renderer.enabled = false;
        }

        public void SetIndexSpawnedObject(int indexSpawnedObject)
        {
            _indexSpawnedObject = indexSpawnedObject;
        }
        
        public int GetIndexSpawnedObject()
        {
            return _indexSpawnedObject;
        }

        public void ChangeState(EBuildingState buildingState)
        {
            _buildingState = buildingState;

            ResetMaterial();

            if (_buildingState == EBuildingState.Build)
            {
                if (_indexSpawnedObject != -1)
                {
                    _collider.enabled = false;
                    _renderer.material = _cellMaterialConfig.EmptyMaterial;
                }
                else
                {
                    _collider.enabled = true;
                }
            }
            else
            {
                _collider.enabled = _indexSpawnedObject != -1;
            }
        }

        public void ResetMaterial()
        {
            _renderer.material = _buildingState == EBuildingState.Build && _indexSpawnedObject == -1
                ? _cellMaterialConfig.NormalMaterial
                : _cellMaterialConfig.EmptyMaterial;
        }
    }
}