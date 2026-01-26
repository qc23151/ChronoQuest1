using UnityEngine;

namespace TimeRewind
{
    /// <summary>
    /// Interface for objects that can be rewound in time.
    /// Implement this on any component that needs to participate in the rewind system.
    /// </summary>
    public interface IRewindable
    {
        /// <summary>
        /// Called when time rewind begins.
        /// Use this to disable physics, input, or other systems that shouldn't run during rewind.
        /// </summary>
        void OnStartRewind();

        /// <summary>
        /// Called when time rewind ends and normal gameplay resumes.
        /// Use this to re-enable physics, input, and restore normal operation.
        /// </summary>
        void OnStopRewind();

        /// <summary>
        /// Capture the current state of this object for recording.
        /// Called every recording interval during normal gameplay.
        /// </summary>
        /// <returns>A snapshot of the current state</returns>
        RewindState CaptureState();

        /// <summary>
        /// Apply a previously recorded state to this object.
        /// Called during rewind to restore the object to a past state.
        /// </summary>
        /// <param name="state">The state to apply</param>
        void ApplyState(RewindState state);
    }
}
