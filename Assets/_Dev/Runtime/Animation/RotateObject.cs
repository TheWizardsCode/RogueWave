using UnityEngine;

namespace Playground
{
    public class RotateObject : MonoBehaviour
    {
        public Vector3 rotationSpeed = new Vector3(0, 100, 0);

        void Update()
        {
            transform.Rotate(rotationSpeed.x * Time.deltaTime, rotationSpeed.y * Time.deltaTime, rotationSpeed.z * Time.deltaTime);
        }
    }
}