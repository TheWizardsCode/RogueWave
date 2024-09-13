using UnityEngine;
using System.Text;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace WizardsCode.CommandTerminal
{
    public enum TerminalState
    {
        Close,
        OpenSmall,
        OpenFull
    }

    public class Terminal : MonoBehaviour
    {
        [Header("Behaviour")]
        [SerializeField, Tooltip("If true, the console will be open when the game starts.")]
        bool OpenOnStart = false;
        [SerializeField, Tooltip("The timescale to apply when the console is open.")]
        internal float TimeScale = 0.1f;

        [Header("Window")]
        [Range(0, 1)]
        [SerializeField]
        float MaxHeight = 0.7f;

        [SerializeField]
        [Range(0, 1)]
        float SmallTerminalRatio = 0.33f;
        
        [SerializeField] string ToggleHotkey      = "`";
        [SerializeField] string ToggleFullHotkey  = "#`";
        [SerializeField] int BufferSize           = 512;

        [Header("Input")]
        [SerializeField] Font ConsoleFont;
        [SerializeField] string InputCaret        = ">";

        [Header("Theme")]
        [Range(0, 1)]
        [SerializeField] float InputContrast;
        [SerializeField] Color BackgroundColor    = Color.black;
        [SerializeField] Color ForegroundColor    = Color.white;
        [SerializeField] Color ShellColor         = Color.white;
        [SerializeField] Color InputColor         = Color.cyan;
        [SerializeField] Color WarningColor       = Color.yellow;
        [SerializeField] Color ErrorColor         = Color.red;

        [Header("Commands")]
        [SerializeField, Tooltip("A list of additional assemblies to search for commands. The built in commands will always be provided. If left empty, only the built in terminal commands will be used.")]
        string[] CommandAssemblies;

        [Serializable]
        public class LogEvent : UnityEvent<TerminalLogType, string> { }
        [Header("Events")]
        [SerializeField, Tooltip("Whenever a Log is recorded it will also be passed to this event.")]
        public LogEvent OnLog;

        TerminalState state;
        TextEditor editor_state;
        bool input_fix;
        bool move_cursor;
        bool initial_open; // Used to focus on TextField when console opens
        float open_target;
        float real_window_size;
        string command_text;
        string cached_command_text;
        static Vector2 scrollPosition;
        GUIStyle window_style;
        GUIStyle label_style;
        GUIStyle input_style;
        private static Terminal _instance;
        private float _originalTimescale;

        internal List<WatchedCommand> watchedCommands = new List<WatchedCommand>();

        public static CommandLog Buffer { get; private set; }
        public static CommandShell Shell { get; private set; }
        public static CommandHistory History { get; private set; }
        public static CommandAutocomplete Autocomplete { get; private set; }

        public static string LastCommandOutput
        {
            get
            {
                return Buffer.LastCommandOutput;
            }
        }

        public static Terminal Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Terminal>();
                    if (_instance == null)
                    {
                        Debug.LogError("Unable to find a Terminal Object in the loaded Scenes.");
                    }
                }
                return _instance;
            }
        }

        public static bool IssuedError {
            get { return Shell.IssuedErrorMessage != null; }
        }

        public bool IsClosed {
            get { return state == TerminalState.Close; }
        }

        public static void LogError(string format, params object[] message)
        {
            LogWithScrollReset(TerminalLogType.Error, format, message);
        }

        public static void Log(string format, params object[] message) {
            Log(TerminalLogType.ShellMessage, format, message);
        }

        public static void LogCommand(string format, params object[] message)
        {
            LogWithScrollReset(TerminalLogType.Input, format, message);
        }

        /// <summary>
        /// Add a log item to the buffer and reset the scroll position to the bottom of the log.
        /// This should be used when automatically generated messages are added to the log.
        /// Failure to use this method will result in the new message not being visible to the user, unless they scroll to see them.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="message"></param>
        public static void LogWithScrollReset(string format, params object[] message)
        {
            LogWithScrollReset(TerminalLogType.ShellMessage, format, message);
        }

        public static void Log(TerminalLogType type, string format, params object[] message) {
//#if UNITY_EDITOR
//            switch (type)
//            {
//                case TerminalLogType.Error:
//                    Debug.LogError(string.Format(format, message));
//                    break;
//                case TerminalLogType.Warning:
//                    Debug.LogWarning(string.Format(format, message));
//                    break;
//                default:
//                    Debug.Log(string.Format(format, message));
//                    break;
//            }
//#endif
            Buffer.HandleLog(string.Format(format, message), type);
            Instance.OnLog.Invoke(type, string.Format(format, message));
        }

        /// <summary>
        /// Add a log item to the buffer and reset the scroll position to the bottom of the log.
        /// This should be used when automatically generated messages are added to the log.
        /// Failure to use this method will result in the new message not being visible to the user, unless they scroll to see them.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="message"></param>
        public static void LogWithScrollReset(TerminalLogType type, string format, params object[] message)
        {
            Log(type, format, message);
            ResetScrollPosition();
        }

        public static void ResetScrollPosition()
        {
            scrollPosition.y = int.MaxValue;
        }

        public void OpenSmall ()
        {
            SetState(TerminalState.OpenSmall);
        }

        public void OpenFull()
        {
            SetState(TerminalState.OpenFull);
        }

        public void Close()
        {
            SetState(TerminalState.Close);
        }

        private void SetState(TerminalState new_state) {
            if (state == new_state) return;

            input_fix = true;
            cached_command_text = command_text;
            command_text = "";

            switch (new_state) {
                case TerminalState.Close: {
                    Time.timeScale = _originalTimescale;
                    open_target = 0;
                    break;
                }
                case TerminalState.OpenSmall: {
                    _originalTimescale = Time.timeScale;
                    Time.timeScale = TimeScale;
                    open_target = Screen.height * MaxHeight * SmallTerminalRatio;
                    real_window_size = open_target;
                    ResetScrollPosition();
                    break;
                }
                case TerminalState.OpenFull:
                default: {
                    _originalTimescale = Time.timeScale;
                    Time.timeScale = TimeScale;
                    real_window_size = Screen.height * MaxHeight;
                    open_target = real_window_size;
                    break;
                }
            }

            state = new_state;
        }

        private void ToggleState(TerminalState new_state) {
            if (state == new_state) {
                SetState(TerminalState.Close);
            } else {
                SetState(new_state);
            }
        }

        void OnEnable() {
            Application.logMessageReceived += HandleUnityLog;
        }

        void OnDisable() {
            Application.logMessageReceived -= HandleUnityLog;

            Shell.DeregisterCommands(CommandAssemblies);
        }

        void Awake()
        {
            Buffer = new CommandLog(BufferSize);
            Shell = new CommandShell();
            History = new CommandHistory();
            Autocomplete = new CommandAutocomplete();

            if (ConsoleFont == null) {
                ConsoleFont = Font.CreateDynamicFontFromOSFont("Courier New", 16);
                Debug.LogWarning("Command Console Warning: Please assign a font.");
            }

            command_text = "";
            cached_command_text = command_text;
            Assert.AreNotEqual(ToggleHotkey.ToLower(), "return", "Return is not a valid ToggleHotkey");

            SetupWindow();
            SetupInput();
            SetupLabels();

            Shell.RegisterCommands(CommandAssemblies);

            if (IssuedError) {
                Log(TerminalLogType.Error, "Error: {0}", Shell.IssuedErrorMessage);
            }

            foreach (var command in Shell.Commands) {
                Autocomplete.Register(command.Key);
            }

            if (OpenOnStart)
            {
                SetState(TerminalState.OpenSmall);
            } else
            {
                SetState(TerminalState.Close);
            }

#if DEVELOPMENTBUILD || UNITY_EDITOR
            Shell.RuntimeLevel = 999999;
#endif
        }

        void Update()
        {
            for (int i = 0; i < watchedCommands.Count; i++)
            {
                if (Time.time > watchedCommands[i].nextRun)
                {
                    watchedCommands[i].nextRun = Time.time + watchedCommands[i].seconds;
                    string cmd = watchedCommands[i].command + " " + watchedCommands[i].args;
                    Terminal.LogWithScrollReset(TerminalLogType.Input, cmd);
                    Terminal.Shell.RunCommand(cmd);
                }
            }
        }

        void OnGUI() {
            if (Event.current.Equals(Event.KeyboardEvent(ToggleHotkey))) {
                SetState(TerminalState.OpenSmall);
                initial_open = true;
            } else if (Event.current.Equals(Event.KeyboardEvent(ToggleFullHotkey))) {
                SetState(TerminalState.OpenFull);
                initial_open = true;
            }

            if (IsClosed) {
                return;
            }
            
            Rect rect = new Rect(0, open_target - real_window_size, Screen.width, real_window_size);
            GUILayout.Window(88, rect, DrawConsole, "", window_style);
        }

        void SetupWindow() {
            real_window_size = Screen.height * MaxHeight / 3;
            new Rect(0, open_target - real_window_size, Screen.width, real_window_size);

            // Set background color
            Texture2D background_texture = new Texture2D(1, 1);
            background_texture.SetPixel(0, 0, BackgroundColor);
            background_texture.Apply();

            window_style = new GUIStyle();
            window_style.normal.background = background_texture;
            window_style.padding = new RectOffset(4, 4, 4, 4);
            window_style.normal.textColor = ForegroundColor;
            window_style.font = ConsoleFont;
        }

        void SetupLabels() {
            label_style = new GUIStyle();
            label_style.font = ConsoleFont;
            label_style.normal.textColor = ForegroundColor;
            label_style.wordWrap = true;
        }

        void SetupInput() {
            input_style = new GUIStyle();
            input_style.padding = new RectOffset(4, 4, 4, 4);
            input_style.font = ConsoleFont;
            input_style.fixedHeight = ConsoleFont.fontSize * 1.6f;
            input_style.normal.textColor = InputColor;

            var dark_background = new Color();
            dark_background.r = BackgroundColor.r - InputContrast;
            dark_background.g = BackgroundColor.g - InputContrast;
            dark_background.b = BackgroundColor.b - InputContrast;
            dark_background.a = 0.5f;

            Texture2D input_background_texture = new Texture2D(1, 1);
            input_background_texture.SetPixel(0, 0, dark_background);
            input_background_texture.Apply();
            input_style.normal.background = input_background_texture;
        }

        void DrawConsole(int Window2D) {
            GUILayout.BeginVertical();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUIStyle.none, GUIStyle.none);
            GUILayout.FlexibleSpace();
            DrawLogs();
            GUILayout.EndScrollView();

            if (move_cursor) {
                CursorToEnd();
                move_cursor = false;
            }

            if (Event.current.Equals(Event.KeyboardEvent("escape"))) {
                SetState(TerminalState.Close);
            } else if (Event.current.Equals(Event.KeyboardEvent("return"))) {
                EnterCommand();
            } else if (Event.current.Equals(Event.KeyboardEvent("up"))) {
                command_text = History.Previous();
                move_cursor = true;
            } else if (Event.current.Equals(Event.KeyboardEvent("down"))) {
                command_text = History.Next();
            } else if (Event.current.Equals(Event.KeyboardEvent(ToggleHotkey))) {
                ToggleState(TerminalState.OpenSmall);
            } else if (Event.current.Equals(Event.KeyboardEvent(ToggleFullHotkey))) {
                ToggleState(TerminalState.OpenFull);
            } else if (Event.current.Equals(Event.KeyboardEvent("tab"))) {
                CompleteCommand();
                move_cursor = true; // Wait till next draw call
            }

            GUILayout.BeginHorizontal();

            if (InputCaret != "") {
                GUILayout.Label(InputCaret, input_style, GUILayout.Width(ConsoleFont.fontSize));
            }

            GUI.SetNextControlName("command_text_field");
            command_text = GUILayout.TextField(command_text, input_style);

            if (input_fix && command_text.Length > 0) {
                command_text = cached_command_text; // Otherwise the TextField picks up the ToggleHotkey character event
                input_fix = false;                  // Prevents checking string Length every draw call
            }

            if (initial_open) {
                GUI.FocusControl("command_text_field");
                initial_open = false;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        void DrawLogs() {
            foreach (var log in Buffer.Logs) {
                label_style.normal.textColor = GetLogColor(log.type);
                GUILayout.Label(log.message, label_style);
            }
        }

        /// <summary>
        /// Enter a command into the console for execution.
        /// </summary>
        void EnterCommand()
        {
            Log(TerminalLogType.Input, "{0}", command_text);


            Buffer.ClearLastCommandOutput();
            Shell.RunCommand(command_text);
            History.Push(command_text);

            if (IssuedError)
            {
                Log(TerminalLogType.Error, "Error: {0}", Shell.IssuedErrorMessage);
            }

            command_text = "";
            ResetScrollPosition();
        }

        public void WatchCommand(CommandArg[] args)
        {
            float seconds = args[0].Float;
            string command = args[1].String;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length - 2; i++)
            {
                sb.Append(args[i + 2]);
                sb.Append(" ");
            }

            WatchedCommand watchedCommand = new WatchedCommand();
            watchedCommand.command = command;
            watchedCommand.args = sb.ToString().Trim();
            watchedCommand.seconds = seconds;
            watchedCommand.nextRun = Time.time;

            watchedCommands.Add(watchedCommand);
        }

        public void UnwatchCommand(CommandArg[] args)
        {
            string command = args[0].String;

            for (int i = 0; i < watchedCommands.Count; i++)
            {
                if (watchedCommands[i].command == command)
                {
                    watchedCommands.RemoveAt(i);
                    return;
                }
            }
        }

        void CompleteCommand() {
            string input = command_text;
            string[] completions = Autocomplete.Complete(ref input);

            if (completions.Length == 0)
            {
                Log($"No completions found.");
                ResetScrollPosition();
                return;
            }

            int shortestIndex = 0;
            for (int i = 1; i < completions.Length; i++)
            {
                if (completions[i].Length < completions[shortestIndex].Length)
                {
                    shortestIndex = i;
                }
            }

            if (completions.Length > 1)
            {
                // iterate through all the possible completions and find the longest common prefix
                for (int i = 0; i < completions[shortestIndex].Length; i++)
                {
                    char c = completions[shortestIndex][i];
                    for (int j = 0; j < completions.Length; j++)
                    {
                        if (completions[j][i] != c)
                        {
                            command_text = input + completions[shortestIndex].Substring(0, i);
                            Log($"\nPossible Completions: {string.Join(" / ", completions)}");
                            ResetScrollPosition();
                            return;
                        }
                    }
                }
            } else
            {
                command_text = input + completions[shortestIndex];
                Log($"\nNo more completions found");
                ResetScrollPosition();
            }
        }

        void CursorToEnd() {
            if (editor_state == null) {
                editor_state = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            }

            editor_state.MoveCursorToPosition(new Vector2(999, 999));
        }

        void HandleUnityLog(string message, string stack_trace, LogType type) {
            Buffer.HandleLog(message, stack_trace, (TerminalLogType)type);
            ResetScrollPosition();
        }

        Color GetLogColor(TerminalLogType type) {
            switch (type) {
                case TerminalLogType.Message: return ForegroundColor;
                case TerminalLogType.Warning: return WarningColor;
                case TerminalLogType.Input: return InputColor;
                case TerminalLogType.ShellMessage: return ShellColor;
                default: return ErrorColor;
            }
        }
    }
}
