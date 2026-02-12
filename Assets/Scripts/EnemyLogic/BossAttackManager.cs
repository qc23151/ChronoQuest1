using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    public GameObject fireball;
    public GameObject fireColumn;
    public GameObject fireRow;
    public GameObject fireWave;
    public Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void spawnFireball()
    {
        float randX = Random.Range(-12f, 2.5f);
        Instantiate(fireball, new Vector3(randX,9f,0f), transform.rotation);
    }

    public void spawnFireColumns()
    {
        float playerX = player.transform.position.x;
        Instantiate(fireColumn, new Vector3(playerX,-1.2f,0f), transform.rotation);
    }

    public void spawnFireRow()
    {
        Instantiate(fireRow, new Vector3(2.5f, -4.25f, 0f), transform.rotation);
    }

    public void spawnFireWave()
    {
        Instantiate(fireWave, new Vector3(4.5f, -6.5f, 0f), transform.rotation);
    }
}
