using UnityEngine;
using System.Collections.Generic;

// MonsterPoolManager는 각 몬스터의 풀(Queue)을 관리하는 싱글톤 클래스입니다.
public class MonsterPoolManager : Singleton<MonsterPoolManager>
{
    // stageMonsterData에는 몬스터 정보(이름, 프리팹, 스폰율 등)가 저장되어 있습니다.
    [HideInInspector]
    public StageMonsterData stageMonsterData;
    private Dictionary<string, Queue<GameObject>> monsterPools = new Dictionary<string, Queue<GameObject>>();

    public void UpdateStageMonsterData(StageMonsterData newData)
    {
        stageMonsterData = newData;
        InitializePools();
    }

    // InitializePools()에서 기존 몬스터 풀을 정리하고 새로 구성합니다.
    private void InitializePools()
    {
        monsterPools.Clear();

        foreach (var monster in stageMonsterData.monsters)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            int initialPoolSize = Mathf.CeilToInt(monster.spawnRate * 2);
            
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject newMonster = Instantiate(monster.monsterPrefab);
                newMonster.SetActive(false);
                pool.Enqueue(newMonster);
            }
            
            monsterPools[monster.monsterName] = pool;
        }
    }

    private GameObject GetPrefabByName(string monsterName)
    {
        var monsterData = stageMonsterData.monsters.Find(m => m.monsterName == monsterName);
        if (monsterData.monsterPrefab == null)
        {
            Debug.LogWarning($"Monster '{monsterName}' not found!");
            return null;
        }
        return monsterData.monsterPrefab;
    }

    // GetMonsterFromPool()에서 풀에 남은 몬스터가 없으면 Instantiate로 새롭게 생성합니다.
    public GameObject GetMonsterFromPool(string monsterName)
    {
        if (string.IsNullOrEmpty(monsterName) || !monsterPools.ContainsKey(monsterName))
        {
            Debug.LogWarning($"Pool for monster '{monsterName}' doesn't exist!");
            return null;
        }

        Queue<GameObject> pool = monsterPools[monsterName];
        
        if (pool.Count == 0)
        {
            GameObject newMonster = Instantiate(GetPrefabByName(monsterName));
            if (newMonster == null) return null;
            return newMonster;
        }

        GameObject monster = pool.Dequeue();
        if (monster != null) monster.SetActive(true);
        return monster;
    }

    // ReturnMonsterToPool()으로 몬스터를 다시 비활성화 후 풀에 반환합니다.
    public void ReturnMonsterToPool(GameObject monster, string monsterName)
    {
        if (monster == null || !monsterPools.ContainsKey(monsterName))
        {
            Debug.LogWarning($"Cannot return monster to pool: {monsterName}");
            return;
        }

        monster.SetActive(false);
        monsterPools[monsterName].Enqueue(monster);
    }
}
