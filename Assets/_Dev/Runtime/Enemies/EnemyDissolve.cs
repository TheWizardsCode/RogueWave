using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    public class EnemyDissolve : MonoBehaviour
    {
        [SerializeField, Tooltip("The time, in seconds, to become fully visible.")]
        private float m_DissolveTime = 1f;

        const string k_ShaderParameter_Dissolve = "_Dissolve";

        private MeshRenderer m_Renderer = null;
        private MaterialPropertyBlock m_PropertyBlock = null;

        private float m_ElepsedTime = 0f;

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
                m_PropertyBlock.SetFloat(k_ShaderParameter_Dissolve, 1);
                m_Renderer.SetPropertyBlock(m_PropertyBlock, 0);
            }
        }

        private void Update()
        {
            if (m_ElepsedTime < m_DissolveTime)
            {
                m_ElepsedTime += Time.deltaTime;
                m_PropertyBlock.SetFloat(k_ShaderParameter_Dissolve, Mathf.Lerp(1f, 0f, m_ElepsedTime / m_DissolveTime));
                m_Renderer.SetPropertyBlock(m_PropertyBlock, 0);
            }
            else
            {
                m_PropertyBlock.SetFloat(k_ShaderParameter_Dissolve, 0);
                m_Renderer.SetPropertyBlock(m_PropertyBlock, 0);
                Destroy(this);
            }
        }

    }
}