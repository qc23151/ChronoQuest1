using System;
using UnityEngine;

namespace TimeRewind
{
    /// <summary>
    /// A circular buffer for storing rewind states efficiently.
    /// Automatically overwrites oldest entries when capacity is reached.
    /// </summary>
    /// <typeparam name="T">The type of state to store</typeparam>
    public class RewindBuffer<T> where T : struct
    {
        private readonly T[] _buffer;
        private readonly int _capacity;
        private int _head;      // Points to the next write position
        private int _tail;      // Points to the oldest entry
        private int _count;

        /// <summary>
        /// Current number of states stored in the buffer
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Maximum capacity of the buffer
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// Whether the buffer has any states stored
        /// </summary>
        public bool HasStates => _count > 0;

        /// <summary>
        /// Whether the buffer is full
        /// </summary>
        public bool IsFull => _count == _capacity;

        /// <summary>
        /// Creates a new RewindBuffer with the specified capacity
        /// </summary>
        /// <param name="capacity">Maximum number of states to store</param>
        public RewindBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than zero", nameof(capacity));

            _capacity = capacity;
            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        /// <summary>
        /// Add a new state to the buffer.
        /// If the buffer is full, the oldest state will be overwritten.
        /// </summary>
        /// <param name="state">The state to add</param>
        public void Add(T state)
        {
            _buffer[_head] = state;
            _head = (_head + 1) % _capacity;

            if (_count < _capacity)
            {
                _count++;
            }
            else
            {
                // Buffer is full, move tail forward (oldest entry is overwritten)
                _tail = (_tail + 1) % _capacity;
            }
        }

        /// <summary>
        /// Get a state at a specific index (0 = oldest, Count-1 = newest)
        /// </summary>
        /// <param name="index">Index from oldest to newest</param>
        /// <returns>The state at the specified index</returns>
        public T Get(int index)
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            int actualIndex = (_tail + index) % _capacity;
            return _buffer[actualIndex];
        }

        /// <summary>
        /// Get the newest (most recent) state
        /// </summary>
        /// <returns>The most recent state</returns>
        public T GetNewest()
        {
            if (_count == 0)
                throw new InvalidOperationException("Buffer is empty");

            int newestIndex = (_head - 1 + _capacity) % _capacity;
            return _buffer[newestIndex];
        }

        /// <summary>
        /// Get the oldest state
        /// </summary>
        /// <returns>The oldest state</returns>
        public T GetOldest()
        {
            if (_count == 0)
                throw new InvalidOperationException("Buffer is empty");

            return _buffer[_tail];
        }

        /// <summary>
        /// Remove and return the newest state (pop from the end)
        /// </summary>
        /// <returns>The newest state that was removed</returns>
        public T PopNewest()
        {
            if (_count == 0)
                throw new InvalidOperationException("Buffer is empty");

            _head = (_head - 1 + _capacity) % _capacity;
            _count--;
            return _buffer[_head];
        }

        /// <summary>
        /// Remove all states added after the specified index.
        /// Useful when resuming from a rewound position.
        /// </summary>
        /// <param name="keepCount">Number of states to keep (from oldest)</param>
        public void TrimToCount(int keepCount)
        {
            if (keepCount < 0)
                throw new ArgumentOutOfRangeException(nameof(keepCount));

            if (keepCount >= _count)
                return;

            _count = keepCount;
            _head = (_tail + keepCount) % _capacity;
        }

        /// <summary>
        /// Clear all states from the buffer
        /// </summary>
        public void Clear()
        {
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        /// <summary>
        /// Find the index of the state closest to the target timestamp.
        /// Assumes states are ordered by timestamp (oldest to newest).
        /// </summary>
        /// <param name="targetTimestamp">The timestamp to search for</param>
        /// <param name="getTimestamp">Function to extract timestamp from state</param>
        /// <returns>Index of the closest state, or -1 if buffer is empty</returns>
        public int FindClosestIndex(float targetTimestamp, Func<T, float> getTimestamp)
        {
            if (_count == 0)
                return -1;

            // Binary search for efficiency
            int left = 0;
            int right = _count - 1;

            while (left < right)
            {
                int mid = (left + right) / 2;
                float midTimestamp = getTimestamp(Get(mid));

                if (midTimestamp < targetTimestamp)
                    left = mid + 1;
                else
                    right = mid;
            }

            // Check if we should return left or left-1
            if (left > 0)
            {
                float leftTimestamp = getTimestamp(Get(left));
                float prevTimestamp = getTimestamp(Get(left - 1));
                
                if (Mathf.Abs(prevTimestamp - targetTimestamp) < Mathf.Abs(leftTimestamp - targetTimestamp))
                    return left - 1;
            }

            return left;
        }

        /// <summary>
        /// Get two states that bracket the target timestamp for interpolation.
        /// </summary>
        /// <param name="targetTimestamp">The timestamp to find</param>
        /// <param name="getTimestamp">Function to extract timestamp from state</param>
        /// <param name="before">State before the timestamp</param>
        /// <param name="after">State after the timestamp</param>
        /// <param name="t">Interpolation factor (0-1)</param>
        /// <returns>True if bracketing states were found, false otherwise</returns>
        public bool GetInterpolationStates(
            float targetTimestamp, 
            Func<T, float> getTimestamp,
            out T before, 
            out T after, 
            out float t)
        {
            before = default;
            after = default;
            t = 0f;

            if (_count == 0)
                return false;

            if (_count == 1)
            {
                before = Get(0);
                after = before;
                t = 0f;
                return true;
            }

            // Find the first state with timestamp >= target
            int afterIndex = FindClosestIndex(targetTimestamp, getTimestamp);
            
            // Clamp to valid range
            afterIndex = Mathf.Clamp(afterIndex, 0, _count - 1);
            
            float afterTimestamp = getTimestamp(Get(afterIndex));
            
            // If target is before the oldest state
            if (afterIndex == 0 && targetTimestamp <= afterTimestamp)
            {
                before = Get(0);
                after = before;
                t = 0f;
                return true;
            }

            // If target is at or after the newest state
            if (afterIndex == _count - 1 && targetTimestamp >= afterTimestamp)
            {
                before = Get(_count - 1);
                after = before;
                t = 0f;
                return true;
            }

            // Find the state before
            int beforeIndex = afterIndex > 0 ? afterIndex - 1 : afterIndex;
            
            before = Get(beforeIndex);
            after = Get(afterIndex);

            float beforeTimestamp = getTimestamp(before);
            afterTimestamp = getTimestamp(after);

            // Calculate interpolation factor
            float duration = afterTimestamp - beforeTimestamp;
            if (duration > 0)
                t = (targetTimestamp - beforeTimestamp) / duration;
            else
                t = 0f;

            t = Mathf.Clamp01(t);
            return true;
        }
    }
}
