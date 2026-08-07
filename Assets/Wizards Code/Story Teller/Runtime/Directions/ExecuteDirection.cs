using UnityEngine;

namespace WizardsCode.StoryTeller
{
    /// <summary>
    /// Executes a method on a component. Reflection is used to find the method and execute it.
    /// </summary>
    /// <param name="args">OBJECT_NAME.COMPONENT_TYPE.METHOD_NAME</param>
    public class ExecuteDirection : AbstractDirection
    {
        public override string DirectionName { get { return "Execute Method"; } }

        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 1))
            {
                return;
            }

            string methodSignature = parameters[0].Trim();
            
            string[] parts = methodSignature.Split('.');
            if (parts.Length != 3)
            {
                LogError("Method signature should be in the format OBJECT_NAME.COMPONENT_TYPE.METHOD_NAME", parameters);
                return;
            }

            string objectName = parts[0];
            string componentName = parts[1];
            string methodName = parts[2];

            Transform obj = StoryManager.Instance.FindTarget(objectName);
            if (!obj)
            {
                LogError($"{StoryManager.GetCurrentKnotName()} contains a direction to execute method with the arguments {string.Join(", ", parameters)} but no object with the name {componentName} can be found", parameters);
                return;
            }
            else
            {
                Component component = obj.GetComponent(componentName);
                if (!component)
                {
                    LogError($"{StoryManager.GetCurrentKnotName()} contains a direction to execute method with the arguments {string.Join(", ", parameters)} but no component with the name {componentName} can be found on the object", parameters);
                    return;
                }
                else
                {
                    System.Type type = component.GetType();
                    System.Reflection.MethodInfo method = type.GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

                    if (method == null)
                    {
                        LogError($"{StoryManager.GetCurrentKnotName()} contains a direction to execute method with the arguments {string.Join(", ", parameters)} but no method with the name {methodName} can be found on the component", parameters);
                        return;
                    }
                    else
                    {
                        method.Invoke(component, null);
                    }
                }
            }
        }
    }
}
