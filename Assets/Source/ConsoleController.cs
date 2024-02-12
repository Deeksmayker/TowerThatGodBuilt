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

    private void Start() => consoleLine.onValueChanged.AddListener(delegate { CheckForHint(); });

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

    public void CreateNewHistoryText(string text)
    {
        historyPanel.SetActive(true);

        var newTextObject = Instantiate(historyTextPrefab, historyContent.transform);
        historyContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, historyContent.transform.childCount * 50);
        newTextObject.GetComponent<TextMeshProUGUI>().text = text;
    }

    public void CreateWrongHistoryText() => CreateNewHistoryText("No such command found");

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
            CreateNewHistoryText("--" + consoleLine.text);

            switch (consoleLine.text)
            {
                case "help":
                    Command_Help();
                    break;
                case "kill_all":
                    Command_KillAll();
                    break;
                case "reload_level":
                    Command_ReloadLevel();
                    break;
                case "heal":
                    Command_Heal();
                    break;
                case "teleport":
                    Command_Teleport();
                    break;
                case "save":
                    Command_Save();
                    break;
                case "exit":
                    Command_Exit();
                    break;
                case "stats":
                    Command_Stats();
                    break;
                default:
                    CreateWrongHistoryText();
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
        for (int i = 0; i < commands.Count; i++)
            CreateNewHistoryText(commands[i] + " - " + commandMeanings[i]);
    }

    public void Command_KillAll() => CreateNewHistoryText("All enemies are killed.");

    public void Command_ReloadLevel() => CreateNewHistoryText("The level has been reloaded.");

    public void Command_Heal() => CreateNewHistoryText("The player's health is restored.");

    public void Command_Teleport() => CreateNewHistoryText("The player is moved to the starting point.");

    public void Command_Save() => CreateNewHistoryText("The game is saved.");

    public void Command_Exit() => CreateNewHistoryText("The game is exiting.");

    public void Command_Stats() => CreateNewHistoryText("The player's stats are shown.");
}
