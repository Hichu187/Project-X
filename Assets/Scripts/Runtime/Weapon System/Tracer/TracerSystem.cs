using UnityEngine;

namespace Game
{
    public static class TracerSystem
    {
        public static void Configure(TracerObject prefab, Transform parent = null, int prewarm = 20)
        {
            var pool = TracerPool.instance;
            pool.Configure(prefab, parent);
            if (prewarm > 0)
                pool.Prewarm(prewarm);
        }

        public static void SpawnTracer(Vector3 start, Vector3 end, Color color, float width = 0.04f)
        {
            var pool = TracerPool.instance;
            var tracer = pool.Get();

            tracer.Play(start, end, color, width, () =>
            {
                pool.Release(tracer);
            });
        }
    }
}
