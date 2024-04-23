using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Interfaces;

namespace Source.Features.SceneEditor.UI.ModePanel
{
    public class InstrumentModeViewController : IChangeStateListener<EInstrumentState>
    {
        private const string CURRENT_INSTRUMENT_TEXT_FORMAT = "Current Instrument: {0}";
        
        private readonly TextView _textView;

        public InstrumentModeViewController(TextView textView)
        {
            _textView = textView;
            
            _textView.SetText(string.Format(CURRENT_INSTRUMENT_TEXT_FORMAT, EInstrumentState.Default));
        }
        
        public void OnStateChange(EInstrumentState state)
        {
            _textView.SetText(string.Format(CURRENT_INSTRUMENT_TEXT_FORMAT, state.ToString()));
        }
    }
}