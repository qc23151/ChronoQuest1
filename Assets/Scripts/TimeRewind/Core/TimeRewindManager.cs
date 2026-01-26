using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeRewind
{
    /// <summary>
    /// Central manager for the time rewind system.
    /// Handles registration of rewindable objects, state recording, and rewind playback.
    /// </summary>
    public class TimeRewindManager : MonoBehaviour
    {
        #region Singleton
        
        private static TimeRewindManager _instance;
        
        /// <summary>
        /// Singleton instance of the TimeRewindManager
        /// </summary>
        public static TimeRewindManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<TimeRewindManager>();
                    
                    if (_instance == null)
                    {
                        var go = new GameObject("TimeRewindManager");
                        _instance = go.AddComponent<TimeRewindManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                
                // Ensure initialization even if Awake hasn't run yet
                _instance.EnsureInitialized();
                
                return _instance;
            }
        }
        
        #endregion

        #region Configuration
        
        [Header("Rewind Settings")]
        [Tooltip("Maximum duration of rewind history in seconds")]
        [SerializeField] private float maxRewindDuration = 5f;
        
        [Tooltip("How many states to record per second (higher = smoother but more memory)")]
        [SerializeField] private int recordsPerSecond = 50;
        
        [Tooltip("Speed multiplier for rewinding (1 = real-time, 2 = double speed)")]
        [SerializeField] private float rewindSpeed = 1f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        #endregion

        #region State
        
        private Dictionary<IRewindable, RewindBuffer<RewindState>> _rewindables;
        private bool _isRewinding;
        private float _currentRewindTime;
        private float _recordTimer;
        private float _recordInterval;
        private bool _initialized;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Whether time is currently being rewound
        /// </summary>
        public bool IsRewinding => _isRewinding;
        
        /// <summary>
        /// Whether there is enough history to rewind
        /// </summary>
        public bool CanRewind
        {
            get
            {
                if (_rewindables == null || _rewindables.Count == 0)
                    return false;
                    
                foreach (var kvp in _rewindables)
                {
                    if (kvp.Value.HasStates)
                        return true;
                }
                return false;
            }
        }
        
        /// <summary>
        /// Progress of rewind (0 = current time, 1 = oldest recorded time)
        /// </summary>
        public float RewindProgress
        {
            get
            {
                if (!_isRewinding || !CanRewind)
                    return 0f;
                
                float oldestTime = GetOldestRecordedTime();
                float newestTime = GetNewestRecordedTime();
                float totalDuration = newestTime - oldestTime;
                
                if (totalDuration <= 0)
                    return 0f;
                
                return 1f - ((_currentRewindTime - oldestTime) / totalDuration);
            }
        }
        
        /// <summary>
        /// Remaining rewind time available in seconds
        /// </summary>
        public float RemainingRewindTime
        {
            get
            {
                if (!CanRewind)
                    return 0f;
                
                float oldestTime = GetOldestRecordedTime();
                return _currentRewindTime - oldestTime;
            }
        }
        
        /// <summary>
        /// Maximum rewind duration setting
        /// </summary>
        public float MaxRewindDuration => maxRewindDuration;
        
        #endregion

        #region Events
        
        /// <summary>
        /// Called when rewind starts
        /// </summary>
        public event Action OnRewindStart;
        
        /// <summary>
        /// Called when rewind stops
        /// </summary>
        public event Action OnRewindStop;
        
        /// <summary>
        /// Called every frame during rewind with progress (0-1)
        /// </summary>
        public event Action<float> OnRewindProgress;
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            EnsureInitialized();
        }
        
        private void EnsureInitialized()
        {
            if (_initialized)
                return;
                
            _rewindables = new Dictionary<IRewindable, RewindBuffer<RewindState>>();
            _recordInterval = 1f / recordsPerSecond;
            _recordTimer = 0f;
            _initialized = true;
            
            if (enableDebugLogs)
                Debug.Log("[TimeRewind] Manager initialized");
        }
        
        private void Update()
        {
            if (_isRewinding)
            {
                UpdateRewind();
            }
        }
        
        private void FixedUpdate()
        {
            if (!_isRewinding)
            {
                UpdateRecording();
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Register a rewindable object with the manager
        /// </summary>
        /// <param name="rewindable">The object to register</param>
        public void Register(IRewindable rewindable)
        {
            if (rewindable == null)
                return;
                
            EnsureInitialized();
            
            if (_rewindables.ContainsKey(rewindable))
                return;
            
            int bufferCapacity = Mathf.CeilToInt(maxRewindDuration * recordsPerSecond);
            _rewindables[rewindable] = new RewindBuffer<RewindState>(bufferCapacity);
            
            if (enableDebugLogs)
                Debug.Log($"[TimeRewind] Registered: {(rewindable as MonoBehaviour)?.gameObject.name ?? "Unknown"}");
        }
        
        /// <summary>
        /// Unregister a rewindable object from the manager
        /// </summary>
        /// <param name="rewindable">The object to unregister</param>
        public void Unregister(IRewindable rewindable)
        {
            if (rewindable == null)
                return;
            
            _rewindables.Remove(rewindable);
        }
        
        /// <summary>
        /// Start rewinding time (call when rewind button is pressed)
        /// </summary>
        public void StartRewind()
        {
            if (_isRewinding)
            {
                if (enableDebugLogs)
                    Debug.Log("[TimeRewind] StartRewind called but already rewinding");
                return;
            }
            
            if (!CanRewind)
            {
                if (enableDebugLogs)
                    Debug.Log($"[TimeRewind] StartRewind called but CanRewind=false (registered: {_rewindables?.Count ?? 0})");
                return;
            }
            
            _isRewinding = true;
            _currentRewindTime = Time.time;
            
            if (enableDebugLogs)
                Debug.Log($"[TimeRewind] Rewind STARTED at time {_currentRewindTime:F2}");
            
            // Notify all rewindables
            foreach (var rewindable in _rewindables.Keys)
            {
                var mb = rewindable as MonoBehaviour;
                if (mb != null)
                    rewindable.OnStartRewind();
            }
            
            OnRewindStart?.Invoke();
        }
        
        /// <summary>
        /// Stop rewinding time (call when rewind button is released)
        /// </summary>
        public void StopRewind()
        {
            if (!_isRewinding)
                return;
            
            _isRewinding = false;
            
            // Trim future states from all buffers
            TrimFutureStates();
            
            // Notify all rewindables
            foreach (var rewindable in _rewindables.Keys)
            {
                var mb = rewindable as MonoBehaviour;
                if (mb != null)
                    rewindable.OnStopRewind();
            }
            
            OnRewindStop?.Invoke();
        }
        
        /// <summary>
        /// Clear all recorded history
        /// </summary>
        public void ClearHistory()
        {
            foreach (var buffer in _rewindables.Values)
            {
                buffer.Clear();
            }
        }
        
        #endregion

        #region Private Methods
        
        private void UpdateRecording()
        {
            if (_rewindables == null || _rewindables.Count == 0)
                return;
                
            _recordTimer += Time.fixedDeltaTime;
            
            if (_recordTimer >= _recordInterval)
            {
                _recordTimer = 0f;
                RecordCurrentStates();
            }
        }
        
        private void RecordCurrentStates()
        {
            // Collect any destroyed objects to remove
            List<IRewindable> toRemove = null;
            
            foreach (var kvp in _rewindables)
            {
                // Check if the rewindable (MonoBehaviour) was destroyed
                var mb = kvp.Key as MonoBehaviour;
                if (mb == null)
                {
                    toRemove ??= new List<IRewindable>();
                    toRemove.Add(kvp.Key);
                    continue;
                }
                
                var state = kvp.Key.CaptureState();
                kvp.Value.Add(state);
            }
            
            // Remove destroyed objects
            if (toRemove != null)
            {
                foreach (var item in toRemove)
                {
                    _rewindables.Remove(item);
                    if (enableDebugLogs)
                        Debug.Log("[TimeRewind] Removed destroyed rewindable");
                }
            }
            
            // Log periodically (every ~1 second)
            if (enableDebugLogs && Time.frameCount % 50 == 0 && _rewindables.Count > 0)
            {
                var firstBuffer = _rewindables.Values.GetEnumerator();
                if (firstBuffer.MoveNext())
                {
                    Debug.Log($"[TimeRewind] Recording... States buffered: {firstBuffer.Current.Count}");
                }
            }
        }
        
        private void UpdateRewind()
        {
            // Move backward in time
            _currentRewindTime -= Time.deltaTime * rewindSpeed;
            
            float oldestTime = GetOldestRecordedTime();
            
            // Clamp to oldest available time
            if (_currentRewindTime < oldestTime)
            {
                _currentRewindTime = oldestTime;
            }
            
            // Apply interpolated states to all rewindables
            foreach (var kvp in _rewindables)
            {
                // Skip destroyed objects
                var mb = kvp.Key as MonoBehaviour;
                if (mb == null)
                    continue;
                
                var buffer = kvp.Value;
                
                if (buffer.GetInterpolationStates(
                    _currentRewindTime,
                    s => s.Timestamp,
                    out var before,
                    out var after,
                    out float t))
                {
                    var interpolatedState = RewindState.Lerp(before, after, t);
                    kvp.Key.ApplyState(interpolatedState);
                }
            }
            
            OnRewindProgress?.Invoke(RewindProgress);
        }
        
        private void TrimFutureStates()
        {
            foreach (var kvp in _rewindables)
            {
                var buffer = kvp.Value;
                
                if (!buffer.HasStates)
                    continue;
                
                // Find how many states to keep (those with timestamp <= current rewind time)
                int keepCount = 0;
                for (int i = 0; i < buffer.Count; i++)
                {
                    if (buffer.Get(i).Timestamp <= _currentRewindTime)
                        keepCount = i + 1;
                    else
                        break;
                }
                
                buffer.TrimToCount(keepCount);
            }
        }
        
        private float GetOldestRecordedTime()
        {
            float oldestTime = float.MaxValue;
            
            foreach (var buffer in _rewindables.Values)
            {
                if (buffer.HasStates)
                {
                    float bufferOldest = buffer.GetOldest().Timestamp;
                    if (bufferOldest < oldestTime)
                        oldestTime = bufferOldest;
                }
            }
            
            return oldestTime == float.MaxValue ? Time.time : oldestTime;
        }
        
        private float GetNewestRecordedTime()
        {
            float newestTime = 0f;
            
            foreach (var buffer in _rewindables.Values)
            {
                if (buffer.HasStates)
                {
                    float bufferNewest = buffer.GetNewest().Timestamp;
                    if (bufferNewest > newestTime)
                        newestTime = bufferNewest;
                }
            }
            
            return newestTime;
        }
        
        #endregion
    }
}
