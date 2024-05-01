using System;
using TMPro;
using UnityEngine;

namespace Source.Features.SceneEditor.UI.Inspector.Components
{
    public class PositionComponent : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _xPositionField;
        [SerializeField] private TMP_InputField _yPositionField;
        [SerializeField] private TMP_InputField _zPositionField;

        private Transform _selectedTransform;
        
        private void Awake()
        {
            _xPositionField.contentType = TMP_InputField.ContentType.DecimalNumber;
            _yPositionField.contentType = TMP_InputField.ContentType.DecimalNumber;
            _zPositionField.contentType = TMP_InputField.ContentType.DecimalNumber;
            
            _xPositionField.onValueChanged.AddListener(OnXPositionChanged);
            _yPositionField.onValueChanged.AddListener(OnYPositionChanged);
            _zPositionField.onValueChanged.AddListener(OnZPositionChanged);
        }

        public void SetPositionData(Transform selectedTransform)
        {
            _selectedTransform = selectedTransform;

            var position = selectedTransform.position;
            
            _xPositionField.text = position.x.ToString();
            _yPositionField.text = position.y.ToString();
            _zPositionField.text = position.z.ToString();
        }

        private void OnXPositionChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            
            var xPosition = float.Parse(value);
            
            var position = _selectedTransform.position;
            position.x = xPosition;
            
            _selectedTransform.SetPositionAndRotation(position, _selectedTransform.rotation);
        }

        private void OnYPositionChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            
            var yPosition = float.Parse(value);
            
            var position = _selectedTransform.position;
            position.y = yPosition;
            
            _selectedTransform.SetPositionAndRotation(position, _selectedTransform.rotation);
        }

        private void OnZPositionChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            
            var zPosition = float.Parse(value);
            
            var position = _selectedTransform.position;
            position.z = zPosition;
            
            _selectedTransform.SetPositionAndRotation(position, _selectedTransform.rotation);
        }
    }
}