using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ConsoleController : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private ScrollView historyScrollView;
    // prefab

    [SerializeField] private List<string> commands;


    public void OnInputFieldEndEdit()
    {
        if (inputField != null)
        {
            string fieldResult = inputField.text;

            switch (fieldResult)
            {

            }
        }
    }


    public void ShowAllCommands()
    {

    }

}
