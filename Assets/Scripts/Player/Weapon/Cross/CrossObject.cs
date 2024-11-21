using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;


public class CrossObject : MonoBehaviour
{
    public float attackDamage;
    public List<GameObject> monstersHit;

    void Start()
    {
        monstersHit = new List<GameObject>();
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            if (monstersHit.Contains(other.gameObject))
            {
                return;
            }
            Monster monster = other.GetComponent<Monster>();
            monster.TakeDamage(attackDamage);
            monstersHit = monstersHit.Append(other.gameObject).ToList();
        }
    }
}
