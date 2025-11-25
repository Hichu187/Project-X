using UnityEngine;

namespace Game
{
    public class WeaponCoroutineRunner : MonoBehaviour
    {
        private static WeaponCoroutineRunner _instance;

        public static void Run(System.Collections.IEnumerator routine)
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("[WeaponCoroutineRunner]");
                _instance = go.AddComponent<WeaponCoroutineRunner>();
                Object.DontDestroyOnLoad(go);
            }

            _instance.StartCoroutine(routine);
        }
    }
}
