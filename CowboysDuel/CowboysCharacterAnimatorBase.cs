using RootMotion.FinalIK;
using UnityEngine;

namespace _Scripts.Core.Characters
{
    public class CowboysCharacterAnimatorBase : MonoBehaviour
    {
        [field: SerializeField] public Animator Animator { get; private set; }
        [SerializeField] private FullBodyBipedIK _fullBodyBipedIk;
        private static readonly int vertical = Animator.StringToHash("Vertical");
        private static readonly int horizontal = Animator.StringToHash("Horizontal");
        private Vector3 _currentMoveDirection;
        private float _animationChangeSpeed = 10f;

        public void OnKilled()
        {
            Animator.StopPlayback();
            Animator.enabled = false;
            _fullBodyBipedIk.enabled = false;
        }

        public void SetMoveDirection(Vector3 directionInLocalSpace)
        {
            directionInLocalSpace = Vector3.Lerp(_currentMoveDirection, directionInLocalSpace,
                Time.deltaTime * _animationChangeSpeed);
            _currentMoveDirection = directionInLocalSpace;
            Animator.SetFloat(vertical, directionInLocalSpace.z);
            Animator.SetFloat(horizontal, directionInLocalSpace.x);
        }
    }
}