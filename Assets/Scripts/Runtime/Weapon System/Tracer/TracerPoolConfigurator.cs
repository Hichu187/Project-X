using UnityEngine;

namespace Game
{
    public class TracerPoolConfigurator : MonoBehaviour
    {
        [SerializeField] private TracerObject tracerPrefab;
        [SerializeField] private Transform tracerParent;
        [SerializeField] private int prewarmCount = 20;

        private void Awake()
        {
            if (!tracerParent)
                tracerParent = transform;

            if (!tracerPrefab)
            {
                Debug.LogError("[TracerPoolConfigurator] Chưa gán tracerPrefab");
                return;
            }

            TracerSystem.Configure(tracerPrefab, tracerParent, prewarmCount);
        }
    }
}
