using UnityEngine;

using TMPro;
using System;

namespace RogueWave.Editor
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// This is a simple FPS counter that can be used to display the current FPS on screen. As well as gather data about average/max/min FPS. This is useful for debugging performance issues.
    /// 
    /// When in a developer build or in the editor the Moving Average FPS counter will be displayed on screen. In a release build the FPS counter will not be displayed.
    /// 
    /// The game log for Development builds will contain the average, min and max FPS.
    /// 
    /// </summary>
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField, Tooltip("The size of the font.")]
        int size = 16;
        [SerializeField, Tooltip("The position of the FPS counter.")]
        Vector2 position = new Vector2(5, 5);
        [SerializeField, Tooltip("The colour of the font.")]
        Color colour = Color.green;
        [SerializeField, Tooltip("The number of frames to wait before collecting FPS data, this should be set high enough to allow the scene to start up so that the minFPS is accurate.")]
        int startUpFrames = 600;
        [SerializeField, Tooltip("The update interval for the FPS counter. A lower setting here will make the counter more accurate but will have more of an impact on performance.")]
        float updateInterval = 0.3f;

        float elapsedIntervalTime;
        int intervalFrameCount;
        private float totalFPS;
        private int totalIntervals;

        public float currentFPS { get; private set; }
        public float movingAverageFPS { get; private set; }
        public float averageFPS { get; private set; }
        public float minFPS { get; private set; }
        public float maxFPS { get; private set; }
        
        GUIStyle style = new GUIStyle();

        private void Awake()
        {
            minFPS = 500;
            maxFPS = 0;
#if UNITY_EDITOR
            style.fontSize = size;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = colour;
#endif
        }

        void Update()
        {
            if (startUpFrames > 0)
            {
                startUpFrames--;
                return;
            }

            intervalFrameCount++;
            elapsedIntervalTime += Time.unscaledDeltaTime;

            if (elapsedIntervalTime >= updateInterval)
            {
                currentFPS = intervalFrameCount / elapsedIntervalTime;
                movingAverageFPS = (movingAverageFPS + currentFPS) / 2;
                totalFPS += currentFPS;
                totalIntervals++;
                averageFPS = totalFPS / totalIntervals;

                intervalFrameCount = 0;
                elapsedIntervalTime = 0.0f;

                if (currentFPS < minFPS)
                {
                    minFPS = currentFPS;
                }
                if (currentFPS > maxFPS)
                {
                    maxFPS = currentFPS;
                }
            }
        }

#if UNITY_EDITOR
        void OnGUI()
        {
            GUI.Label(new Rect(position.x, position.y, 200, 100), $"FPS: {movingAverageFPS:.0} ({minFPS:.0} - {maxFPS:.0})", style);
        }
    }
#endif
#endif
}