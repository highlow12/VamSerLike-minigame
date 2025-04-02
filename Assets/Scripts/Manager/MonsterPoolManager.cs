using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// MonsterPoolManager는 몬스터 객체 풀을 관리하는 싱글톤 클래스입니다.
/// 각 몬스터 타입별로 풀을 유지하고 몬스터 생성 및 재사용을 담당합니다.
/// </summary>
public class MonsterPoolManager : Singleton<MonsterPoolManager>
{
    [HideInInspector]
    public StageMonsterData stageMonsterData;
    
    // 몬스터 풀 컬렉션
    private Dictionary<string, Queue<GameObject>> monsterPools = new Dictionary<string, Queue<GameObject>>();
    
    // 몬스터별 활성 개체 수 추적
    private Dictionary<string, int> activeMonsterCount = new Dictionary<string, int>();
    
    // 몬스터별 최대 풀 크기 설정 (기본값: 50)
    private const int DEFAULT_MAX_POOL_SIZE = 50;
    private Dictionary<string, int> monsterPoolSizes = new Dictionary<string, int>();

    // 몬스터가 풀로 반환될 때 이벤트
    public event Action<string, GameObject> OnMonsterReturnedToPool;

    /// <summary>
    /// 새로운 스테이지 몬스터 데이터로 몬스터 풀을 업데이트합니다.
    /// </summary>
    /// <param name="newData">새 스테이지 몬스터 데이터</param>
    public void UpdateStageMonsterData(StageMonsterData newData)
    {
        if (newData == null)
        {
            Debug.LogError("Cannot update monster pool with null data");
            return;
        }

        stageMonsterData = newData;
        InitializePools();
    }

    /// <summary>
    /// 모든 몬스터 풀을 초기화합니다.
    /// </summary>
    private void InitializePools()
    {
        if (stageMonsterData == null || stageMonsterData.monsters == null || stageMonsterData.monsters.Count == 0)
        {
            Debug.LogWarning("No monster data available for pool initialization");
            return;
        }

        // 기존 풀의 모든 몬스터 제거
        CleanupPools();
        
        monsterPools.Clear();
        activeMonsterCount.Clear();
        monsterPoolSizes.Clear();

        // 각 몬스터 타입별로 풀 생성
        foreach (var monster in stageMonsterData.monsters)
        {
            if (string.IsNullOrEmpty(monster.monsterName) || monster.monsterPrefab == null)
            {
                Debug.LogWarning("Invalid monster data found (missing name or prefab)");
                continue;
            }
            
            // 풀 크기 계산 (스폰율에 비례)
            int initialPoolSize = Mathf.CeilToInt(monster.spawnRate * 2);
            initialPoolSize = Mathf.Clamp(initialPoolSize, 5, DEFAULT_MAX_POOL_SIZE);
            
            Queue<GameObject> pool = new Queue<GameObject>(initialPoolSize);
            
            // 풀에 몬스터 초기 생성
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject newMonster = InstantiateMonster(monster.monsterPrefab, monster.monsterName);
                if (newMonster != null)
                {
                    newMonster.SetActive(false);
                    pool.Enqueue(newMonster);
                }
            }
            
            monsterPools[monster.monsterName] = pool;
            activeMonsterCount[monster.monsterName] = 0;
            monsterPoolSizes[monster.monsterName] = DEFAULT_MAX_POOL_SIZE;
            
            Debug.Log($"Pool initialized for {monster.monsterName} with {pool.Count} objects");
        }
    }

    /// <summary>
    /// 풀에서 몬스터를 가져옵니다. 사용 가능한 객체가 없으면 새로 생성합니다.
    /// </summary>
    /// <param name="monsterName">가져올 몬스터 이름</param>
    /// <returns>활성화된 몬스터 GameObject</returns>
    public GameObject GetMonsterFromPool(string monsterName)
    {
        if (string.IsNullOrEmpty(monsterName))
        {
            Debug.LogError("Cannot get monster with null or empty name");
            return null;
        }

        if (!monsterPools.ContainsKey(monsterName))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Pool for monster '{monsterName}' doesn't exist, creating new pool");
#endif
            CreatePoolForMonster(monsterName);
            
            // 풀 생성에 실패했다면 null 반환
            if (!monsterPools.ContainsKey(monsterName))
                return null;
        }

        Queue<GameObject> pool = monsterPools[monsterName];
        GameObject monster = null;
        
        // 풀에 사용 가능한 객체가 있는지 확인
        if (pool.Count == 0)
        {
            // 풀이 비었으면 새 몬스터 생성
            GameObject prefab = GetPrefabByName(monsterName);
            if (prefab == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"Cannot create monster: Prefab for '{monsterName}' not found");
#endif
                return null;
            }
            
            monster = InstantiateMonster(prefab, monsterName);
        }
        else
        {
            // 풀에서 몬스터 가져오기
            monster = pool.Dequeue();
        }

        if (monster != null)
        {
            monster.SetActive(true);
            
            // 활성 몬스터 수 추적
            if (activeMonsterCount.ContainsKey(monsterName))
                activeMonsterCount[monsterName]++;
            else
                activeMonsterCount[monsterName] = 1;
        }
        
        return monster;
    }

    /// <summary>
    /// 몬스터를 비활성화하고 풀로 반환합니다.
    /// </summary>
    /// <param name="monster">반환할 몬스터 GameObject</param>
    /// <param name="monsterName">몬스터 이름</param>
    public void ReturnMonsterToPool(GameObject monster, string monsterName)
    {
        if (monster == null || string.IsNullOrEmpty(monsterName))
        {
            Debug.LogWarning($"Cannot return invalid monster to pool: {monsterName}");
            return;
        }

        if (!monsterPools.ContainsKey(monsterName))
        {
            Debug.LogWarning($"Pool doesn't exist for monster: {monsterName}, creating new pool");
            CreatePoolForMonster(monsterName);
        }

        // 몬스터 상태 초기화
        ResetMonsterState(monster);
        monster.SetActive(false);
        
        // 풀 최대 크기 제한 확인
        if (monsterPools[monsterName].Count < GetMaxPoolSize(monsterName))
        {
            monsterPools[monsterName].Enqueue(monster);
        }
        else
        {
            // 풀이 최대 크기에 도달했으면 객체 파괴
            Debug.Log($"Pool for {monsterName} reached maximum size, destroying excess object");
            Destroy(monster);
        }
        
        // 활성 몬스터 수 감소
        if (activeMonsterCount.ContainsKey(monsterName) && activeMonsterCount[monsterName] > 0)
            activeMonsterCount[monsterName]--;
            
        // 이벤트 발생
        OnMonsterReturnedToPool?.Invoke(monsterName, monster);
    }

    /// <summary>
    /// 몬스터 이름으로 프리팹을 찾습니다.
    /// </summary>
    /// <param name="monsterName">몬스터 이름</param>
    /// <returns>몬스터 프리팹</returns>
    private GameObject GetPrefabByName(string monsterName)
    {
        if (stageMonsterData == null || stageMonsterData.monsters == null)
            return null;
            
        var monsterData = stageMonsterData.monsters.Find(m => m.monsterName == monsterName);
        if (monsterData.Equals(default(StageMonsterData.MonsterData)) || monsterData.monsterPrefab == null)
        {
            Debug.LogWarning($"Monster '{monsterName}' prefab not found!");
            return null;
        }
        return monsterData.monsterPrefab;
    }

    /// <summary>
    /// 특정 몬스터 타입의 풀 최대 크기를 설정합니다.
    /// </summary>
    /// <param name="monsterName">몬스터 이름</param>
    /// <param name="maxSize">최대 풀 크기</param>
    public void SetMaxPoolSize(string monsterName, int maxSize)
    {
        if (string.IsNullOrEmpty(monsterName) || maxSize < 1)
            return;
            
        monsterPoolSizes[monsterName] = maxSize;
    }

    /// <summary>
    /// 특정 몬스터 타입의 풀 최대 크기를 가져옵니다.
    /// </summary>
    /// <param name="monsterName">몬스터 이름</param>
    /// <returns>최대 풀 크기</returns>
    private int GetMaxPoolSize(string monsterName)
    {
        if (monsterPoolSizes.TryGetValue(monsterName, out int size))
            return size;
        return DEFAULT_MAX_POOL_SIZE;
    }

    /// <summary>
    /// 몬스터를 생성하고 식별자를 추가합니다.
    /// </summary>
    /// <param name="prefab">몬스터 프리팹</param>
    /// <param name="monsterName">몬스터 이름</param>
    /// <returns>생성된 몬스터 GameObject</returns>
    private GameObject InstantiateMonster(GameObject prefab, string monsterName)
    {
        if (prefab == null)
            return null;
            
        try
        {
            var obj = Instantiate(prefab);
            
            // 몬스터 식별자 추가
            if (obj.TryGetComponent(out MonsterIdentify identify))
            {
                identify.monsterName = monsterName;
            }
            else
            {
                obj.AddComponent<MonsterIdentify>().monsterName = monsterName;
            }
            
            return obj;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error instantiating monster {monsterName}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 특정 몬스터 타입을 위한 새 풀을 생성합니다.
    /// </summary>
    /// <param name="monsterName">몬스터 이름</param>
    private void CreatePoolForMonster(string monsterName)
    {
        if (string.IsNullOrEmpty(monsterName))
            return;
            
        var prefab = GetPrefabByName(monsterName);
        if (prefab == null)
        {
            Debug.LogError($"Cannot create pool for {monsterName}: Prefab not found");
            return;
        }
        
        // 기본 풀 크기로 풀 생성
        Queue<GameObject> pool = new Queue<GameObject>();
        int initialSize = 5; // 기본 초기 풀 크기
        
        for (int i = 0; i < initialSize; i++)
        {
            GameObject monster = InstantiateMonster(prefab, monsterName);
            if (monster != null)
            {
                monster.SetActive(false);
                pool.Enqueue(monster);
            }
        }
        
        monsterPools[monsterName] = pool;
        activeMonsterCount[monsterName] = 0;
        monsterPoolSizes[monsterName] = DEFAULT_MAX_POOL_SIZE;
        
#if UNITY_EDITOR
        Debug.Log($"Created new pool for {monsterName} with {initialSize} objects");
#endif
    }

    /// <summary>
    /// 몬스터 상태를 초기화합니다.
    /// </summary>
    /// <param name="monster">초기화할 몬스터</param>
    private void ResetMonsterState(GameObject monster)
    {
        // 몬스터 컴포넌트에 따라 필요한 상태 초기화 로직 추가
        // 예: 체력, 상태 등 초기화
        
        // 몬스터 체력 초기화 예시
        if (monster.TryGetComponent(out Monster monsterComponent))
        {
            // ResetState 메서드가 없으므로 기본 상태 초기화 로직을 추가합니다.
            monsterComponent.ResetHealth(); // 예: 체력을 초기화하는 메서드
            monsterComponent.ResetPosition(); // 예: 위치를 초기화하는 메서드
        }
    }

    /// <summary>
    /// 모든 풀을 정리하고 몬스터 객체를 파괴합니다.
    /// </summary>
    private void CleanupPools()
    {
        foreach (var pool in monsterPools.Values)
        {
            foreach (var monster in pool)
            {
                if (monster != null)
                    Destroy(monster);
            }
        }
    }

    /// <summary>
    /// 현재 활성화된 특정 몬스터의 수를 가져옵니다.
    /// </summary>
    /// <param name="monsterName">몬스터 이름</param>
    /// <returns>활성화된 몬스터 수</returns>
    public int GetActiveMonsterCount(string monsterName)
    {
        if (string.IsNullOrEmpty(monsterName) || !activeMonsterCount.ContainsKey(monsterName))
            return 0;
            
        return activeMonsterCount[monsterName];
    }

    /// <summary>
    /// 현재 활성화된 모든 몬스터의 수를 가져옵니다.
    /// </summary>
    /// <returns>총 활성화된 몬스터 수</returns>
    public int GetTotalActiveMonsterCount()
    {
        int total = 0;
        foreach (var count in activeMonsterCount.Values)
        {
            total += count;
        }
        return total;
    }

    private void OnDestroy()
    {
        CleanupPools();
    }
}
