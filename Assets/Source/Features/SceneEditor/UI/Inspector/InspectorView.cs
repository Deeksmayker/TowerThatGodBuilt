using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.UI.Inspector.Components;
using UnityEngine;

namespace Source.Features.SceneEditor.UI.Inspector
{
    public class InspectorView : MonoBehaviour
    {
        [SerializeField] private PositionComponent _positionComponent;
        [SerializeField] private RotationComponent _rotationComponent;

        public PositionComponent GetPositionComponent()
        {
            return _positionComponent;
        }
        
        public RotationComponent GetRotationComponent()
        {
            return _rotationComponent;
        }
    }
}