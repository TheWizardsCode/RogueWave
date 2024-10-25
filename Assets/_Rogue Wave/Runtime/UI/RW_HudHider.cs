using NaughtyAttributes;
using NeoFPS;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class RW_HudHider : MonoBehaviour
    {
        [SerializeField, Tooltip("The Nanobot Management Panel."), Required]
        private GameObject m_NanobotManagement = null;
        [SerializeField, Tooltip("The Level Statust Panel."), Required]
        private GameObject m_LevelStatus = null;
        [SerializeField, Tooltip("Resources and Enemies Panel."), Required]
        private GameObject m_ResourcesAndEnemies = null;

        private static RW_HudHider s_Instance = null;

        protected void Awake()
        {
            s_Instance = this;
        }

        protected void OnDestroy()
        {
            if (s_Instance == this)
                s_Instance = null;
        }

        public static void HideHUD()
        {
            if (s_Instance != null)
            {
                s_Instance.m_NanobotManagement.SetActive(false);
                s_Instance.m_LevelStatus.SetActive(false);
                s_Instance.m_ResourcesAndEnemies.SetActive(false);

                HudHider.HideHUD();
            }
        }

        public static void ShowHUD()
        {
            if (s_Instance != null)
            {
                s_Instance.m_NanobotManagement.SetActive(true);
                s_Instance.m_LevelStatus.SetActive(true);
                s_Instance.m_ResourcesAndEnemies.SetActive(true);

                HudHider.ShowHUD();
            }
        }
    }
}
