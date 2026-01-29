using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
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
}
