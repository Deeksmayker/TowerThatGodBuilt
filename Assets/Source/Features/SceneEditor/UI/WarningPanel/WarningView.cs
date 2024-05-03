using TMPro;
using UnityEngine;

namespace Source.Features.SceneEditor.UI.WarningPanel
{
    public class WarningView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textComponent;

        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void SetText(string text)
        {
            _textComponent.text = text;
        }
        
        public void ClearText()
        {
            _textComponent.text = string.Empty;
        }
        
        public void SetColor(Color color)
        {
            _textComponent.color = color;
        }
    }
}