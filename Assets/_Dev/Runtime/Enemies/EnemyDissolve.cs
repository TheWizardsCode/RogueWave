using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    public class EnemyDissolve : MonoBehaviour
    {
        [SerializeField, Tooltip("")]
        private float m_DissolveTime = 0.5f;

        const string k_ShaderParameter_Dissolve = "_Dissolve";

        private MeshRenderer m_Renderer = null;
        private MaterialPropertyBlock m_PropertyBlock = null;

        private float m_Dissolve = 1f;

        private void OnEnable()
        {
            if (m_Renderer == null)
            {
                m_Renderer = GetComponent<MeshRenderer>();
                m_PropertyBlock = new MaterialPropertyBlock();
                m_Renderer.GetPropertyBlock(m_PropertyBlock, 0);
            }

            if (m_Renderer != null)
            {
                m_Dissolve = 1f;
                m_PropertyBlock.SetFloat(k_ShaderParameter_Dissolve, m_Dissolve);
                m_Renderer.SetPropertyBlock(m_PropertyBlock, 0);
            }
        }

        private void Update()
        {
            if (m_Dissolve != 0f)
            {
                m_Dissolve -= Time.deltaTime;
                if (m_Dissolve < 0f)
                    m_Dissolve = 0f;

                m_PropertyBlock.SetFloat(k_ShaderParameter_Dissolve, m_Dissolve);
                m_Renderer.SetPropertyBlock(m_PropertyBlock, 0);
            }
        }
    }
}