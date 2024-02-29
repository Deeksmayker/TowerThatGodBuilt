using System;
using UnityEngine;

namespace Source.Features.SceneEditor.Objects
{
    public class Cell : MonoBehaviour
    {
        public event Action<Cell> Clicked;
         
        [SerializeField] private float _width;
        [SerializeField] private float _height;

        [SerializeField] private MeshRenderer _renderer;

        [SerializeField] private Material _normalMaterial;
        [SerializeField] private Material _highlightMaterial;

        private int _indexSpawnedObject = -1;

        private void OnMouseEnter()
        {
            if (_renderer.enabled)
                _renderer.material = _highlightMaterial;
        }

        private void OnMouseExit()
        {
            if (_renderer.enabled)
                _renderer.material = _normalMaterial;
        }

        private void OnMouseUpAsButton()
        {
            if (!_renderer.enabled)
                return;

            _renderer.enabled = false;
            
            Clicked?.Invoke(this);
        }

        public void Show()
        {
            _renderer.enabled = true;
        }

        public void Hide()
        {
            _renderer.enabled = false;
        }

        public Vector2 GetCellSize()
        {
            return new Vector2(_width, _height);
        }

        public void SetIndexSpawnedObject(int indexSpawnedObject)
        {
            _indexSpawnedObject = indexSpawnedObject;
        }
        
        public int GetIndexSpawnedObject()
        {
            return _indexSpawnedObject;
        }
    }
}