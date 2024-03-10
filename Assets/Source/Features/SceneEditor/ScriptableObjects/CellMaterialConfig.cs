using UnityEngine;

namespace Source.Features.SceneEditor.Configs
{
    [CreateAssetMenu(fileName = "CellMaterialConfig", menuName = "Configs/CellMaterialConfig", order = 0)]
    public class CellMaterialConfig : ScriptableObject
    {
        public Material NormalMaterial;
        public Material EmptyMaterial;
        public Material HighlightAddingMaterial;
        public Material HighlightRemovingMaterial;
    }
}