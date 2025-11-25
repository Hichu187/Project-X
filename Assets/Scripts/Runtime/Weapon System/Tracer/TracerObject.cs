using System;
using System.Collections;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(LineRenderer))]
    public class TracerObject : MonoBehaviour
    {
        [SerializeField] private LineRenderer line;
        [SerializeField] private float travelSpeed = 200f;
        [SerializeField] private float stayDuration = 0.05f;

        private Action _onFinished;

        private void Awake()
        {
            if (!line)
                line = GetComponent<LineRenderer>();

            line.positionCount = 2;
            line.useWorldSpace = true;
        }

        public void Play(Vector3 start, Vector3 end, Color color, float width, Action onFinished)
        {
            _onFinished = onFinished;

            line.startColor = line.endColor = color;
            line.startWidth = line.endWidth = width;

            StopAllCoroutines();
            StartCoroutine(PlayRoutine(start, end));
        }

        private IEnumerator PlayRoutine(Vector3 start, Vector3 end)
        {
            float totalDist = Vector3.Distance(start, end);
            if (totalDist <= 0.001f)
            {
                line.SetPosition(0, start);
                line.SetPosition(1, end);
                yield return new WaitForSeconds(stayDuration);
                _onFinished?.Invoke();
                yield break;
            }

            float t = 0f;
            while (t < 1f)
            {
                float dist01 = Mathf.Clamp01(t);
                Vector3 curEnd = Vector3.Lerp(start, end, dist01);

                line.SetPosition(0, start);
                line.SetPosition(1, curEnd);

                t += Time.deltaTime * (travelSpeed / totalDist);
                yield return null;
            }

            line.SetPosition(0, start);
            line.SetPosition(1, end);

            yield return new WaitForSeconds(stayDuration);

            _onFinished?.Invoke();
        }
    }
}
