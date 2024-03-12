using System;
using Source.Features.SceneEditor.Interfaces;
using Source.Features.SceneEditor.Objects;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class MouseHandler : MonoBehaviour
    {
        private IMousePointed _previousPointed;

        private void Update()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.transform.TryGetComponent<IMousePointed>(out var pointed))
                {
                    if ((Component)_previousPointed != null && _previousPointed != pointed)
                        _previousPointed.MouseExit();

                    _previousPointed = pointed;

                    pointed.MouseEnter();
                    
                    if (Input.GetMouseButtonUp(0))
                        _previousPointed.MouseLeftButtonUp();
                    
                    if (Input.GetMouseButton(0))
                        _previousPointed.MouseLeftButton();
                }
            }
            else
            {
                if (_previousPointed != null)
                {
                    _previousPointed.MouseExit();
                    _previousPointed = null;
                }
            }
        }
    }
}