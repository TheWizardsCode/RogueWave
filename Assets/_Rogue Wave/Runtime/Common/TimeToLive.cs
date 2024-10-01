using NeoFPS;
using System.Collections;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// TimeToLive can be added to an object to cause it to be destroyed, or returned to the pool, 
    /// after a certain amount of time.
    /// </summary>
    public class TimeToLive : MonoBehaviour
    {
        [SerializeField, Tooltip("The time in seconds before the object is destroyed.")]
        private float m_TimeToLive = 3;

        PooledObject m_PooledObject;
        private float m_TimeToDestroy;

        private void Start()
        {
            m_PooledObject = GetComponent<PooledObject>();
        }

        private void OnEnable()
        {
            StartCoroutine(StartCountdown());
        }

        private IEnumerator StartCountdown()
        {
            yield return new WaitForSeconds(m_TimeToLive);

            if (m_PooledObject != null)
            {
                m_PooledObject.ReturnToPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
