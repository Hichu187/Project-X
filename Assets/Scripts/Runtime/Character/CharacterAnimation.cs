using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(CharacterControl))]
    public class CharacterAnimation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterControl character;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform visualRoot;

        [Header("Animator Parameters")]
        [SerializeField] private string moveXParam = "MoveX";
        [SerializeField] private string moveYParam = "MoveY";
        [SerializeField] private string isMovingParam = "IsMoving";
        [SerializeField] private float dampTime = 0.1f;
        [SerializeField] private float moveThreshold = 0.01f; // độ nhạy để coi là đang di chuyển

        private void Reset()
        {
            if (!character) character = GetComponent<CharacterControl>();
            if (!animator) animator = GetComponentInChildren<Animator>();
            if (!visualRoot) visualRoot = character.MeshRoot ? character.MeshRoot : transform;
        }

        private void Update()
        {
            if (character == null || animator == null) return;

            // Lấy vector input từ CharacterControl
            Vector3 input = character.GetInputMoveVector();

            // Lấy hướng (local space)
            Vector3 local = visualRoot.InverseTransformDirection(input);

            // Tách thành 2D
            Vector2 move = new Vector2(local.x, local.z);

            bool isMoving = move.sqrMagnitude > (moveThreshold * moveThreshold);
            animator.SetBool(isMovingParam, isMoving);

            if (!isMoving)
            {
                // Reset ngay lập tức – không delay
                animator.SetFloat(moveXParam, 0f);
                animator.SetFloat(moveYParam, 0f);
                return;
            }

            // Chuẩn hóa để blend góc chéo đẹp
            if (move.sqrMagnitude > 1f)
                move.Normalize();

            // Damping khi đang di chuyển
            animator.SetFloat(moveXParam, move.x, dampTime, Time.deltaTime);
            animator.SetFloat(moveYParam, move.y, dampTime, Time.deltaTime);
        }

    }
}
