using NeoFPS;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    public class CustomRogueLiteData : MonoBehaviour
    {
        [SerializeField, Tooltip("")]
        private RogueLitePersistentData m_CustomData = new RogueLitePersistentData();

        void Awake()
        {
            if (RogueLiteManager.persistentData == null)
            {
                Debug.Log("Assigning custom rogue-lite player data");
                RogueLiteManager.AssignPersistentData(m_CustomData);
            }
        }
    }
}