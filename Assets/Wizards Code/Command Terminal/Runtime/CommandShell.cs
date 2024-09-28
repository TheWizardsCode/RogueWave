using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.IO;

namespace WizardsCode.CommandTerminal
{
    public struct CommandInfo
    {
        public Action<CommandArg[]> proc;
        public int max_arg_count;
        public int min_arg_count;
        public string help;
        public string group;
        public int runtimeLevel;
    }

    public struct CommandArg
    {
        public CommandArg(String value)
        {
            String = value;
        }

        public string String { get; set; }

        public int Int {
            get {
                int int_value;

                if (int.TryParse(String, out int_value)) {
                    return int_value;
                }

                TypeError("int");
                return 0;
            }
        }

        public float Float {
            get {
                float float_value;

                if (float.TryParse(String, out float_value)) {
                    return float_value;
                }

                TypeError("float");
                return 0;
            }
        }

        public bool Bool {
            get {
                if (string.Compare(String, "TRUE", ignoreCase: true) == 0) {
                    return true;
                }

                if (string.Compare(String, "FALSE", ignoreCase: true) == 0) {
                    return false;
                }

                TypeError("bool");
                return false;
            }
        }

        public override string ToString() {
            return String;
        }

        void TypeError(string expected_type) {
            Terminal.Shell.IssueErrorMessage(
                "Incorrect type for {0}, expected <{1}>",
                String, expected_type
            );
        }
    }

    public class CommandShell
    {
        Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>();
        List<CommandArg> arguments = new List<CommandArg>(); // Cache for performance
        private int _RuntimeLevel;

        public string IssuedErrorMessage { get; private set; }

        public Dictionary<string, CommandInfo> Commands {
            get { return commands; }
        }

        /// <summary>
        /// Runtime Level of the shell clamped between -1 and 9999.
        /// -1 Means no commands can be run.
        /// Commands with a runtime level higher than the shell's runtime level will not be executed.
        /// </summary>
        public int RuntimeLevel
        {
            get
            {
                return _RuntimeLevel;
            }
            set
            {
                _RuntimeLevel = Math.Clamp(value, -1, 99999);
            }
        }

        /// <summary>
        /// Uses reflection to find all RegisterCommand attributes
        /// and adds them to the commands dictionary.
        /// </summary>
        public void RegisterCommands(string[] assemblies) {
            var rejected_commands = new Dictionary<string, CommandInfo>();
            var method_flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();
            
            foreach (string assembly in assemblies)
            {
                try
                {
                    allTypes = allTypes.Concat(Assembly.Load(assembly).GetTypes()).ToArray();
                }
                catch (Exception e)
                {
#if UNITY_EDITOR
                    Debug.LogError("Command Ternminal failed to load assembly " + assembly + " " + e.Message);
#endif
                }
            }   

            foreach (var type in allTypes)
            {
                foreach (var method in type.GetMethods(method_flags)) {
                    var attribute = Attribute.GetCustomAttribute(
                        method, typeof(RegisterCommandAttribute)) as RegisterCommandAttribute;

                    if (attribute == null) {
                        if (method.Name.StartsWith("FRONTCOMMAND", StringComparison.CurrentCultureIgnoreCase)) {
                            // Front-end Command methods don't implement RegisterCommand, use default attribute
                            attribute = new RegisterCommandAttribute();
                        } else {
                            continue;
                        }
                    }

                    var methods_params = method.GetParameters();

                    string command_name = InferFrontCommandName(method.Name);
                    Action<CommandArg[]> proc;

                    if (attribute.Name == null) {
                        // Use the method's name as the command's name
                        command_name = InferCommandName(command_name == null ? method.Name : command_name);
                    } else {
                        command_name = attribute.Name;
                    }

                   if (methods_params.Length != 1 || methods_params[0].ParameterType != typeof(CommandArg[])) {
                        // Method does not match expected Action signature,
                        // this could be a command that has a FrontCommand method to handle its arguments.
                        rejected_commands.Add(command_name.ToUpper(), CommandFromParamInfo(methods_params, attribute.Help));
                        continue;
                    }

                    // Convert MethodInfo to Action.
                    // This is essentially allows us to store a reference to the method,
                    // which makes calling the method significantly more performant than using MethodInfo.Invoke().
                    proc = (Action<CommandArg[]>)Delegate.CreateDelegate(typeof(Action<CommandArg[]>), method);
                    if (string.IsNullOrEmpty(attribute._Group))
                    {
                        string group = type.Name;

                        // strip everything from "commands" onwards, if that string exists
                        int index = group.IndexOf("Commands", StringComparison.CurrentCultureIgnoreCase);
                        if (index > 0)
                        {
                            group = group.Substring(0, index);
                        }

                        // convert from camel case to spaced words
                        group = System.Text.RegularExpressions.Regex.Replace(group, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");

                        attribute._Group = group;
                    }
                    AddCommand(command_name, attribute.Group, proc, attribute.MinArgCount, attribute.MaxArgCount, attribute.Help, attribute.RuntimeLevel);
                }
            }
            HandleRejectedCommands(rejected_commands);
        }

        /// <summary>
        /// Uses reflection to find all classed with RegisterCommand attributes and calls OnDestroy on them.
        /// </summary>
        public void DeregisterCommands(string[] assemblies)
        {
            var method_flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();

            foreach (string assembly in assemblies)
            {
                try
                {
                    allTypes = allTypes.Concat(Assembly.Load(assembly).GetTypes()).ToArray();
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to load assembly " + assembly + " " + e.Message);
                }
            }

            foreach (var type in allTypes)
            {
                foreach (var method in type.GetMethods(method_flags))
                {
                    var attribute = Attribute.GetCustomAttribute(
                        method, typeof(RegisterCommandAttribute)) as RegisterCommandAttribute;

                    if (attribute == null)
                    {
                        if (method.Name.StartsWith("FRONTCOMMAND", StringComparison.CurrentCultureIgnoreCase))
                        {
                            // Front-end Command methods don't implement RegisterCommand, use default attribute
                            attribute = new RegisterCommandAttribute();
                        }
                        else
                        {
                            continue;
                        }
                    }

                    var methods_params = method.GetParameters();

                    if (methods_params.Length != 1 || methods_params[0].ParameterType != typeof(CommandArg[]))
                    {
                        continue;
                    }

                    // By now we have confirmed this is an executable commands, so we can call OnDestroy on it.
                    continue;
                }
            }
        }

        /// <summary>
        /// Parses an input line into a command and runs that command.
        /// </summary>
        /// <returns>True if the command was executed correctly. False if the terminal issued an error.</returns>
        public bool RunCommand(string line) {
            //Debug.Log("Attempting to run command: " + line);

            string remaining = line;
            IssuedErrorMessage = null;
            arguments.Clear();

            while (remaining != "") {
                var argument = EatArgument(ref remaining);

                if (argument.String != "") {
                    arguments.Add(argument);
                }
            }

            if (arguments.Count == 0) {
                string msg = $"Didn't find a command to run in {line}.";
                Terminal.LogError(msg);
                IssueErrorMessage(msg);
                return false;
            }

            string command_name = arguments[0].String.Trim().ToUpper();
            arguments.RemoveAt(0); // Remove command name from arguments

            if (commands.TryGetValue(command_name, out CommandInfo command)) {
                if (command.runtimeLevel <= RuntimeLevel)
                {
                    return RunCommand(command_name, arguments.ToArray());
                } else
                {
                    string msg = $"{command_name} has a runtime level of {command.runtimeLevel}. Your current runtime level is {RuntimeLevel}.";
                    IssueErrorMessage(msg);

#if UNITY_EDITOR
                    Terminal.Log("You can set your runtime level with the command 'SetRuntimeLevel <level>'");
#endif
                    return false;
                }
            } else if (RunCommand($"run {line}"))
            {
                return true;
            }
            else {
                string msg = $"Command {command_name} could not be found";
                IssueErrorMessage(msg);
                return false;
            }
        }

        /// <summary>
        /// Execute the command provided.
        /// </summary>
        /// <returns>True if the command was executed correctly. False if the terminal issued an error.</returns>
        public bool RunCommand(string command_name, CommandArg[] arguments) {
            var command = commands[command_name];
            int arg_count = arguments.Length;
            string error_message = null;
            int required_arg = 0;

            if (arg_count < command.min_arg_count) {
                if (command.min_arg_count == command.max_arg_count) {
                    error_message = "exactly";
                } else {
                    error_message = "at least";
                }
                required_arg = command.min_arg_count;
            } else if (command.max_arg_count > -1 && arg_count > command.max_arg_count) {
                // Do not check max allowed number of arguments if it is -1
                if (command.min_arg_count == command.max_arg_count) {
                    error_message = "exactly";
                } else {
                    error_message = "at most";
                }
                required_arg = command.max_arg_count;
            }

            if (error_message != null) {
                string plural_fix = required_arg == 1 ? "" : "s";
                IssueErrorMessage(
                    "{0} requires {1} {2} argument{3}",
                    command_name,
                    error_message,
                    required_arg,
                    plural_fix
                );
                return false;
            }

            command.proc(arguments);
            return true;
        }

        public void AddCommand(string name, CommandInfo info) {
            name = name.ToUpper();

            if (commands.ContainsKey(name)) {
                IssueErrorMessage("Command {0} is already defined.", name);
                return;
            }

            commands.Add(name, info);
        }

        public void AddCommand(string name,
                               string group,
                               Action<CommandArg[]> proc,
                               int min_arg_count = 0,
                               int max_arg_count = -1,
                               string help = "[None provided]",
                               int runtimeLevel = 99999) {
            if (help == null)
            {
                help = "[None provided]";
            }

            var info = new CommandInfo() {
                proc = proc,
                min_arg_count = min_arg_count,
                max_arg_count = max_arg_count,
                help = help,
                group = group,
                runtimeLevel = runtimeLevel
            };

            AddCommand(name, info);
        }

        public void IssueErrorMessage(string format, params object[] message) {
#if UNITY_EDITOR
            Debug.LogError(string.Format(format, message));
#endif
            IssuedErrorMessage = string.Format(format, message);
        }

        string InferCommandName(string method_name) {
            string command_name;
            int index = method_name.IndexOf("COMMAND", StringComparison.CurrentCultureIgnoreCase);

            if (index >= 0) {
                // Method is prefixed, suffixed with, or contains "COMMAND".
                command_name = method_name.Remove(index, 7);
            } else {
                command_name = method_name;
            }

            return command_name;
        }

        string InferFrontCommandName(string method_name) {
            int index = method_name.IndexOf("FRONT", StringComparison.CurrentCultureIgnoreCase);
            return index >= 0 ? method_name.Remove(index, 5) : null;
        }

        void HandleRejectedCommands(Dictionary<string, CommandInfo> rejected_commands) {
            foreach (var command in rejected_commands) {
                if (commands.ContainsKey(command.Key)) {
                    commands[command.Key] = new CommandInfo() {
                        group = commands[command.Key].group,
                        proc = commands[command.Key].proc,
                        min_arg_count = command.Value.min_arg_count,
                        max_arg_count = command.Value.max_arg_count,
                        help = command.Value.help
                    };
                } else {
                    IssueErrorMessage("{0} is missing a front command and does not have a `CommandArg[] args` parameter.", command);
                }
            }
        }

        CommandInfo CommandFromParamInfo(ParameterInfo[] parameters, string help) {
            int optional_args = 0;

            foreach (var param in parameters) {
                if (param.IsOptional) {
                    optional_args += 1;
                }
            }

            return new CommandInfo() {
                proc = null,
                min_arg_count = parameters.Length - optional_args,
                max_arg_count = parameters.Length,
                help = help
            };
        }

        CommandArg EatArgument(ref string s) {
            var arg = new CommandArg();
            int space_index = s.IndexOf(' ');
            int start_quote_index = s.IndexOf('"');
            if (start_quote_index >= 0 && start_quote_index < space_index)
            {
                int end_quote_index = s.IndexOf('"', start_quote_index + 1);

                if (end_quote_index >= 0)
                {
                    arg.String = s.Substring(start_quote_index + 1, end_quote_index - start_quote_index - 1);
                    s = s.Substring(end_quote_index + 1); // Remaining
                }
                else
                {
                    arg.String = s.Substring(start_quote_index + 1);
                    s = "";
                }
            }
            else
            {
                if (space_index >= 0)
                {
                    arg.String = s.Substring(0, space_index);
                    s = s.Substring(space_index + 1); // Remaining
                }
                else
                {
                    arg.String = s;
                    s = "";
                }
            }

            return arg;
        }

        internal string GetHelpText()
        {
            string currentGroup = string.Empty;

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
                    sb.AppendLine($"## {currentGroup}");
                    sb.AppendLine("");
                }
                sb.AppendLine($"  * {command.Key.ToLower().PadRight(26)} : Runtime Level {command.Value.runtimeLevel} : {command.Value.help.Trim()}");
            }

            try
            {
                string path = $"{Application.persistentDataPath}/help.md";
                File.WriteAllText(path, sb.ToString());

                Terminal.Log($"Help file written to {path}");
            }
            catch (System.Exception e)
            {
                Terminal.Log($"Failed to write help file: {e.Message}");
            }

            return sb.ToString();
        }
    }
}
