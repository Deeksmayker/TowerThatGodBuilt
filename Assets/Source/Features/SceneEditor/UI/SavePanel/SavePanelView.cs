using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.EventSystem;

namespace Source.Features.SceneEditor.UI.SavePanel
{
    public class SavePanelView : MonoBehaviour
    {
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _showButton;
        [SerializeField] private TMP_InputField _inputField;

        public string GetInputFieldText()
        {
            return _inputField.text;
        }

        public Button GetSaveButton()
        {
            return _saveButton;
        }
        
        public Button GetCloseButton()
        {
            return _closeButton;
        }
        
        public Button GetShowButton()
        {
            return _showButton;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            EventSystem.current .SetSelectedGameObject(_inputField.gameObject, null);
            _inputField.OnPointerClick(new PointerEventData(EventSystem.current));
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}