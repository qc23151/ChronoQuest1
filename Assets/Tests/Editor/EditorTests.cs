using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class EditorTests
{
    [Test]
    public void ValidSprite()
    {
        // WARNING : This filepath will need to change when we update the sprite used for main player 
        GameObject playerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Sprites/MeleeSkeleton/Skull Boy.aseprite");
        Assert.IsNotNull(playerSprite, "Couldn't find player sprite");
    }

    [Test]
    public void ValidPlayerMovementScript()
    {
        MonoScript script = UnityEditor.AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/PlayerMovement.cs");
        Assert.IsNotNull(script, "Couldn't find script for player movement!");
    }

}
