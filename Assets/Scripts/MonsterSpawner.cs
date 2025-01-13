using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// MonsterSpawner는 주어진 StageMonsterData를 바탕으로 몬스터를 주기적으로 소환합니다.
public class MonsterSpawner : Singleton<MonsterSpawner>
{

    [Tooltip("몬스터 소환에 필요한 StageMonsterData.")]
    public StageMonsterData stageMonsterData;
    
    private Dictionary<string, Coroutine> spawnRoutines = new Dictionary<string, Coroutine>();
    [Tooltip("몬스터가 생성되는 최소 반경.")]
    public float MinimumSpawnRadius = 20f;
    [Tooltip("몬스터가 생성되는 최대 반경.")]
    public float MaximumSpawnRadius = 30f;

    public void UpdateSategwMonsterData(StageMonsterData newData)
    {
        stageMonsterData = newData;
        MonsterPoolManager.Instance.UpdateStageMonsterData(stageMonsterData);
    }

    private void Start()
    {
        if (stageMonsterData == null)
        {
            Debug.LogWarning("StageMonsterData is not set!");
            return;
        }
        MonsterPoolManager.Instance.UpdateStageMonsterData(stageMonsterData);
        StartSpawning();
    }
    // StartSpawning()에서 monsterData별 코루틴을 만들어 몬스터를 스폰합니다.
    private void StartSpawning()
    {
        if (spawnRoutines == null)
            spawnRoutines = new Dictionary<string, Coroutine>();
            
        spawnRoutines.Clear();
        foreach (var monsterData in stageMonsterData.monsters)
        {
            var routine = StartCoroutine(SpawnRoutine(monsterData));
            spawnRoutines[monsterData.monsterName] = routine;
        }
    }
    // StopSpawningMonster(), StopAllSpawning()으로 개별 혹은 전체 코루틴을 중단할 수 있습니다.
    public void StopSpawningMonster(string monsterName)
    {
        
        if (spawnRoutines.ContainsKey(monsterName))
        {
            StopCoroutine(spawnRoutines[monsterName]);
            spawnRoutines.Remove(monsterName);
        }
    }

    public void StopAllSpawning()
    {
        foreach (var routine in spawnRoutines.Values)
        {
            StopCoroutine(routine);
        }
        spawnRoutines.Clear();
    }
    // SpawnRoutine()에서 spawnRate를 이용해 기다렸다가 MonsterPoolManager에서 몬스터를 꺼냅니다.
    private IEnumerator SpawnRoutine(StageMonsterData.MonsterData monsterData)
    {
        
        while (true)
        {
            yield return new WaitForSeconds(1f / monsterData.spawnRate);
            var monster = MonsterPoolManager.Instance.GetMonsterFromPool(monsterData.monsterName);

            if (monster != null)
            {
                var SpawnDir = Random.insideUnitCircle.normalized;
                var spawnPos = SpawnDir * Random.Range(MinimumSpawnRadius, MaximumSpawnRadius) ;
                monster.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0) + GameManager.Instance.player.transform.position;
            }
        }
    }
}
