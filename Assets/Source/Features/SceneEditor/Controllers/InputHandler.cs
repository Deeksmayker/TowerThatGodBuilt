﻿using System;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class InputHandler : MonoBehaviour
    {
        public event Action<int> AlphaButtonPressed;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Alpha1))
                AlphaButtonPressed?.Invoke(0);
            
            if (Input.GetKeyUp(KeyCode.Alpha2))
                AlphaButtonPressed?.Invoke(1);
            
            if (Input.GetKeyUp(KeyCode.Alpha3))
                AlphaButtonPressed?.Invoke(2);
            
            if (Input.GetKeyUp(KeyCode.Alpha4))
                AlphaButtonPressed?.Invoke(3);
            
            if (Input.GetKeyUp(KeyCode.Alpha5))
                AlphaButtonPressed?.Invoke(4);
            
            if (Input.GetKeyUp(KeyCode.Alpha6))
                AlphaButtonPressed?.Invoke(5);
            
            if (Input.GetKeyUp(KeyCode.Alpha7))
                AlphaButtonPressed?.Invoke(6);
            
            if (Input.GetKeyUp(KeyCode.Alpha8))
                AlphaButtonPressed?.Invoke(7);
            
            if (Input.GetKeyUp(KeyCode.Alpha9))
                AlphaButtonPressed?.Invoke(8);
            
            if (Input.GetKeyUp(KeyCode.Alpha0))
                AlphaButtonPressed?.Invoke(9);
        }
    }
}