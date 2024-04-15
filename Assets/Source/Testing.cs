using UnityEngine;
using UnityEngine.SceneManagement;

public class Testing : MonoBehaviour{
    [SerializeField] private GameObject panel1;
    
    private void Update(){
        if (Input.GetKeyDown(KeyCode.J)){
            panel1.SetActive(!panel1.activeSelf);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1)){
            SceneManager.LoadScene(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)){
            SceneManager.LoadScene(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)){
            SceneManager.LoadScene(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)){
            SceneManager.LoadScene(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5)){
            SceneManager.LoadScene(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6)){
            SceneManager.LoadScene(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7)){
            SceneManager.LoadScene(6);
        }
    }
}
