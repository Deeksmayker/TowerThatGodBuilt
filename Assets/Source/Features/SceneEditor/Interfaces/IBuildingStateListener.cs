using Source.Features.SceneEditor.Enums;

namespace Source.Features.SceneEditor.Interfaces
{
    public interface IBuildingStateListener
    {
        void ChangeState(EBuildingState buildingState);
    }
}