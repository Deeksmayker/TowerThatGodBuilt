using System;
using System.Collections.Generic;
using Source.Features.SceneEditor.Controllers;
using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Interfaces;
using TMPro;
using UnityEngine;

namespace Source.Features.SceneEditor.UI.ObjectsPanel
{
    public class ObjectsPanel : MonoBehaviour, IChangeStateListener<EBuildingState>
    {
        private Color _highlightColor = Color.red;
        private Color _defaultColor = Color.black;
        
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private List<TMP_Text> _objectCardTexts;
        
        private void Awake()
        {
            _inputHandler.AlphaButtonPressed += OnAlphaButtonPressed;
            
            OnAlphaButtonPressed(0);
        }
        
        private void OnDestroy()
        {
            _inputHandler.AlphaButtonPressed -= OnAlphaButtonPressed;
        }
        
        public void OnStateChange(EBuildingState state)
        {
            gameObject.SetActive(state == EBuildingState.Build);
        }
        
        private void OnAlphaButtonPressed(int index)
        {
            if (index < 0 || index >= _objectCardTexts.Count) return;

            foreach (var text in _objectCardTexts)
            {
                text.color = _defaultColor;
            }
            
            _objectCardTexts[index].color = _highlightColor;
        }
    }
}