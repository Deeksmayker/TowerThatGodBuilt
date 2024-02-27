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

        private bool _isOccupied;
        
        private void OnMouseEnter()
        {
            if (!_isOccupied)
                _renderer.material = _highlightMaterial;
        }

        private void OnMouseExit()
        {
            if (!_isOccupied)
                _renderer.material = _normalMaterial;
        }

        private void OnMouseUp()
        {
            if (_isOccupied)
                return;

            _isOccupied = true;
            _renderer.enabled = false;
            
            Clicked?.Invoke(this);
        }

        public Vector2 GetCellSize()
        {
            return new Vector2(_width, _height);
        }
    }
}