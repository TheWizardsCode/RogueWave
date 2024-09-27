using NaughtyAttributes;
using UnityEngine;

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
    [DefaultExecutionOrder(1000)]
    public class SceneSetupCommands : MonoBehaviour
    {
        [SerializeField, Tooltip("The commands to execute when this object awakens."), TextArea(10, 30)]
        private string onAwakeScript;
        [SerializeField, Tooltip("The commands to execute when this object is started."), TextArea(10, 30)]
        private string onStartScript;
        [SerializeField, Tooltip("A set of commands to run on demand. Click the button below to run them. Can be edited and run at runtime."), TextArea(10, 30)]
        private string onDemandScript;

        private void Awake()
        {
            ExecuteScript(onAwakeScript);
        }

        private void Start()
        {
            ExecuteScript(onStartScript);
        }

        public void ExecuteScript(string script)
        {
            if (string.IsNullOrEmpty(script))
            {
                return;
            }

            TerminalCommands.RunScript(script);
        }

        [Button]
        public void RunOnDemandScript()
        {
            ExecuteScript(onDemandScript);
        }
    }
}