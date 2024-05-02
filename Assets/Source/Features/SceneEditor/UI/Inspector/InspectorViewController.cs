using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.UI.Inspector.Components;
using UnityEngine;

namespace Source.Features.SceneEditor.UI.Inspector
{
    public class InspectorViewController
    {
        private readonly InspectorView _view;

        public InspectorViewController(InspectorView view)
        {
            _view = view;
        }

        public void Construct(Transform selectedTransform, bool isRotated)
        {
            InitializePositionView(selectedTransform);

            if (isRotated)
            {
                _view.GetRotationComponent().Show();
                InitializeRotationView(selectedTransform);
            }
            else
            {
                _view.GetRotationComponent().Hide();
            }
        }
        
        public void Show()
        {
            _view.gameObject.SetActive(true);
        }

        public void Hide()
        {
            _view.gameObject.SetActive(false);
        }

        private void InitializePositionView(Transform selectedTransform)
        {
            _view.GetPositionComponent().SetPositionData(selectedTransform);
        }
        
        private void InitializeRotationView(Transform selectedTransform)
        {
            _view.GetRotationComponent().SetRotationData(selectedTransform);
        }
    }
}