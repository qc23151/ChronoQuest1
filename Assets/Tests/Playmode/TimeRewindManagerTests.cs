using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using TimeRewind;

public class TimeRewindManagerTests
{
    [UnityTest]
    public IEnumerator TimeRewindManagerExistsInScene()
    {
        yield return SceneManager.LoadSceneAsync("MovementPlayground", LoadSceneMode.Single);
        yield return null;

        var manager = Object.FindFirstObjectByType<TimeRewindManager>();
        Assert.IsNotNull(manager, "TimeRewindManager should exist in the scene");
    }

    [UnityTest]
    public IEnumerator TimeRewindManagerStartsNotRewinding()
    {
        yield return SceneManager.LoadSceneAsync("MovementPlayground", LoadSceneMode.Single);
        yield return null;

        var manager = Object.FindFirstObjectByType<TimeRewindManager>();
        Assert.IsFalse(manager.IsRewinding, "TimeRewindManager should not be rewinding on start");
    }

    [UnityTest]
    public IEnumerator PlayerHasRewindableComponent()
    {
        yield return SceneManager.LoadSceneAsync("MovementPlayground", LoadSceneMode.Single);
        yield return null;

        GameObject player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player, "Player should exist");

        var rewindable = player.GetComponent<IRewindable>();
        Assert.IsNotNull(rewindable, "Player should have a component implementing IRewindable");
    }


    [UnityTest]
    public IEnumerator RewindCanBeStartedAndStopped()
    {
        yield return SceneManager.LoadSceneAsync("MovementPlayground", LoadSceneMode.Single);
        yield return null;

        var manager = Object.FindFirstObjectByType<TimeRewindManager>();
        Assert.IsNotNull(manager, "TimeRewindManager should exist");

        // Wait for some states to be recorded
        yield return new WaitForSeconds(0.5f);

        // Initially not rewinding
        Assert.IsFalse(manager.IsRewinding, "Should not be rewinding initially");

        // Start rewind
        manager.StartRewind();
        Assert.IsTrue(manager.IsRewinding, "Should be rewinding after StartRewind");

        yield return null;

        // Stop rewind
        manager.StopRewind();
        Assert.IsFalse(manager.IsRewinding, "Should not be rewinding after StopRewind");
    }

    [UnityTest]
    public IEnumerator RewindRestoresPlayerToHigherPosition()
    {
        yield return SceneManager.LoadSceneAsync("MovementPlayground", LoadSceneMode.Single);
        yield return null;

        GameObject player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player, "Player should exist");

        var manager = Object.FindFirstObjectByType<TimeRewindManager>();
        Assert.IsNotNull(manager, "TimeRewindManager should exist");

        float startHeight = player.transform.position.y;

        // Wait a short time for the player to fall slightly (but NOT land on the ground)
        // This ensures we have recorded states at different heights
        yield return new WaitForSeconds(0.5f);

        float heightAfterFalling = player.transform.position.y;
        Assert.Less(heightAfterFalling, startHeight, "Player should have fallen before rewinding");
        Assert.IsTrue(manager.CanRewind, "Manager should have recorded states to rewind");

        // Simulate holding R using the Input System so the controller keeps rewind active.
        var keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
        InputSystem.Update();

        // Give Update() a frame to process input and start rewind.
        yield return null;

        if (!manager.IsRewinding)
        {
            manager.StartRewind();
        }
        Assert.IsTrue(manager.IsRewinding, "Manager should be rewinding after StartRewind()");

        // Poll until player height increases (timeScale is reduced during rewind, so wait in real time)
        float heightDuringRewind = heightAfterFalling;
        float timeoutSeconds = 5f;
        float elapsed = 0f;
        float pollInterval = 0.05f;

        while (elapsed < timeoutSeconds)
        {
            yield return new WaitForSecondsRealtime(pollInterval);
            elapsed += pollInterval;

            heightDuringRewind = player.transform.position.y;
            // Check for any meaningful increase in height
            if (heightDuringRewind > heightAfterFalling + 0.1f)
                break;
        }

        manager.StopRewind();
        Assert.IsFalse(manager.IsRewinding, "Manager should stop rewinding after StopRewind()");

        // Release R key after rewind
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        InputSystem.Update();

        Assert.Greater(heightDuringRewind, heightAfterFalling,
            $"Player should have rewound to a higher position. Before rewind: {heightAfterFalling:F2}, After rewind: {heightDuringRewind:F2}");
    }
}
