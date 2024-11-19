using System.Linq;
using UnityEngine;

public class CrossObject : MonoBehaviour
{
    public float attackDamage;
    public GameObject[] monstersHit;

    void Start()
    {
        monstersHit = default;
    }


    void OnTriggerEneter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            Monster monster = other.GetComponent<Monster>();
            monster.TakeDamage(attackDamage);
            monstersHit.Append(other.gameObject);
        }
    }
}
