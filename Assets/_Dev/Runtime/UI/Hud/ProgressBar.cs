using UnityEngine;
using UnityEngine.UI;

namespace RogueWave.UI
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField, Tooltip("The minimum value of the bar.")]
        float minValue = 0;
        [SerializeField, Tooltip("The maximum value of the bar.")]
        float maxValue = 100;
        [SerializeField, Tooltip("The mask that will be used to increase/decrease the progress meter.")]
        Image mask;

        float value;
        float fullValue;

        public float MinValue
        {
            get => minValue;
            set
            {
                minValue = value;
                Value = value;
            }
        }

        public float MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = value;
                Value = value;
            }
        }

        public float Value
        {
            get => value;
            set
            {
                if (value < minValue)
                    value = minValue;
                else if (value > maxValue)
                    value = maxValue;

                this.value = value;
                mask.fillAmount = (value - minValue) / (maxValue - minValue);
            }
        }
    }
}