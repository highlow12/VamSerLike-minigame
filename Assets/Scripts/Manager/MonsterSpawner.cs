using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 몬스터 생성을 관리하는 싱글톤 매니저
public class MonsterSpawner : Singleton<MonsterSpawner>
{
    // 스테이지에서 사용할 몬스터 데이터 정보
    [Tooltip("몬스터 소환에 필요한 StageMonsterData")]
    public StageMonsterData stageMonsterData;
    
    // 현재 실행 중인 몬스터 스폰 코루틴들
    private Dictionary<string, Coroutine> spawnRoutines = new Dictionary<string, Coroutine>();

    // 몬스터가 플레이어로부터 생성되는 최소/최대 거리
    [Tooltip("몬스터가 생성되는 최소 반경")]
    public float MinimumSpawnRadius = 20f;
    [Tooltip("몬스터가 생성되는 최대 반경")]
    public float MaximumSpawnRadius = 30f;

    // 새로운 스테이지 데이터로 업데이트
    public void UpdateStageMonsterData(StageMonsterData newData)
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
        //StartSpawning();
    }

    // 모든 종류의 몬스터 랜덤 스폰 시작
    private void StartAllRandomSpawning()
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

    // 특정 몬스터의 랜덤 스폰 시작
    // monsterName: 스폰할 몬스터의 이름
    public void StartRandomSpawning(string monsterName)
    {
        var monsterData = stageMonsterData.monsters.Find(x => x.monsterName == monsterName);
        if (monsterData.Equals(default(StageMonsterData.MonsterData)))
        {
            Debug.LogWarning("MonsterData not found: " + monsterName);
            return;
        }
        if (spawnRoutines.ContainsKey(monsterName))
        {
            Debug.LogWarning("Monster is already spawning: " + monsterName);
            return;
        }
        var routine = StartCoroutine(SpawnRoutine(monsterData));
        spawnRoutines[monsterName] = routine;
    }

    // 특정 몬스터의 스폰 중단
    // monsterName: 중단할 몬스터의 이름
    public void StopSpawningMonster(string monsterName)
    {
        if (spawnRoutines.ContainsKey(monsterName))
        {
            StopCoroutine(spawnRoutines[monsterName]);
            spawnRoutines.Remove(monsterName);
        }
    }

    // 모든 몬스터 스폰 중단
    public void StopAllSpawning()
    {
        foreach (var routine in spawnRoutines.Values)
        {
            StopCoroutine(routine);
        }
        spawnRoutines.Clear();
    }

    // 몬스터의 주기적 스폰을 담당하는 코루틴
    // monsterData: 스폰할 몬스터의 데이터
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

    // 지정된 위치에 특정 몬스터를 즉시 생성
    // monsterName: 생성할 몬스터의 이름
    // position: 생성 위치
    // returns: 생성된 몬스터 게임오브젝트
    public GameObject SpawnMonster(string monsterName, Vector2 position)
    {
        var monster = MonsterPoolManager.Instance.GetMonsterFromPool(monsterName);
        if (monster != null)
        {
            monster.transform.position = position;
        }else
        {
            Debug.LogWarning("Monster not found in pool: " + monsterName);
        }
        return monster;
    }
}
