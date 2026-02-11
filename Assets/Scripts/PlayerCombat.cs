using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Melee Settings")]
    [SerializeField] private float meleeRange = 1.5f;
    
    [Header("Spell Settings")]
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform firePoint; // Where the ball spawns (e.g., player's hand)

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // Melee Attack - Left Click
        if (mouse.leftButton.wasPressedThisFrame)
        {
            PerformMelee();
        }

        // Spell Attack - Right Click
        if (mouse.rightButton.wasPressedThisFrame)
        {
            PerformSpell();
        }
    }

    private void PerformMelee()
    {
        Debug.Log("Melee Attack Performed!");
        // Logic for checking hits would go here (e.g., Physics2D.OverlapCircle)
    }

    private void PerformSpell()
    {
        if (spellPrefab == null) return;

        // 1. Get Mouse Position in World Space
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = 10f; // Distance from camera
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        // 2. Calculate Direction
        Vector2 direction = (Vector2)worldMousePos - (Vector2)firePoint.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 3. Spawn and Rotate the Spell
        GameObject spell = Instantiate(spellPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
        Debug.Log("Spell Cast towards: " + worldMousePos);
    }
}