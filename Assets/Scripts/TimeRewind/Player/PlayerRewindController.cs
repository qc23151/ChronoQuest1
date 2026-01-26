using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeRewind
{
    /// <summary>
    /// Handles player-specific rewind functionality.
    /// Add this component alongside PlayerPlatformer to enable time rewind for the player.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerRewindController : MonoBehaviour, IRewindable
    {
        [Header("Input")]
        [Tooltip("The key to hold for rewinding time")]
        [SerializeField] private Key rewindKey = Key.R;
        
        private Rigidbody2D _rb;
        private bool _isRewinding;
        private bool _rewindInputHeld;
        private RigidbodyType2D _originalBodyType;
        
        // Cache the last state for velocity restoration
        private RewindState _lastAppliedState;
        
        /// <summary>
        /// Whether the player is currently being rewound
        /// </summary>
        public bool IsRewinding => _isRewinding;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        
        private void OnEnable()
        {
            Debug.Log("[TimeRewind] PlayerRewindController OnEnable - registering with manager");
            TimeRewindManager.Instance.Register(this);
        }
        
        private void OnDisable()
        {
            if (TimeRewindManager.Instance != null)
            {
                TimeRewindManager.Instance.Unregister(this);
            }
        }
        
        private void Update()
        {
            // Poll for rewind input directly using the new Input System
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                bool wasHeld = _rewindInputHeld;
                _rewindInputHeld = keyboard[rewindKey].isPressed;
                
                // Log state changes
                if (_rewindInputHeld && !wasHeld)
                    Debug.Log("[TimeRewind] Rewind input PRESSED (R key)");
                else if (!_rewindInputHeld && wasHeld)
                    Debug.Log("[TimeRewind] Rewind input RELEASED");
            }
            
            // Handle rewind input state changes
            if (_rewindInputHeld && !TimeRewindManager.Instance.IsRewinding)
            {
                TimeRewindManager.Instance.StartRewind();
            }
            else if (!_rewindInputHeld && TimeRewindManager.Instance.IsRewinding)
            {
                TimeRewindManager.Instance.StopRewind();
            }
        }
        
        #endregion

        #region Input Callbacks (Alternative - for PlayerInput component)
        
        /// <summary>
        /// Called by the Input System when the Rewind action is triggered.
        /// This is an alternative to direct polling - use if you prefer PlayerInput component.
        /// </summary>
        public void OnRewind(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                _rewindInputHeld = true;
                Debug.Log("[TimeRewind] Rewind input PRESSED (via callback)");
            }
            else if (context.canceled)
            {
                _rewindInputHeld = false;
                Debug.Log("[TimeRewind] Rewind input RELEASED (via callback)");
            }
        }
        
        #endregion

        #region IRewindable Implementation
        
        public void OnStartRewind()
        {
            _isRewinding = true;
            
            // Store original physics state
            _originalBodyType = _rb.bodyType;
            
            // Disable physics during rewind
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
        
        public void OnStopRewind()
        {
            _isRewinding = false;
            
            // Restore original physics state
            _rb.bodyType = _originalBodyType;
            
            // Restore velocity from the last applied state
            if (_originalBodyType == RigidbodyType2D.Dynamic)
            {
                _rb.linearVelocity = _lastAppliedState.Velocity;
                _rb.angularVelocity = _lastAppliedState.AngularVelocity;
            }
        }
        
        public RewindState CaptureState()
        {
            return RewindState.CreateWithPhysics(
                transform.position,
                transform.rotation,
                _rb.linearVelocity,
                _rb.angularVelocity,
                Time.time
            );
        }
        
        public void ApplyState(RewindState state)
        {
            transform.position = state.Position;
            transform.rotation = state.Rotation;
            _lastAppliedState = state;
        }
        
        #endregion
    }
}
