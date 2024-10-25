using UnityEngine;
using UnityEngine.UI;

namespace RogueWave.UI
{
    public class LevelProgressBar : ProgressBar
    {
        [SerializeField, Tooltip("The icon that will be denote a new wave in the progress bar.")]
        Sprite defaultWaveStartIcon;
        [SerializeField, Tooltip("A prototoype for the wave icon when rendered on the progress bar.")]
        Image waveIconPrototype;

        RectTransform m_progressBarContainer;
        private WfcDefinition _levelDefinition;

        RectTransform progressBarContainer
        {
            get
            {
                if (m_progressBarContainer == null)
                {
                    m_progressBarContainer = GetComponent<RectTransform>();
                }
                return m_progressBarContainer;
            }
        }

        internal WfcDefinition levelDefinition
        {
            get { return _levelDefinition; }
            set
            {
                _levelDefinition = value;
                if (_levelDefinition != null)
                {
                    float width = progressBarContainer.rect.width;

                    float timeOfWave = 0;
                    foreach (WaveDefinition wave in _levelDefinition.waves)
                    {
                        if (timeOfWave == 0)
                        {
                            timeOfWave = wave.SpawnEventFrequency;
                        }

                        float xPos = (timeOfWave / MaxValue) * width - (width / 2);
                        // place the icon at the start of the wave
                        Image waveStartIconImage = Instantiate<Image>(waveIconPrototype, Vector3.zero, Quaternion.identity, m_progressBarContainer);
                        waveStartIconImage.rectTransform.anchoredPosition = new Vector2(xPos, 0);
                        waveStartIconImage.sprite = defaultWaveStartIcon;
                        waveStartIconImage.gameObject.SetActive(true);

                        timeOfWave += wave.Duration;
                    }
                }
            }
        }


    }
}
