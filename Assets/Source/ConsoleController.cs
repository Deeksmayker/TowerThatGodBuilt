using System.Collections;
using System.Collections.Generic;
using System.Text;
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
    [SerializeField] private List<string> commandMeanings;

    private bool _isConsoleOpened = false;

    private void Start()
    {
        inputField.onValueChanged.AddListener( delegate { ToLower(); });

    }

    public void ToLower()
    {
        inputField.text =  inputField.text.ToLower();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            SwitchConsoleWindow();
        }

        if (Input.GetKeyDown(KeyCode.Return) && _isConsoleOpened)
        {
            Debug.Log("Enter");
            OnInputFieldEndEdit();
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
                case "help":
                    Command_Help();
                    break;
            }
        }
    }


    [SerializeField] private TextMeshProUGUI helpText;

    public void Command_Help()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < commands.Count; i++)
        {
            sb.AppendLine(commands[i] + " - " + commandMeanings[i]);
        }

        helpText.text = sb.ToString();
    }
}
