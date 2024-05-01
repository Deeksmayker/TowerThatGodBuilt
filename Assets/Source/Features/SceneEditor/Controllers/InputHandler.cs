using System;
using Source.Features.SceneEditor.Enums;
using UnityEngine;
using TMPro;


namespace Source.Features.SceneEditor.Controllers
{
    public class InputHandler : MonoBehaviour
    {
        public event Action<int> AlphaButtonPressed;
        public event Action<EInstrumentState> InstrumentStateButtonPressed;
        public event Action<EBuildingState> BuildingStateButtonPressed;
        
        private bool _isInputLocked;

        private void Update()
        {
            if (_isInputLocked) return;
            
            CheckAlphaButtonPressed();
            CheckBuildingModeButtonPressed();
            CheckInstrumentButtonPressed();
        }

        public void LockInput()
        {
            _isInputLocked = true;
        }

        public void UnlockInput()
        {
            _isInputLocked = false;
        }

        private void CheckBuildingModeButtonPressed()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                BuildingStateButtonPressed?.Invoke(EBuildingState.Disabled);
            }
            
            if (Input.GetKeyDown(KeyCode.X))
            {
                BuildingStateButtonPressed?.Invoke(EBuildingState.Build);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                BuildingStateButtonPressed?.Invoke(EBuildingState.Destroy);
            }
        }

        private void CheckInstrumentButtonPressed()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                InstrumentStateButtonPressed?.Invoke(EInstrumentState.Default);
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                InstrumentStateButtonPressed?.Invoke(EInstrumentState.Tassel);
            }
        }

        private void CheckAlphaButtonPressed()
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