using UnityEngine;

namespace TimeRewind
{
    /// <summary>
    /// A rewindable component for objects with Animator.
    /// Records and restores animator state during time rewind.
    /// Can be used alongside RewindableTransform or RewindableRigidbody2D.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class RewindableAnimator : MonoBehaviour, IRewindable
    {
        [Header("Settings")]
        [Tooltip("The animator layer to record (usually 0)")]
        [SerializeField] private int animatorLayer = 0;
        
        [Tooltip("Also record and restore transform position/rotation")]
        [SerializeField] private bool includeTransform = false;
        
        private Animator _animator;
        private bool _isRewinding;
        private float _originalSpeed;
        
        /// <summary>
        /// Whether this object is currently being rewound
        /// </summary>
        public bool IsRewinding => _isRewinding;
        
        /// <summary>
        /// Reference to the Animator component
        /// </summary>
        public Animator Animator => _animator;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        private void OnEnable()
        {
            TimeRewindManager.Instance.Register(this);
        }
        
        private void OnDisable()
        {
            if (TimeRewindManager.Instance != null)
            {
                TimeRewindManager.Instance.Unregister(this);
            }
        }
        
        #endregion

        #region IRewindable Implementation
        
        public void OnStartRewind()
        {
            _isRewinding = true;
            
            // Store and pause animator
            _originalSpeed = _animator.speed;
            _animator.speed = 0f;
        }
        
        public void OnStopRewind()
        {
            _isRewinding = false;
            
            // Restore animator speed
            _animator.speed = _originalSpeed;
        }
        
        public RewindState CaptureState()
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(animatorLayer);
            
            var state = new RewindState
            {
                Timestamp = Time.time,
                AnimatorStateHash = stateInfo.fullPathHash,
                AnimatorNormalizedTime = stateInfo.normalizedTime
            };
            
            if (includeTransform)
            {
                state.Position = transform.position;
                state.Rotation = transform.rotation;
            }
            
            return state;
        }
        
        public void ApplyState(RewindState state)
        {
            // Restore animator state
            _animator.Play(state.AnimatorStateHash, animatorLayer, state.AnimatorNormalizedTime);
            
            // Force animator to update immediately
            _animator.Update(0f);
            
            if (includeTransform)
            {
                transform.position = state.Position;
                transform.rotation = state.Rotation;
            }
        }
        
        #endregion
    }
}
