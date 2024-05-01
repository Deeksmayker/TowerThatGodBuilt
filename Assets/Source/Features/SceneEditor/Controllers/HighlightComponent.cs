using System;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class HighlightComponent : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _highlightMaterial;
        [SerializeField] private Material _defaultMaterial;

        public void SetHighlight()
        {
            _renderer.material = _highlightMaterial;
        }
        
        public void SetDefault()
        {
            _renderer.material = _defaultMaterial;
        }
    }
}