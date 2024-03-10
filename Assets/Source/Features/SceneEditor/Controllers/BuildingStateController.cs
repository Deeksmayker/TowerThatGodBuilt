using System;
using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Interfaces;

namespace Source.Features.SceneEditor.Controllers
{
    public class BuildingStateController : IDisposable
    {
        private readonly InputHandler _inputHandler;
        private readonly IBuildingStateListener[] _stateListeners;

        public BuildingStateController(InputHandler inputHandler, IBuildingStateListener[] stateListeners)
        {
            inputHandler.StateButtonPressed += ChangeState;

            _inputHandler = inputHandler;
            _stateListeners = stateListeners;
        }
        
        public void Dispose()
        {
            _inputHandler.StateButtonPressed -= ChangeState;
            
            GC.SuppressFinalize(this);
        }

        public void ChangeState(int buildingState)
        {
            foreach (var stateListener in _stateListeners)
            {
                stateListener.ChangeState((EBuildingState) buildingState);
            }
        }
    }
}