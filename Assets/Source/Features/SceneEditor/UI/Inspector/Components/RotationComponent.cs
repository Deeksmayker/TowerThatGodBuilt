using System;
using TMPro;
using UnityEngine;

namespace Source.Features.SceneEditor.UI.Inspector.Components
{
    public class RotationComponent : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _xRotationField;
        [SerializeField] private TMP_InputField _yRotationField;
        [SerializeField] private TMP_InputField _zRotationField;

        private Transform _selectedTransform;
        
        private void Awake()
        {
            _xRotationField.contentType = TMP_InputField.ContentType.DecimalNumber;
            _yRotationField.contentType = TMP_InputField.ContentType.DecimalNumber;
            _zRotationField.contentType = TMP_InputField.ContentType.DecimalNumber;
            
            _xRotationField.onValueChanged.AddListener(OnXRotationChanged);
            _yRotationField.onValueChanged.AddListener(OnYRotationChanged);
            _zRotationField.onValueChanged.AddListener(OnZRotationChanged);
        }

        public void SetRotationData(Transform selectedTransform)
        {
            _selectedTransform = selectedTransform;

            var rotation = selectedTransform.rotation.eulerAngles;
            
            _xRotationField.text = rotation.x.ToString();
            _yRotationField.text = rotation.y.ToString();
            _zRotationField.text = rotation.z.ToString();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void OnXRotationChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            
            var xRotation = float.Parse(value);
            
            var rotation = _selectedTransform.rotation.eulerAngles;
            rotation.x = xRotation;
            
            _selectedTransform.SetPositionAndRotation(_selectedTransform.position, Quaternion.Euler(rotation));
        }

        private void OnYRotationChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            
            var yRotation = float.Parse(value);
            
            var rotation = _selectedTransform.rotation.eulerAngles;
            rotation.y = yRotation;
            
            _selectedTransform.SetPositionAndRotation(_selectedTransform.position, Quaternion.Euler(rotation));
        }

        private void OnZRotationChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            
            var zRotation = float.Parse(value);
            
            var rotation = _selectedTransform.rotation.eulerAngles;
            rotation.z = zRotation;
            
            _selectedTransform.SetPositionAndRotation(_selectedTransform.position, Quaternion.Euler(rotation));
        }
    }
}