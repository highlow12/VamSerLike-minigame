using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 몬스터 생성을 관리하는 싱글톤 매니저
/// </summary>
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

    // 이벤트 위임자 선언
    public event Action<string, GameObject> OnMonsterSpawned;

    private void Start()
    {
        InitializePool();
    }

    /// <summary>
    /// 새로운 스테이지 데이터로 업데이트
    /// </summary>
    /// <param name="newData">새 스테이지 몬스터 데이터</param>
    public void UpdateStageMonsterData(StageMonsterData newData)
    {
        if (newData == null)
        {
            Debug.LogError("Attempted to update with null StageMonsterData");
            return;
        }

        stageMonsterData = newData;
        MonsterPoolManager.Instance.UpdateStageMonsterData(stageMonsterData);
    }

    /// <summary>
    /// 몬스터 풀 초기화
    /// </summary>
    private void InitializePool()
    {
        if (stageMonsterData == null)
        {
            Debug.LogWarning("StageMonsterData is not set!");
            return;
        }
        MonsterPoolManager.Instance.UpdateStageMonsterData(stageMonsterData);
    }

    /// <summary>
    /// 모든 종류의 몬스터 랜덤 스폰 시작
    /// </summary>
    public void StartAllRandomSpawning()
    {
        if (stageMonsterData == null || stageMonsterData.monsters == null || stageMonsterData.monsters.Count == 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning("No monster data available for spawning");
#endif
            return;
        }

        if (spawnRoutines == null)
            spawnRoutines = new Dictionary<string, Coroutine>();
            
        StopAllSpawning(); // 이전에 실행 중인 스폰 중지
        
        foreach (var monsterData in stageMonsterData.monsters)
        {
            if (monsterData.Equals(default(StageMonsterData.MonsterData)) || string.IsNullOrEmpty(monsterData.monsterName))
            {
                Debug.LogWarning("Monster with empty name found in stageMonsterData");
                continue;
            }
            
            var routine = StartCoroutine(SpawnRoutine(monsterData));
            spawnRoutines[monsterData.monsterName] = routine;
        }
    }

    /// <summary>
    /// 특정 몬스터의 랜덤 스폰 시작
    /// </summary>
    /// <param name="monsterName">스폰할 몬스터의 이름</param>
    public void StartRandomSpawning(string monsterName)
    {
        if (string.IsNullOrEmpty(monsterName))
        {
            Debug.LogError("Cannot spawn monster with null or empty name");
            return;
        }

        if (stageMonsterData == null || stageMonsterData.monsters == null)
        {
            Debug.LogWarning($"Cannot spawn monster {monsterName}: No stageMonsterData available");
            return;
        }

        var monsterData = stageMonsterData.monsters.Find(x => x.monsterName == monsterName);
        if (monsterData.Equals(default(StageMonsterData.MonsterData)) || string.IsNullOrEmpty(monsterData.monsterName))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"MonsterData not found: {monsterName}");
#endif
            return;
        }

        // 이미 스폰 중인 몬스터라면 중단하고 새로 시작
        StopSpawningMonster(monsterName);
        
        var routine = StartCoroutine(SpawnRoutine(monsterData));
        spawnRoutines[monsterName] = routine;
    }

    /// <summary>
    /// 특정 몬스터의 스폰 중단
    /// </summary>
    /// <param name="monsterName">중단할 몬스터의 이름</param>
    public void StopSpawningMonster(string monsterName)
    {
        if (string.IsNullOrEmpty(monsterName))
        {
            Debug.LogWarning("Attempted to stop spawning with null or empty monster name");
            return;
        }

        if (spawnRoutines != null && spawnRoutines.TryGetValue(monsterName, out Coroutine routine))
        {
            if (routine != null)
                StopCoroutine(routine);
            
            spawnRoutines.Remove(monsterName);
        }
    }

    /// <summary>
    /// 모든 몬스터 스폰 중단
    /// </summary>
    public void StopAllSpawning()
    {
        if (spawnRoutines == null)
            return;
            
        foreach (var routine in spawnRoutines.Values)
        {
            if (routine != null)
                StopCoroutine(routine);
        }
        spawnRoutines.Clear();
    }

    /// <summary>
    /// 몬스터의 주기적 스폰을 담당하는 코루틴
    /// </summary>
    /// <param name="monsterData">스폰할 몬스터의 데이터</param>
    private IEnumerator SpawnRoutine(StageMonsterData.MonsterData monsterData)
    {
        if (monsterData.Equals(default(StageMonsterData.MonsterData)) || monsterData.spawnRate <= 0)
        {
            Debug.LogWarning($"Invalid monster data or spawn rate for: {(monsterData.Equals(default(StageMonsterData.MonsterData)) ? "null" : monsterData.monsterName)}");
            yield break;
        }

        while (true)
        {
            // 스폰 속도에 따른 대기 시간
            float waitTime = 1f / monsterData.spawnRate;
            yield return new WaitForSeconds(waitTime);
            
            try 
            {
                // 플레이어가 없거나 게임이 종료 상태면 스폰 중단
                if (GameManager.Instance == null || GameManager.Instance.player == null)
                {
                    Debug.LogWarning("Cannot spawn monster: Player or GameManager is null");
                    yield break;
                }
                
                var monster = SpawnMonsterAtRandomPosition(monsterData.monsterName);
                
                // 스폰 이벤트 발생
                if (monster != null)
                    OnMonsterSpawned?.Invoke(monsterData.monsterName, monster);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during monster spawn routine: {ex.Message}");
                yield break;
            }
        }
    }

    /// <summary>
    /// 지정된 위치에 특정 몬스터를 즉시 생성
    /// </summary>
    /// <param name="monsterName">생성할 몬스터의 이름</param>
    /// <param name="position">생성 위치</param>
    /// <returns>생성된 몬스터 게임오브젝트</returns>
    public GameObject SpawnMonster(string monsterName, Vector2 position)
    {
        if (string.IsNullOrEmpty(monsterName))
        {
            Debug.LogError("Cannot spawn monster with null or empty name");
            return null;
        }

        try
        {
            var monster = MonsterPoolManager.Instance.GetMonsterFromPool(monsterName);
            if (monster != null)
            {
                monster.transform.position = position;
                OnMonsterSpawned?.Invoke(monsterName, monster);
            }
            else
            {
                Debug.LogWarning($"Monster not found in pool: {monsterName}");
            }
            return monster;
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR
            Debug.LogError($"Error spawning monster {monsterName}: {ex.Message}");
#endif
            return null;
        }
    }

    /// <summary>
    /// 플레이어 주변의 랜덤 위치에 몬스터 생성
    /// </summary>
    /// <param name="monsterName">생성할 몬스터 이름</param>
    /// <returns>생성된 몬스터 게임오브젝트</returns>
    private GameObject SpawnMonsterAtRandomPosition(string monsterName)
    {
        if (GameManager.Instance == null || GameManager.Instance.player == null)
        {
            Debug.LogWarning("Cannot spawn monster: Player or GameManager is null");
            return null;
        }

        // 플레이어 주변 랜덤 위치 계산
        var spawnDir = UnityEngine.Random.insideUnitCircle.normalized;
        var spawnDistance = UnityEngine.Random.Range(MinimumSpawnRadius, MaximumSpawnRadius);
        var spawnPos = spawnDir * spawnDistance + (Vector2)GameManager.Instance.player.transform.position;

        return SpawnMonster(monsterName, spawnPos);
    }
}
