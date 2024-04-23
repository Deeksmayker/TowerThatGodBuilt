using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Interfaces;

namespace Source.Features.SceneEditor.UI.ModePanel
{
    public class BuildingModeViewController : IChangeStateListener<EBuildingState>
    {
        private const string CURRENT_MODE_TEXT_FORMAT = "Current Mode: {0}";
        
        private readonly TextView _textView;

        public BuildingModeViewController(TextView textView)
        {
            _textView = textView;
            _textView.SetText(string.Format(CURRENT_MODE_TEXT_FORMAT, EBuildingState.Disabled));
        }
        
        public void OnStateChange(EBuildingState state)
        {
            _textView.SetText(string.Format(CURRENT_MODE_TEXT_FORMAT, state.ToString()));
        }
    }
}