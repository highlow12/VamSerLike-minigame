using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Text.RegularExpressions;
using System;

public class DebugConsole : MonoBehaviour
{
    public static DebugConsole Instance;
    private List<Line> lines = new();
    public List<string> commandHistory = new();
    private Command[] commands = new Command[7];
    private Image consoleImage;
    private bool isHost = true;
    private Coroutine hideLogCoroutine;
    private Coroutine showLogCoroutine;
    private Command currentCommand;
    public GameObject localPlayer;
    public char commandPrefix = '/';
    public int historyCursor = -1;
    public bool isFocused = false;
    public bool requireParse = false;
    public bool requireRender = false;
    public TMP_InputField inputField;
    public TMP_Text consoleText;
    public GameObject toolTipObject;
    public TMP_Text toolTipText;
    public GameObject debugPanel;
    public string currentCommandText;
    public LayerMask playerLayer;
    List<string> matchedOptions = new();
    int matchedOptionCursor = 0;
    bool preventClearMatchedOptions = false;


    public enum MessageType
    {
        Local,
        Shared
    }

    public enum LineType
    {
        Info,
        Warning,
        Error
    }

    public struct Line
    {
        public string text;
        public MessageType messageType;
        public long tick;

    }
    public struct Command
    {
        public string name;
        // this string will be displayed in console when command is executed successfully
        public string successMessage;
        public List<string> parameters;
        // these properties are used for tool tip
        public string description;
        public string usage;
        public List<List<string>> availableParameters;
    }

    // Commands

    private Command help = new()
    {
        name = "help",
        description = "Display all available commands",
        usage = "help",
        successMessage = "That's all the commands",
        parameters = new List<string>()
    };

    private Command changeWeapon = new()
    {
        name = "changeWeapon",
        description = "Change player weapon",
        usage = "changeWeapon [weaponType]",
        successMessage = "Changed weapon to {argument1}",
        parameters = new List<string>(),
        availableParameters = new List<List<string>>()
    };

    private Command spawn = new()
    {
        name = "spawn",
        description = "Spawn monster, dropItem, etc.",
        usage = "spawn [objectName]",
        successMessage = "Spawned {argument1}",
        parameters = new List<string>(),
        availableParameters = new List<List<string>>()
    };

    private Command addSubWeapon = new()
    {
        name = "addSubWeapon",
        description = "Add sub weapon to player",
        usage = "addSubWeapon [subWeaponType]",
        successMessage = "Added sub weapon {argument1}",
        parameters = new List<string>(),
        availableParameters = new List<List<string>>()
    };

    private Command modifySubWeaponGrade = new()
    {
        name = "modifySubWeaponGrade",
        description = "Modify sub weapon grade",
        usage = "modifySubWeaponGrade [subWeaponType] [grade]",
        successMessage = "Modified sub weapon {argument1} grade to {argument2}",
        parameters = new List<string>(),
        availableParameters = new List<List<string>>()
    };

    private Command setStage = new()
    {
        name = "setStage",
        description = "Set stage",
        usage = "setStage [stageNumber]",
        successMessage = "Set stage to {argument1}",
        parameters = new List<string>()
    };

    private Command simulateItemDrop = new()
    {
        name = "simulateItemDrop",
        description = "Simulate item drop",
        usage = "simulateItemDrop [count]",
        successMessage = "Simulated item drop {argument1} times",
        parameters = new List<string>()
    };

    void Awake()
    {
        Instance = this;
        consoleImage = GetComponent<Image>();
        consoleImage.enabled = false;
        // add commands
        commands[0] = help;
        commands[1] = changeWeapon;
        commands[2] = spawn;
        commands[3] = addSubWeapon;
        commands[4] = modifySubWeaponGrade;
        commands[5] = setStage;
        commands[6] = simulateItemDrop;
    }

    void Start()
    {
        // Initialize weapon types
        List<string> parameters = new();
        for (int i = 0; i < Enum.GetNames(typeof(Weapon.MainWeapon.WeaponType)).Length; i++)
        {
            parameters.Add(Enum.GetName(typeof(Weapon.MainWeapon.WeaponType), i));
        }
        changeWeapon.availableParameters.Add(parameters.ToList());
        parameters.Clear();
        // Initialize object names
        UnityEngine.Object[] resources = Resources.LoadAll("Prefabs/In-game");
        parameters.AddRange(
            from obj in resources
            let gameObject = obj as GameObject
            select $"{$"{gameObject?.tag}/" ?? ""}{obj.name}");
        spawn.availableParameters.Add(parameters.ToList());
        parameters.Clear();
        // Initialize sub weapon types
        for (int i = 0; i < Enum.GetNames(typeof(Weapon.SubWeapon.WeaponType)).Length; i++)
        {
            parameters.Add(Enum.GetName(typeof(Weapon.SubWeapon.WeaponType), i));
        }
        addSubWeapon.availableParameters.Add(parameters.ToList());
        modifySubWeaponGrade.availableParameters.Add(parameters.ToList());
        modifySubWeaponGrade.availableParameters.Add(new List<string> { "0", "1", "2", "3", "4", "5" });
        parameters.Clear();


    }
    void Update()
    {
        if (isFocused)
        {
            if (requireRender)
            {
                RenderLines();
            }
            if (requireParse)
            {
                currentCommand = ParseCommand();
                requireParse = false;
            }
            // Up arrow, Down arrow command history
            CommandHistory();
            // Auto complete command
            AutoComplete();
            if (currentCommandText.Split(commandPrefix).Length > 1)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    commandHistory.Add(currentCommandText);
                    bool commandResult = ExecuteCommand(currentCommand);
                    if (commandResult)
                    {
                        string successMessage = currentCommand.successMessage;
                        // replace arguments in success message
                        int idx = 1;
                        foreach (string argument in currentCommand.parameters)
                        {
                            successMessage = successMessage.Replace($"{{argument{idx}}}", argument);
                            idx++;
                        }
                        AddLine($"[{currentCommand.name}] {successMessage}", LineType.Info, MessageType.Local);
                    }
                    else
                    {
                        AddLine($"[{currentCommand.name}] Command failed", LineType.Error, MessageType.Local);
                    }
                    inputField.text = "";
                    historyCursor = commandHistory.Count;
                    RenderLines();
                }
            }
            else if (currentCommandText.Length > 0)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    commandHistory.Add(currentCommandText);
                    AddLine(currentCommandText, LineType.Info, MessageType.Local);
                    inputField.text = "";
                    RenderLines();
                }
            }
            else
            {
                toolTipObject.SetActive(false);
                toolTipText.text = "";
            }
        }
        else
        {
            inputField.DeactivateInputField();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleFocus();
        }
        if (Input.GetKeyDown((KeyCode)char.ToUpper(commandPrefix)) && !isFocused)
        {
            currentCommandText = commandPrefix.ToString();
            inputField.text = currentCommandText;
            ToggleFocus();
            inputField.caretPosition = inputField.text.Length;

        }
    }

    public void ToggleFocus()
    {
        isFocused = !isFocused;
        if (isFocused)
        {
            inputField.ActivateInputField();
            consoleImage.enabled = true;
            consoleText.enabled = true;
        }
        else
        {
            inputField.DeactivateInputField();
            consoleImage.enabled = false;
            toolTipObject.SetActive(false);
            toolTipText.text = "";
            // hide console after 3 seconds
            if (hideLogCoroutine != null)
            {
                StopCoroutine(hideLogCoroutine);
            }
            hideLogCoroutine = StartCoroutine(HideLog());
        }
    }

    // This function is called by Event Listener
    public void ChangeCommand()
    {
        // remove backslashes from input for avoid the argument exception on regex match
        string newText = inputField.text.Replace("\\", "");
        currentCommandText = newText;
        if (inputField.text.Split(commandPrefix).Length > 1 && isHost)
        {
            requireParse = true;
        }
        preventClearMatchedOptions = false;
    }

    IEnumerator ShowLog()
    {
        RenderLines();
        if (hideLogCoroutine != null)
        {
            StopCoroutine(hideLogCoroutine);
        }
        consoleText.enabled = true;
        yield return new WaitForSeconds(3);
        if (isFocused)
        {
            yield break;
        }
        consoleText.enabled = false;
    }

    IEnumerator HideLog()
    {
        // hide log after 3 seconds
        yield return new WaitForSeconds(3);
        if (isFocused)
        {
            yield break;
        }
        consoleText.enabled = false;
    }

    void AddLine(string line, LineType lineType = LineType.Info, MessageType messageType = MessageType.Local)
    {
        // add line to console
        string newLine = "";
        switch (lineType)
        {
            // white
            case LineType.Info:
                newLine = $"<b><color=#FFFFFF>{line}</color></b>";
                break;
            // yellow
            case LineType.Warning:
                newLine = $"<color=#FFFF00>{line}</color>";
                break;
            // red
            case LineType.Error:
                newLine = $"<b><color=#FF0000>{line}</color></b>";
                break;
        }
        lines.Add(new Line
        {
            text = newLine,
            messageType = messageType,
            tick = GameManager.Instance.gameTimer
        });
        requireRender = true;
    }

    public void MergeLine(Line line, LineType lineType = LineType.Info)
    {
        switch (lineType)
        {
            case LineType.Info:
                line.text = $"<b><color=#FFFFFF>{line.text}</color></b>";
                break;
            case LineType.Warning:
                line.text = $"<color=#FFFF00>{line.text}</color>";
                break;
            case LineType.Error:
                line.text = $"<b><color=#FF0000>{line.text}</color></b>";
                break;
        }
        lines.Add(line);
        if (showLogCoroutine != null)
        {
            StopCoroutine(showLogCoroutine);
        }
        showLogCoroutine = StartCoroutine(ShowLog());
        requireRender = true;
    }

    // overload function for merge line with custom color
    public void MergeLine(Line line, string color)
    {
        line.text = $"<b><color={color}>{line.text}</color></b>";
        lines.Add(line);
        if (showLogCoroutine != null)
        {
            StopCoroutine(showLogCoroutine);
        }
        showLogCoroutine = StartCoroutine(ShowLog());
        requireRender = true;
    }

    void RenderLines()
    {
        // render all lines
        consoleText.text = "";
        // sort lines by tick
        lines.Sort((a, b) =>
        {
            if (a.tick == b.tick)
            {
                return a.text.CompareTo(b.text);
            }
            return a.tick.CompareTo(b.tick);

        });
        foreach (Line line in lines)
        {
            consoleText.text += line.text + "\n";
        }
        requireRender = false;
    }

    void CommandHistory()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (historyCursor == 0)
            {
                AddLine("No more command history", LineType.Warning);
            }
            else if (historyCursor >= 0)
            {
                if (historyCursor != 0)
                {
                    historyCursor--;
                }
                inputField.text = commandHistory[historyCursor];
                inputField.caretPosition = inputField.text.Length;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (historyCursor == commandHistory.Count - 1)
            {
                AddLine("No command history", LineType.Warning);
            }
            else if (historyCursor < commandHistory.Count - 1)
            {
                historyCursor++;
                inputField.text = commandHistory[historyCursor];
                inputField.caretPosition = inputField.text.Length;
            }
        }
    }

    void AutoComplete()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // get current command text
            string[] splitText = currentCommandText.Split(' ');
            splitText[^1] = matchedOptions[matchedOptionCursor];
            string result = string.Join(" ", splitText);
            if (splitText.Length == 1)
            {
                currentCommandText = commandPrefix + result;
            }
            else
            {
                currentCommandText = result;
            }
            inputField.text = currentCommandText;
            inputField.caretPosition = inputField.text.Length;
            if (matchedOptionCursor + 1 > 0)
            {
                preventClearMatchedOptions = true;
            }
            matchedOptionCursor = (matchedOptionCursor + 1) % matchedOptions.Count;
        }
    }

    Command ParseCommand()
    {
        // split text to get command and parameters
        List<string> splitCommand = currentCommandText.Substring(1, currentCommandText.Length - 1).Split(' ').ToList();
        // get toolTip command with regex
        // get command that is not default and matches with currentCommandText
        Command toolTipCommand = commands.FirstOrDefault(c => !Equals(c, default(Command)) && Regex.Match(c.name, splitCommand[0], RegexOptions.IgnoreCase).Success);
        // get executable command with exact match
        Command command = commands.FirstOrDefault(c => c.name == splitCommand[0]);
        // get parameters
        command.parameters = splitCommand.GetRange(1, splitCommand.Count - 1);
        // tool tip logic
        string displayToolTipText = "";
        if (toolTipCommand.name != null || (splitCommand.Count > 1 && Equals(command, toolTipCommand)))
        {
            int wordsCount = splitCommand.Count;
            // display tool tip
            toolTipObject.SetActive(true);
            // if no parameters given, display command description
            if (wordsCount == 1)
            {
                matchedOptions.Clear();
                matchedOptions.Add(toolTipCommand.name);
                displayToolTipText = $"{toolTipCommand.name}: {toolTipCommand.description}\nUsage: {toolTipCommand.usage}";
            }
            // if parameters given and command has available parameters, display available parameters
            else if (toolTipCommand.availableParameters != null || toolTipCommand.availableParameters?.Count > 0)
            {
                // get matched parameters with regex
                if (!preventClearMatchedOptions)
                {
                    matchedOptionCursor = 0;
                    matchedOptions.Clear();
                    foreach (string parameter in toolTipCommand.availableParameters[Math.Max(wordsCount - 2, 0)])
                    {
                        Match match = Regex.Match(parameter, splitCommand.Last(), RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            matchedOptions.Add(parameter);
                        }
                    }
                }
                else
                {
                    preventClearMatchedOptions = false;
                }
                displayToolTipText = $"\nAvailable arguments - Cursor: {matchedOptionCursor}: {string.Join(", ", matchedOptions)}";
            }
            toolTipText.text = displayToolTipText;
        }
        else
        {
            toolTipObject.SetActive(false);
            toolTipText.text = displayToolTipText;
        }
        return command;

    }

    bool ExecuteCommand(Command command)
    {
        bool result = false;
        switch (command.name)
        {
            case "help":
                result = HelpCommand();
                break;
            case "changeWeapon":
                result = ChangeWeaponCommand(command.parameters);
                break;
            case "spawn":
                result = SpawnCommand(command.parameters);
                break;
            case "addSubWeapon":
                result = AddSubWeaponCommand(command.parameters);
                break;
            case "modifySubWeaponGrade":
                result = ModifySubWeaponGradeCommand(command.parameters);
                break;
            case "setStage":
                result = SetStage(command.parameters);
                break;
            case "simulateItemDrop":
                result = SimulateItemDrop(command.parameters);
                break;
            default:
                AddLine("Command not found", LineType.Error);
                break;
        }
        return result;
    }

    // Command functions
    bool HelpCommand()
    {
        // display all available commands
        string text = "";
        text += "Available commands:\n";
        text += "===================================\n";
        foreach (Command command in commands)
        {
            text += $"{command.name}: {command.description}\nUsage: {command.usage}\n";
        }
        text += "===================================";
        AddLine(text);
        return true;
    }

    bool ChangeWeaponCommand(List<string> parameters)
    {
        try
        {
            // parse string to enum
            Weapon.MainWeapon.WeaponType weaponType = (Weapon.MainWeapon.WeaponType)Enum.Parse(typeof(Weapon.MainWeapon.WeaponType), parameters[0]);
            PlayerAttack playerAttack = localPlayer.GetComponent<PlayerAttack>();
            // remove component
            try
            {
                Destroy(playerAttack.mainWeapon.attackObject);
                Destroy(playerAttack.mainWeapon);
            }
            catch
            {
                AddLine("No weapon to remove", LineType.Warning);
            }
            playerAttack.mainWeapon = null;
            // Find weapon class that named same as weaponType
            Type weaponClassType = Type.GetType(weaponType.ToString());
            Weapon.MainWeapon initWeapon;
            if (weaponClassType != null)
            {
                initWeapon = (Weapon.MainWeapon)localPlayer.AddComponent(weaponClassType);
            }
            else
            {
                AddLine($"Weapon type {weaponType} not found", LineType.Error);
                return false;
            }
            initWeapon.weaponRare = playerAttack.GetWeaponRare(weaponType);
            playerAttack.mainWeapon = initWeapon;
            return true;
        }
        catch
        {
            return false;
        }
    }

    bool SpawnCommand(List<string> parameters)
    {
        try
        {
            // get object name
            string objectName = parameters[0];
            // get object from resources
            GameObject obj = Resources.Load<GameObject>($"Prefabs/In-game/{objectName}");
            // get player position
            Vector3 playerPosition = localPlayer.transform.position;
            // get player rotation
            Quaternion playerRotation = localPlayer.transform.rotation;
            // instantiate object
            Instantiate(obj, playerPosition + Vector3.right, playerRotation);
            return true;
        }
        catch (Exception e)
        {
            AddLine(e.Message, LineType.Error);
            return false;
        }
    }

    bool AddSubWeaponCommand(List<string> parameters)
    {
        try
        {
            // parse string to enum
            Weapon.SubWeapon.WeaponType weaponType = (Weapon.SubWeapon.WeaponType)Enum.Parse(typeof(Weapon.SubWeapon.WeaponType), parameters[0]);
            PlayerAttack playerAttack = localPlayer.GetComponent<PlayerAttack>();
            // check if sub weapon already exists
            if (playerAttack.subWeapons.Exists(x => x.weaponType == weaponType))
            {
                AddLine($"SubWeapon {weaponType} already exists", LineType.Warning);
                return false;
            }
            playerAttack.AddSubWeapon(weaponType, 0);
            return true;
        }
        catch (Exception e)
        {
            AddLine(e.Message, LineType.Error);
            return false;
        }
    }

    bool ModifySubWeaponGradeCommand(List<string> parameters)
    {
        try
        {
            // parse string to enum
            Weapon.SubWeapon.WeaponType weaponType = (Weapon.SubWeapon.WeaponType)Enum.Parse(typeof(Weapon.SubWeapon.WeaponType), parameters[0]);
            int grade = int.Parse(parameters[1]);
            PlayerAttack playerAttack = localPlayer.GetComponent<PlayerAttack>();
            // check if sub weapon exists
            if (!playerAttack.subWeapons.Exists(x => x.weaponType == weaponType))
            {
                AddLine($"SubWeapon {weaponType} not found", LineType.Warning);
                return false;
            }
            playerAttack.ModifySubWeaponGrade(weaponType, grade);
            return true;
        }
        catch (Exception e)
        {
            AddLine(e.Message, LineType.Error);
            return false;
        }
    }

    bool SetStage(List<string> parameters)
    {
        try
        {
            int stageNumber = int.Parse(parameters[0]);
            bool result = GameManager.Instance.SetStage(stageNumber);
            return result;
        }
        catch (Exception e)
        {
            AddLine(e.Message, LineType.Error);
            return false;
        }
    }

    bool SimulateItemDrop(List<string> parameters)
    {
        try
        {
            int count = int.Parse(parameters[0]);
            for (int i = 0; i < count; i++)
            {
                string itemName = DropItemManager.Instance.DropItem(localPlayer.transform.position + Vector3.right);
                AddLine($"<color=#00FF00>{itemName}</color> drop.", LineType.Info);
            }
            return true;
        }
        catch (Exception e)
        {
            AddLine(e.Message, LineType.Error);
            return false;
        }
    }

}
