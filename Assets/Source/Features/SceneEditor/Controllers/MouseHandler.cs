using System;
using System.Collections.Generic;
using Source.Features.SceneEditor.Interfaces;
using Source.Features.SceneEditor.Objects;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class MouseHandler : MonoBehaviour
    {
        // TODO: Передавать через DI или Service Locator
        public static MouseHandler Instance;
        
        private IMousePointed _previousPointed;
        private Vector3 _hitPoint;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            CheckRayMouse();
        }
        
        public Vector3 GetHitPoint()
        {
            return _hitPoint;
        }
        
        private void CheckRayMouse()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.transform.TryGetComponent<IMousePointed>(out var pointed))
                {
                    if ((Component)_previousPointed != null && _previousPointed != pointed)
                        _previousPointed.MouseExit();

                    pointed.MouseEnter();
                    
                    _hitPoint = hit.point;
                    
                    if (Input.GetMouseButtonUp(0))
                        pointed.MouseLeftButtonUp();
                    
                    if (Input.GetMouseButtonDown(0))
                        pointed.MouseLeftButtonDown();
                    
                    _previousPointed = pointed;
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