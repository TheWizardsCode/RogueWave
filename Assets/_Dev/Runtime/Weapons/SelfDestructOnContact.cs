using NeoFPS;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// SelfDestructOnContact is a simple weapon that will kill the object it is attached to when it comes into contact with another object. This can be used to create mines, traps, etc.
    /// 
    /// Note that this script assumes that the object it is attached to has a mechanism, such as an explosion, that will cause damage when it dies.
    /// </summary>
    public class SelfDestructOnContact : MonoBehaviour
    {
        
        protected void OnTriggerEnter(Collider other)
        {
            BasicHealthManager manager = GetComponentInParent<BasicHealthManager>();
            if (manager != null)
            {
                manager.AddDamage(float.MaxValue);
            }
        }
    }
}