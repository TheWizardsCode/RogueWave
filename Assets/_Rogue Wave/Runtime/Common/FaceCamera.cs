using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave.common
{
    public class FaceCamera : MonoBehaviour
    {
        private void Update()
        {
            transform.LookAt(Camera.main.transform);
        }
    }
}
