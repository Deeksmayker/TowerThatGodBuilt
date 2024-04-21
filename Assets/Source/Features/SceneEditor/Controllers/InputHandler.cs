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
        
        //@TODO: who knows - knows
        private TextMeshProUGUI _currentEditModeTextMesh;
        private TextMeshProUGUI _avaliableModesTextMesh;
        
        private void Awake(){
            _currentEditModeTextMesh = GameObject.FindWithTag("EditModeText").GetComponent<TextMeshProUGUI>();
            _avaliableModesTextMesh = GameObject.FindWithTag("AvaliableModesText").GetComponent<TextMeshProUGUI>();;
            _currentEditModeTextMesh.text = "Disbaled";
            _avaliableModesTextMesh.text = " Z - Disabled \n X - Build \n C - Destroy \n R - Default \n T - Tassel";
        }

        private void Update()
        {
            if (_isInputLocked) return;
            
            CheckAlphaButtonPressed();

            if (Input.GetKeyDown(KeyCode.Z)){
                BuildingStateButtonPressed?.Invoke(EBuildingState.Disabled);
                _currentEditModeTextMesh.text = "Disabled";
            }
            
            if (Input.GetKeyDown(KeyCode.X)){
                BuildingStateButtonPressed?.Invoke(EBuildingState.Build);
                _currentEditModeTextMesh.text = "Build";
            }

            if (Input.GetKeyDown(KeyCode.C)){
                BuildingStateButtonPressed?.Invoke(EBuildingState.Destroy);
                _currentEditModeTextMesh.text = "Destroy";
            }

            if (Input.GetKeyDown(KeyCode.R)){
                InstrumentStateButtonPressed?.Invoke(EInstrumentState.Default);
                _currentEditModeTextMesh.text = "Default";
            }

            if (Input.GetKeyDown(KeyCode.T)){
                InstrumentStateButtonPressed?.Invoke(EInstrumentState.Tassel);
                _currentEditModeTextMesh.text = "Tassel";
            }
        }

        public void LockInput()
        {
            _isInputLocked = true;
        }

        public void UnlockInput()
        {
            _isInputLocked = false;
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