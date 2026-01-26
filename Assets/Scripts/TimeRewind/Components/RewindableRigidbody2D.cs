using UnityEngine;

namespace TimeRewind
{
    /// <summary>
    /// A rewindable component for objects with Rigidbody2D physics.
    /// Records position, rotation, and velocity data.
    /// Automatically handles kinematic state during rewind.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class RewindableRigidbody2D : MonoBehaviour, IRewindable
    {
        private Rigidbody2D _rb;
        private bool _isRewinding;
        private bool _wasKinematic;
        private RigidbodyType2D _originalBodyType;
        
        /// <summary>
        /// Whether this object is currently being rewound
        /// </summary>
        public bool IsRewinding => _isRewinding;
        
        /// <summary>
        /// Reference to the Rigidbody2D component
        /// </summary>
        public Rigidbody2D Rigidbody => _rb;
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        
        protected virtual void OnEnable()
        {
            TimeRewindManager.Instance.Register(this);
        }
        
        protected virtual void OnDisable()
        {
            if (TimeRewindManager.Instance != null)
            {
                TimeRewindManager.Instance.Unregister(this);
            }
        }
        
        #endregion

        #region IRewindable Implementation
        
        public virtual void OnStartRewind()
        {
            _isRewinding = true;
            
            // Store original physics state
            _wasKinematic = _rb.isKinematic;
            _originalBodyType = _rb.bodyType;
            
            // Disable physics during rewind
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
        
        public virtual void OnStopRewind()
        {
            _isRewinding = false;
            
            // Restore original physics state
            _rb.bodyType = _originalBodyType;
        }
        
        public virtual RewindState CaptureState()
        {
            return RewindState.CreateWithPhysics(
                transform.position,
                transform.rotation,
                _rb.linearVelocity,
                _rb.angularVelocity,
                Time.time
            );
        }
        
        public virtual void ApplyState(RewindState state)
        {
            transform.position = state.Position;
            transform.rotation = state.Rotation;
            
            // Store velocity for when we resume
            // Note: We don't apply velocity during rewind since physics is disabled
            // The velocity will be applied when OnStopRewind is called
        }
        
        /// <summary>
        /// Called after OnStopRewind to apply the final velocity state.
        /// Override this if you need custom velocity restoration behavior.
        /// </summary>
        protected virtual void RestoreVelocity(RewindState lastState)
        {
            if (!_wasKinematic)
            {
                _rb.linearVelocity = lastState.Velocity;
                _rb.angularVelocity = lastState.AngularVelocity;
            }
        }
        
        #endregion
    }
}
