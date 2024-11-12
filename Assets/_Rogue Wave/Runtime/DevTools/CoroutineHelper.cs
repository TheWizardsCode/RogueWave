using UnityEngine;
using System.Collections;

namespace RogueWave
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

        /// <summary>
        /// If you need to invoke a method with parameters after a delay, use this method.
        /// 
        /// Usage:  
        /// CoroutineHelper.Instance.InvokeMethodWithDelay((msg) => Debug.Log(msg), "Hello with parameter after delay", 2f);
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="parameter"></param>
        /// <param name="delaySeconds"></param>
        /// <returns></returns>
        public void InvokeMethodWithDelay<T>(System.Action<T> method, T parameter, float delaySeconds)
        {
            StartCoroutine(InvokeMethodCoroutine(method, parameter, delaySeconds));
        }

        /// <summary>
        /// If you need to invoke a method with parameters after a delay, use this method.
        /// 
        /// Usage:  
        /// CoroutineHelper.Instance.InvokeMethodWithDelay((msg) => MyMethod(msg, value), "My message", 2.3f, 2f);
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="parameter"></param>
        /// <param name="delaySeconds"></param>
        /// <returns></returns>
        public void InvokeMethodWithDelay<T1, T2>(System.Action<T1, T2> method, T1 parameter1, T2 parameter2, float delaySeconds)
        {
            StartCoroutine(InvokeMethodCoroutine(method, parameter1, parameter2, delaySeconds));
        }

        IEnumerator InvokeMethodCoroutine<T>(System.Action<T> method, T parameter, float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            method(parameter);
        }

        IEnumerator InvokeMethodCoroutine<T1, T2>(System.Action<T1, T2> method, T1 parameter1, T2 parameter2, float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            method(parameter1, parameter2);
        }
    }
}
