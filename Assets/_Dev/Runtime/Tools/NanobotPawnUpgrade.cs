using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    public abstract class NanobotPawnUpgrade : MonoBehaviour
    {
        protected NanobotPawnController m_NanobotPawn;

        protected virtual NanobotPawnController nanobotPawn
        {
            get
            {
                if (m_NanobotPawn == null)
                {
                    m_NanobotPawn = FindObjectOfType<NanobotPawnController>();
                }
                return m_NanobotPawn;
            }
        }

        protected virtual void Start()
        {
            transform.SetParent(nanobotPawn.transform);
        }
    }
}