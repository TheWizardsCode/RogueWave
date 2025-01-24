
using UnityEngine;

namespace WizardsCode.StoryTeller
{
    /// <summary>
    /// Wait for a specified duration of time to pass. This is measured in game seconds, that
    /// is it is affected by the `Time.timeScale` value`.
    /// </summary>
    public class WaitForDurationDirection : AbstractWaitForDirection
    {
        public override string DirectionName { get { return "Wait For Duration"; } }

        public float endTime = float.NegativeInfinity;

        public override bool IsWaiting
        {
            get
            {
                return Time.timeSinceLevelLoad < endTime;
            }
        }

        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 1, 1))
            {
                return;
            }

            if (!float.TryParse(parameters[0], out float time))
            {
                LogError(DirectionName + " direction requires a single argument that is a float representing the duration to wait for.", parameters);
            }

            endTime = Time.timeSinceLevelLoad + time;
            StoryManager.Instance.AddWaitForState(this);
        }

        public void Execute(float duration)
        {
            endTime = Time.timeSinceLevelLoad + duration;
        }
    }
}
