using System;

namespace Source.Features.SceneEditor.UI.QuickPlay
{
    public class QuickPlayViewController
    {
        public event Action ButtonClicked;

        public QuickPlayViewController(QuickPlayView view)
        {
            view.GetButton().onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            ButtonClicked?.Invoke();
        }
    }
}