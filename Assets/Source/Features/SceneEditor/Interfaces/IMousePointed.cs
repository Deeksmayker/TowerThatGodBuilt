using UnityEngine;

namespace Source.Features.SceneEditor.Interfaces
{
    public interface IMousePointed
    {
        void MouseExit();
        void MouseEnter();
        void MouseLeftButtonUp();
        void MouseLeftButton();
    }
}