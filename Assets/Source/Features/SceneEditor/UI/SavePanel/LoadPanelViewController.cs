using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Source.Features.SceneEditor.UI.SavePanel
{
    public class LoadPanelViewController
    {
        public event Action Opened;
        public event Action Closed;
        public event Action<string> LoadButtonClicked;
        
        private readonly LoadPanelView _view;
        private readonly Button _openButton;
        
        private Color _defaultColor;

        public LoadPanelViewController(LoadPanelView view)
        {
            _view = view;
            _openButton = _view.GetShowButton();
            
            var loadButton = _view.GetLoadButton();
            var closeButton = _view.GetCloseButton();
            
            _openButton.onClick.AddListener(OnShowButtonClicked);
            loadButton.onClick.AddListener(OnLoadButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            
            _defaultColor = _view.GetInputField().textComponent.color;
        }

        private void OnLoadButtonClicked()
        {
            var fileName = _view.GetInputField().text;

            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("File name is null or empty.");
                return;
            }

            if (fileName.Contains(" "))
            {
                _view.GetInputField().textComponent.color = Color.red;
                _view.GetInputField().onValueChanged.AddListener(OnInputValueChanged);

                Debug.LogError("Invalid file name.");
                return;
            }
            
            OnCloseButtonClicked();
            LoadButtonClicked?.Invoke(fileName);
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

            EventSystem.current.SetSelectedGameObject(_view.GetInputField().gameObject, null);
            _view.GetInputField().OnPointerClick(new PointerEventData(EventSystem.current));
        }
        
        private void OnInputValueChanged(string _)
        {
            _view.GetInputField().textComponent.color = _defaultColor;
            _view.GetInputField().onValueChanged.RemoveListener(OnInputValueChanged);
        }
    }
}