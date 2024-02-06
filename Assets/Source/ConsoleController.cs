using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConsoleController : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject historyTextPrefab;
    [SerializeField] private GameObject historyContent;
    [SerializeField] private GameObject consoleWindow;
    [SerializeField] private GameObject historyPanel;
    [SerializeField] private TMP_InputField consoleLine;

    [Header("Lists")]
    [SerializeField] private List<GameObject> hintObjects;
    [SerializeField] private List<string> commandMeanings;
    [SerializeField] private List<string> commands;

    private bool _isConsoleOpened = false;

    private void Start()
    {
        consoleLine.onValueChanged.AddListener(delegate { CheckForHint(); });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
            SwitchConsoleWindow();

        if (Input.GetKeyDown(KeyCode.Return) && _isConsoleOpened)
            SendCommand();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (hintObjects[0].activeInHierarchy)
            {
                consoleLine.text = hintObjects[0].GetComponentInChildren<TextMeshProUGUI>().text;
                SendCommand();
            }
        }
    }

    public void CreateNewHistoryText(string text, bool isWrongCommand = false)
    {
        historyPanel.SetActive(true);

        var newTextObject = Instantiate(historyTextPrefab, historyContent.transform);
        historyContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, historyContent.transform.childCount * 50);
        newTextObject.GetComponent<TextMeshProUGUI>().text = text;

        if (isWrongCommand)
        {
            var wrongTextObject = Instantiate(historyTextPrefab, historyContent.transform);
            historyContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, historyContent.transform.childCount * 50);
            wrongTextObject.GetComponent<TextMeshProUGUI>().text = "No such command found";
        }

        historyContent.transform.localPosition = new Vector3(historyContent.transform.localPosition.x, 0, 0);
        // ???
    }

    public void CheckForHint()
    {
        consoleLine.text = consoleLine.text.ToLower();
        HideHints();

        if (consoleLine.text.Length > 1)
        {
            List<string> hintsStr = new();

            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i].Contains(consoleLine.text))
                    hintsStr.Add(commands[i]);

                if (hintsStr.Count == 3)
                    break;
            }

            if (hintsStr.Count > 0)
            {
                for (int i = 0; i < hintsStr.Count; i++)
                {
                    hintObjects[i].SetActive(true);
                    hintObjects[i].GetComponentInChildren<TextMeshProUGUI>().text = hintsStr[i];
                }
            }
        }
    }

    public void SwitchConsoleWindow()
    {
        consoleWindow.SetActive(!_isConsoleOpened);
        _isConsoleOpened = !_isConsoleOpened;
    }

    public void SendCommand()
    {
        if (consoleLine != null)
        {
            switch (consoleLine.text)
            {
                case "help":
                    Command_Help();
                    break;
                default:
                    CreateNewHistoryText("--" + consoleLine.text, true);
                    break;
            }
        }

        HideHints();
        consoleLine.text = string.Empty;
    }

    public void HideHints()
    {
        for (int i = 0; i < hintObjects.Count; i++)
            hintObjects[i].SetActive(false);
    }

    public void Command_Help()
    {
        CreateNewHistoryText("--help");
        for (int i = 1; i < commands.Count; i++)
            CreateNewHistoryText(commands[i] + " - " + commandMeanings[i]);
    }
}
