using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Source.Features.SceneEditor.UI.SavePanel
{
    public class LoadPanelView : MonoBehaviour
    {
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _showButton;
        [SerializeField] private TMP_InputField _inputField;

        public TMP_InputField GetInputField()
        {
            return _inputField;
        }

        public Button GetLoadButton()
        {
            return _loadButton;
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
            
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}