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

    [SerializeField] private GameObject consoleWindow;
    [SerializeField] private List<string> commands;

    private bool _isConsoleOpened = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            Debug.Log("SwitchConsoleWindow");
            SwitchConsoleWindow();
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) /*&& _isConsoleOpened*/)
        {
            Debug.Log("Enter");
        }
    }

    public void SwitchConsoleWindow()
    {
        consoleWindow.SetActive(!_isConsoleOpened);
        _isConsoleOpened = !_isConsoleOpened;
    }

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
