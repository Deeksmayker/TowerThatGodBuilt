using UnityEngine;
using UnityEngine.UI;

namespace Source.Features.SceneEditor.UI.QuickPlay
{
    public class QuickPlayView : MonoBehaviour
    {
        [SerializeField] private Button _button;

        public Button GetButton()
        {
            return _button;
        }
    }
}