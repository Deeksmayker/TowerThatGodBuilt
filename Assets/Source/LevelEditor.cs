#if UNITY_EDITOR

using UnityEngine;

[ExecuteInEditMode]
public class LevelEditor : MonoBehaviour{
    private void OnValidate(){
        //Debug.Log("SFD");
        // if (GUILayout.Button("Press Me")){ 
        //     Debug.Log("Hello!");
        // }

    }
    // private void OnGUI(){
    //     Debug.Log("SDF");
    //      if (GUI.Button(new Rect(10, 10, 150, 100), "I am a button"))
    //     {
    //         print("You clicked the button!");
    //     }

    // }
    
    // private void Update(){
    //     //Debug.Log(Input.mousePosition);
    //     Vector3 mousePos = Input.mousePosition;
    //     Ray ray = Camera.main.ScreenPointToRay(mousePos);
    //     if (Physics.Raycast(ray, 100000f, Layers.Environment)){
    //         Debug.Log("abobababa");   
    //     } else{
    //         Debug.Log("vashu mamu");
    //     }
    
    //     if (Input.GetMouseButtonDown(0)){
    //         Debug.Log("SDF");
    //     }
    // }
}
#endif


