using System.Text;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace WizardsCode.CommandTerminal
{
    public class TerminalCommands
    {
        private static CancellationTokenSource cts;

        private static void OnDestroy()
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        private void Start()
        {
            cts = new CancellationTokenSource();
        }

        [RegisterCommand(Help = "Clears the Command Console", MaxArgCount = 0)]
        public static void CommandClear(CommandArg[] args) {
            Terminal.Buffer.Clear();
        }

        [RegisterCommand(Name = "Help", 
            Help = "Lists all Commands or displays help documentation of a Command", 
            MaxArgCount = 1,
            RuntimeLevel = 0)]
        public static void Help(CommandArg[] args) {
            string currentGroup = string.Empty;
            
            if (args.Length == 0) {
                StringBuilder sb = new StringBuilder();

                foreach (var command in Terminal.Shell.Commands)
                {
                    if (command.Value.group != currentGroup)
                    {
                        currentGroup = command.Value.group;
                        if (string.IsNullOrEmpty(currentGroup))
                        {
                            currentGroup = "Misc";
                        }
                        sb.AppendLine("");
                        sb.AppendLine($"===== {currentGroup} =====");
                        sb.AppendLine("");
                    }
#if UNITY_EDITOR
                    sb.AppendLine($"{command.Key.ToLower().PadRight(26)} : Runtime Level {command.Value.runtimeLevel} : {command.Value.help.Trim()}");
#else
                    sb.AppendLine($"{command.Key.ToLower().PadRight(26)} : {command.Value.help.Trim()}");
#endif
                }

                Terminal.Log(sb.ToString());


#if UNITY_EDITOR
                try
                {
                    string path = $"{Application.persistentDataPath}/help.md";
                    File.WriteAllText(path, sb.ToString());

                    Terminal.Log($"Help file written to {path}");
                }
                catch    (System.Exception e)
                {
                    Terminal.Log($"Failed to write help file: {e.Message}");
                }
#endif

                return;
            }

            string command_name = args[0].String.ToUpper();

            if (!Terminal.Shell.Commands.ContainsKey(command_name)) {
                Terminal.Shell.IssueErrorMessage("Command {0} could not be found.", command_name);
                return;
            }

            string help = Terminal.Shell.Commands[command_name].help;

            if (help == null) {
                Terminal.Log("{0} does not provide any help documentation.", command_name);
            } else {
                Terminal.Log(help);
            }
        }

        [RegisterCommand(Name = "Watch", Help = "Run another command every x seconds. The first parameter is the number of seconds between runs, " +
    "The second parameter is the command to run, followed by 0..n parameters for that command.",
            RuntimeLevel = 0)]
        public static void Watch(CommandArg[] args)
        {
            if (args.Length < 2)
            {
                Terminal.Log("Usage: watch <seconds> <command> [parameters]");
                return;
            }

            Terminal.Instance.WatchCommand(args);
        }

        [RegisterCommand(Name = "Unwatch", Help = "Stop watching a command. The parameter is the command to stop watching. Leave the parameter empty to list all currently watched commands.",
            RuntimeLevel = 0)]
        public static void Unwatch(CommandArg[] args)
        {
            if (args.Length == 0)
            {
                Terminal.Log("Usage: unwatch <command>\n\n");
                if (Terminal.Instance.watchedCommands.Count > 0)
                {
                    foreach (WatchedCommand cmd in Terminal.Instance.watchedCommands)
                    {
                        Terminal.Log($"Every {cmd.seconds} run `{cmd.command} {cmd.args}`");
                    }
                } else
                {
                    Terminal.Log("No watched commands.");
                }
                return;
            }

            Terminal.Instance.UnwatchCommand(args);

            Terminal.Log($"No command {args} is being watched.");
        }

#if WIZARDS_DEBUG
        [RegisterCommand(Help = "Time the execution of a Command", MinArgCount = 1)]
        public static void CommandTime(CommandArg[] args) {
            var sw = new Stopwatch();
            sw.Start();

            Terminal.Shell.RunCommand(JoinArguments(args));

            sw.Stop();
            Terminal.Log("Time: {0}ms", (double)sw.ElapsedTicks / 10000);
        }
        
        [RegisterCommand(Help = "Outputs the StackTrace of the previous message", MaxArgCount = 0)]
        public static void Trace(CommandArg[] args) {
            int log_count = Terminal.Buffer.Logs.Count;

            if (log_count - 2 <  0) {
                Terminal.Log("Nothing to trace.");
                return;
            }

            var log_item = Terminal.Buffer.Logs[log_count - 2];

            if (log_item.stack_trace == "") {
                Terminal.Log("{0} (no trace)", log_item.message);
            } else {
                Terminal.Log(log_item.stack_trace);
            }
        }
#endif

        [RegisterCommand(Help = "Log a message to the Terminal.", MinArgCount = 1)]
        public static void Log(CommandArg[] args)
        {
            Terminal.Log(JoinArguments(args));
        }

        [RegisterCommand(Help = "Quits running Application", MaxArgCount = 0)]
        public static void Quit(CommandArg[] args) {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

#if UNITY_EDITOR
        [RegisterCommand(Help = "Set the runtime level of the terminal to a value between 0 (no commands can be run) and 9999 (all commands can be run).",
            RuntimeLevel = 0)]
        public static void SetRuntimeLevel(CommandArg[] args)
        {
            if (args.Length == 0)
            {
                Terminal.LogError("You must provide a runtime level.");
                return;
            }

            int level = args[0].Int;
            Terminal.Shell.RuntimeLevel = level;
            Terminal.Log($"Runtime level set to {Terminal.Shell.RuntimeLevel}");
        }
#endif

        static string JoinArguments(CommandArg[] args) {
            var sb = new StringBuilder();
            int arg_length = args.Length;

            for (int i = 0; i < arg_length; i++) {
                sb.Append(args[i].String);

                if (i < arg_length - 1) {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        [RegisterCommand(Help = "Execute a script file. Provide the path to the script file as the first parameter.")]
        public static async void Run(CommandArg[] args)
        {
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            if (args.Length == 0)
            {
                Terminal.LogError("You must provide a path to the script file to execute.");
                return;
            }

            string name = args[0].String;

            TextAsset textAsset = Resources.Load<TextAsset>($"Terminal Scripts/{name}");
            if (textAsset == null)
            {
                Terminal.LogError($"The file '{name}.txt' does not exist in the Resources folder.");
                return;
            }

            float _originalTimeScale = Terminal.Instance.TimeScale;
            Terminal.Instance.TimeScale = 1;
            
            if (token.IsCancellationRequested)
            {
                Terminal.Log("Cancelling run command");
                Terminal.Instance.TimeScale = _originalTimeScale;
                return;
            }

            string script = textAsset.text;
            RunScript(script);

            Terminal.Instance.TimeScale = _originalTimeScale;
        }

        public static async void RunScript(string script)
        {
            string[] lines = script.Split('\n');
            foreach (string line in lines)
            {
                if (line.StartsWith("#") || line.Trim().Length == 0)
                {
                    continue;
                }

                if (line.ToLower().StartsWith("wait"))
                {
                    float waitTime = float.Parse(line.Substring(4));
                    await Task.Delay(TimeSpan.FromSeconds(waitTime));
                    continue;
                }

                if (line.ToLower().StartsWith("open"))
                {
                    Terminal.Instance.OpenSmall();
                    continue;
                }

                if (line.ToLower().StartsWith("close"))
                {
                    Terminal.Instance.Close();
                    continue;
                }

                Terminal.LogCommand(line);
                Terminal.Shell.RunCommand(line);
            }
        }
    }
}
