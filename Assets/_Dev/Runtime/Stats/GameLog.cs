using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// GameLog alloes us to record game activities. These log items can be echoed to the console when running in the editor.
    /// They will always be recorded in an internal log which can be sent to a server for analysis.
    /// </summary>
    public sealed class GameLog
    {
        enum LogType
        {
            Info,
            Warning,
            Error
        }
        static List<(DateTime Timestamp, LogType logType, string Value)> m_log = new List<(DateTime, LogType, string)>();

        /// <summary>
        /// Log an info message to the game log. The message should be one more more `key:value` pairs separated by commas.
        /// </summary>
        /// <param name="message">One or more `key:value` pairs, each pair separated by a comma. For example `key1:value1, key2:value2, key3:value3`</param>
        public static void Info(string message)
        {
            m_log.Add((DateTime.Now, LogType.Info, message));

#if UNITY_EDITOR
            Debug.Log(message);
#endif
        }

        public static void Log(string message)
        {
            Info(message);
        }

        /// <summary>
        /// Log a warning message to the game log. The message should be one more more `key:value` pairs separated by commas.
        /// </summary>
        /// <param name="message">One or more `key:value` pairs, each pair separated by a comma. For example `key1:value1, key2:value2, key3:value3`</param>
        public static void LogWarning(string message)
        {
            m_log.Add((DateTime.Now, LogType.Warning, message));

#if UNITY_EDITOR
            Debug.LogWarning(message);
#endif
        }

        /// <summary>
        /// Log an error message to the game log. The message should be one more more `key:value` pairs separated by commas.
        /// </summary>
        /// <param name="message">One or more `key:value` pairs, each pair separated by a comma. For example `key1:value1, key2:value2, key3:value3`</param>
        public static void LogError(string message)
        {
            m_log.Add((DateTime.Now, LogType.Error, message));

#if UNITY_EDITOR
            Debug.LogError(message);
#endif
        }

        public static void ClearLog()
        {
            m_log.Clear();
        }

        public static string ToYAML()
        {
            StringBuilder info = new StringBuilder();
            StringBuilder error = new StringBuilder();
            StringBuilder warning = new StringBuilder();

            foreach (var log in m_log)
            {
                bool hasHeading = false;
                string currentKey = string.Empty;
                var values = log.Value.Split(',');
                foreach (string rawValue in values)
                {
                    string value = rawValue.Trim();
                    switch (log.logType)
                    {
                        case LogType.Error:
                            error.AppendLine($"  - timestamp: {log.Timestamp}");
                            error.AppendLine($"    - {value}");
                            continue;
                        case LogType.Warning:
                            warning.AppendLine($"  - timestamp: {log.Timestamp}");
                            warning.AppendLine($"    - {value}");
                            continue;
                        case LogType.Info:
                            if (!hasHeading)
                            {
                                info.AppendLine($"  - timestamp: {log.Timestamp}");
                                hasHeading = true;
                                info.Append($"    - {value}");
                                if (values.Length > 1)
                                {
                                    info.AppendLine($": ");
                                }
                                else
                                {
                                    info.AppendLine();
                                }
                            }
                            else if (!string.IsNullOrEmpty(value))
                            {
                                info.AppendLine($"      - {value}");
                            }
                            break;
                    }
                }
            }

            StringBuilder yaml = new StringBuilder();
            if (error.Length > 0)
            {
                yaml.AppendLine("LogErrors:");
                yaml.Append(error.ToString());
            }

            if (warning.Length > 0)
            {
                yaml.AppendLine("LogWarnings:");
                yaml.Append(warning.ToString());
            }

            yaml.AppendLine("LogInfo:");
            yaml.Append(info.ToString());

            return yaml.ToString();
        }
    }
}