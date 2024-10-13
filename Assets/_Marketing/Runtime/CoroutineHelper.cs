using UnityEngine;
using System.Collections;

namespace WizardsCode.Marketing
{
    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;

        public static CoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("CoroutineHelper");
                    _instance = obj.AddComponent<CoroutineHelper>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        public void StartHelperCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}
