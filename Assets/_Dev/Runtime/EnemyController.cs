using NeoFPS;
using NeoFPS.SinglePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField, Tooltip("How fast the enemy moves.")]
        private float speed = 5f;
        [SerializeField, Tooltip("How fast the enemy rotates.")]
        private float rotationSpeed = 1f;

        private void Update()
        {
            if (FpsSoloCharacter.localPlayerCharacter == null)
                return;

            Vector3 destination = FpsSoloCharacter.localPlayerCharacter.localTransform.position;
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

            Vector3 directionToTarget = destination - transform.position;
            directionToTarget.y = 0;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        public void OnAliveIsChanged(bool isAlive)
        {
            if (!isAlive)
                Destroy(gameObject);
        }

    }
}
