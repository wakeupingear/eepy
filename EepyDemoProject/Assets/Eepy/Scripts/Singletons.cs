using UnityEngine;

namespace Eepy
{
    [DisallowMultipleComponent]
    public class Singletons : MonoBehaviour
    {
        public static Singletons Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}