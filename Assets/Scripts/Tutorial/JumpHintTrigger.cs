using UnityEngine;

public class JumpHintTrigger : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorial; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Entered jump hint trigger"); 
        if (!other.CompareTag("Player")) return; 

        tutorial.TriggerJumpHint(); 
    }
}
