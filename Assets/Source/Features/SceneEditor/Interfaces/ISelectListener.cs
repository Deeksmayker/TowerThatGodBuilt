using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Objects;
using UnityEngine;

namespace Source.Features.SceneEditor.Interfaces
{
    public interface ISelectListener
    {
        Transform GetTransform();
        ECubeType GetType();
        void OnSelectStateChange(bool isSelected);
    }
}