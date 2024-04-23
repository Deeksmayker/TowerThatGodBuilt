using TMPro;
using UnityEngine;

namespace Source.Features.SceneEditor.UI.ModePanel
{
    public class TextView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        
        public void SetText(string text)
        {
            _text.text = text;
        }
    }
}