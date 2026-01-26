using System.Collections.Generic;
using UnityEngine;

namespace TimeRewind
{
    /// <summary>
    /// Represents a snapshot of an object's state at a specific point in time.
    /// Used for recording and restoring object states during time rewind.
    /// </summary>
    [System.Serializable]
    public struct RewindState
    {
        /// <summary>
        /// The timestamp when this state was recorded (using Time.time)
        /// </summary>
        public float Timestamp;

        /// <summary>
        /// World position of the object
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// World rotation of the object
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Linear velocity (for Rigidbody objects)
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// Angular velocity (for Rigidbody objects)
        /// </summary>
        public float AngularVelocity;

        /// <summary>
        /// Animator state hash (for animated objects)
        /// </summary>
        public int AnimatorStateHash;

        /// <summary>
        /// Animator normalized time (for animated objects)
        /// </summary>
        public float AnimatorNormalizedTime;

        /// <summary>
        /// Custom data dictionary for extensibility (health, ammo, etc.)
        /// Note: Only use serializable types as values
        /// </summary>
        public Dictionary<string, object> CustomData;

        /// <summary>
        /// Creates a new RewindState with the given transform data
        /// </summary>
        public static RewindState Create(Vector3 position, Quaternion rotation, float timestamp)
        {
            return new RewindState
            {
                Timestamp = timestamp,
                Position = position,
                Rotation = rotation,
                Velocity = Vector2.zero,
                AngularVelocity = 0f,
                AnimatorStateHash = 0,
                AnimatorNormalizedTime = 0f,
                CustomData = null
            };
        }

        /// <summary>
        /// Creates a new RewindState with transform and physics data
        /// </summary>
        public static RewindState CreateWithPhysics(
            Vector3 position, 
            Quaternion rotation, 
            Vector2 velocity, 
            float angularVelocity,
            float timestamp)
        {
            return new RewindState
            {
                Timestamp = timestamp,
                Position = position,
                Rotation = rotation,
                Velocity = velocity,
                AngularVelocity = angularVelocity,
                AnimatorStateHash = 0,
                AnimatorNormalizedTime = 0f,
                CustomData = null
            };
        }

        /// <summary>
        /// Linearly interpolate between two states
        /// </summary>
        /// <param name="a">Start state</param>
        /// <param name="b">End state</param>
        /// <param name="t">Interpolation factor (0-1)</param>
        /// <returns>Interpolated state</returns>
        public static RewindState Lerp(RewindState a, RewindState b, float t)
        {
            return new RewindState
            {
                Timestamp = Mathf.Lerp(a.Timestamp, b.Timestamp, t),
                Position = Vector3.Lerp(a.Position, b.Position, t),
                Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t),
                Velocity = Vector2.Lerp(a.Velocity, b.Velocity, t),
                AngularVelocity = Mathf.Lerp(a.AngularVelocity, b.AngularVelocity, t),
                AnimatorStateHash = t < 0.5f ? a.AnimatorStateHash : b.AnimatorStateHash,
                AnimatorNormalizedTime = Mathf.Lerp(a.AnimatorNormalizedTime, b.AnimatorNormalizedTime, t),
                CustomData = t < 0.5f ? a.CustomData : b.CustomData
            };
        }

        /// <summary>
        /// Set a custom data value
        /// </summary>
        public void SetCustomData(string key, object value)
        {
            CustomData ??= new Dictionary<string, object>();
            CustomData[key] = value;
        }

        /// <summary>
        /// Get a custom data value
        /// </summary>
        public T GetCustomData<T>(string key, T defaultValue = default)
        {
            if (CustomData == null || !CustomData.TryGetValue(key, out var value))
                return defaultValue;
            
            return (T)value;
        }
    }
}
