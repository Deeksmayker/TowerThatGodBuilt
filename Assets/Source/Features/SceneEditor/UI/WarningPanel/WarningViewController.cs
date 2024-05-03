

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Source.Features.SceneEditor.UI.WarningPanel
{
    public class WarningViewController
    {
        private readonly WarningView _view;
        private readonly Color _errorColor = Color.red;

        public WarningViewController(WarningView view)
        {
            _view = view;
        }

        public async void ShowErrorTextWithTime(string message, float timeInSecond)
        {
            _view.Show();
            
            _view.SetText(message);
            _view.SetColor(_errorColor);
            
            await Task.Delay(TimeSpan.FromSeconds(timeInSecond));

            _view.Hide();
        }
    }
}