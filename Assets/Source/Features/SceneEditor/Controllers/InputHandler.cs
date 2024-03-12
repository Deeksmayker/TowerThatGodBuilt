using System;
using Source.Features.SceneEditor.Enums;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class InputHandler : MonoBehaviour
    {
        public event Action<int> AlphaButtonPressed;
        public event Action<EInstrumentState> InstrumentStateButtonPressed;
        public event Action<EBuildingState> BuildingStateButtonPressed;
        public event Action SpacePressed;
        public event Action EnterPressed;
        

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
            
            if (Input.GetKeyUp(KeyCode.Space))
                SpacePressed?.Invoke();
            
            if (Input.GetKeyUp(KeyCode.Return))
                EnterPressed?.Invoke();
            
            if (Input.GetKeyDown(KeyCode.Z))
                BuildingStateButtonPressed?.Invoke(EBuildingState.Build);
            
            if (Input.GetKeyDown(KeyCode.X))
                BuildingStateButtonPressed?.Invoke(EBuildingState.Destroy);
            
            if (Input.GetKeyDown(KeyCode.R))
                InstrumentStateButtonPressed?.Invoke(EInstrumentState.Default);
            
            if (Input.GetKeyDown(KeyCode.T))
                InstrumentStateButtonPressed?.Invoke(EInstrumentState.Tassel);
        }
    }
}