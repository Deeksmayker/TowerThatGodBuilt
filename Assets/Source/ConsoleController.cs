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
        inputField.onValueChanged.AddListener(delegate { ToLower(); });

    }

    public void ToLower()
    {
        inputField.text = inputField.text.ToLower();


        for (int i = 0; i < hints.Count; i++)
        {
            hints[i].SetActive(false);
        }
        if (inputField.text.Length > 1)
        {
            CheckForHint();
        }

    }


    [SerializeField] private List<GameObject> hints;

    public void CheckForHint()
    {
        List<string> hintsStr = new List<string>();

        for (int i = 0; i < commands.Count; i++)
        {
            if (commands[i].Contains(inputField.text))
            {
                hintsStr.Add(commands[i]);
            }

            if (hintsStr.Count == 3)
                break;
        }

        if (hintsStr.Count > 0)
        {
            for (int i = 0; i < hintsStr.Count; i++)
            {
                hints[i].SetActive(true);
                hints[i].GetComponentInChildren<TextMeshProUGUI>().text = hintsStr[i];
            }
        }
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

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (hints[0].activeInHierarchy)
            {
                inputField.text = hints[0].GetComponentInChildren<TextMeshProUGUI>().text;

                OnInputFieldEndEdit();
            }
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

        for (int i = 0; i < hints.Count; i++)
        {
            hints[i].SetActive(false);
        }

        inputField.text = string.Empty;


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
