using NeoFPS;
using NeoFPS.ModularFirearms;
using Playground;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RogueWave.Editor
{
    public class BasicWeaponShowcase : MonoBehaviour
    {
        [SerializeField, Tooltip("The duration of the animation routine.")]
        float animationDuration = 5f;
        [SerializeField, Tooltip("The speed at which the object should rotate.")]
        float rotationSpeed = 20f;

        IEnumerator Start()
        {
            Setup();

            StartCoroutine(Animate());

            while ( animationDuration > 0)
            {
                animationDuration -= Time.deltaTime;
                yield return null;
            }

            TearDown();
        }

        IEnumerator Animate()
        {
            yield return new WaitForSeconds(Random.Range(0, animationDuration * 0.2f));

            while (animationDuration > 0)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private void TearDown()
        {
        }

        private void Setup()
        {
        }
    }
}