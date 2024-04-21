using System;
using UnityEngine;
using UnityEngine.UI;

namespace Source.Features.SceneEditor.UI.SavePanel
{
    public class SavePanelController
    {
        public event Action Opened;
        public event Action Closed;
        public event Action<string> SaveButtonClicked;
        
        private readonly SavePanelView _view;
        private readonly Button _openButton;

        public SavePanelController(SavePanelView view)
        {
            _view = view;
            _openButton = _view.GetShowButton();
            
            var saveButton = _view.GetSaveButton();
            var closeButton = _view.GetCloseButton();
            
            _openButton.onClick.AddListener(OnShowButtonClicked);
            saveButton.onClick.AddListener(OnSaveButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnSaveButtonClicked()
        {
            var fileName = _view.GetInputFieldText();

            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("File name is null or empty.");
                return;
            }
            
            SaveButtonClicked?.Invoke(fileName);
        }

        private void OnCloseButtonClicked()
        {
            Closed?.Invoke();
            
            _openButton.gameObject.SetActive(true);
            _view.Hide();
        }

        private void OnShowButtonClicked()
        {
            Opened?.Invoke();
            
            _openButton.gameObject.SetActive(false);
            _view.Show();
        }
    }
}