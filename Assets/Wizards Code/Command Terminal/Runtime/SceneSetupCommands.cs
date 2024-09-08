using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace WizardsCode.CommandTerminal
{
    /// <summary>
    /// Using SceneSetupCommands you can define a set of commands that will be executed either when a scene is loaded, or on demand.
    /// This can be useful for setting up a scene for testing or debugging purposes, though it might also be useful
    /// in a production environment for setting up a scene for a demo or tutorial.
    /// 
    /// Simply provide the commands, one per line, in the SceneSetupCommands component on a GameObject in the scene.
    /// It is often a good idea to have this script last in the Script Execution Order so that all other scripts
    /// are loaded before the commands are executed.
    /// </summary>
    public class SceneSetupCommands : MonoBehaviour
    {
        [SerializeField, Tooltip("Should the commands be executed when the object is loaded? If this is false then a call to ExecuteCommands() will be required.")]
        private bool executeOnStart = true;
        [SerializeField, Tooltip("The commands to execute when the scene is loaded."), TextArea(10, 30)]
        private string commands;

        private void Start()
        {
            ExecuteCommands();
        }

        public void ExecuteCommands()
        {
            string[] lines = commands.Split('\n');
            foreach (string line in lines)
            {
                Terminal.Shell.RunCommand(line);
            }
        }
    }
}