using System.Collections;
using UnityEngine;

public class SmapleMonter : MonoBehaviour
{
    MonsterIdentify monsterIdentify;
    void Start()
    {
        monsterIdentify = GetComponent<MonsterIdentify>();
        StartCoroutine(Despawn());
    }
    // Update is called once per frame
    void Update()
    {
        var dir = GameManager.Instance.player.transform.position - transform.position;
        dir.Normalize();
        transform.position += dir * Time.deltaTime * 3;
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(6);
        MonsterPoolManager.Instance.ReturnMonsterToPool(gameObject, monsterIdentify.monsterName);
    }
}

