using System.Collections.Generic;
using UnityEngine;

public class MonsterPatternManager : Singleton<MonsterPatternManager>
{
    // Public properties for monitoring and UI access
    public float gameStartTime;
    public List<WaveInstance> activeWaves = new();
    public int currentPatternIndex = 0;
    public int currentWaveIndex = 0;
    public StageSpawnPattern stageSpawnPattern;
    public WavePatternData waveSpawnPattern;

    // Wave management class with clearer responsibilities
    public class WaveInstance
    {
        public WavePattern wavePattern;
        public int currentPatternIndex;
        public float waveStartTime;
        public List<GameObject> spawnedMonsters;

        public WaveInstance(WavePattern wavePattern, float startTime)
        {
            this.wavePattern = wavePattern;
            currentPatternIndex = 0;
            waveStartTime = startTime;
            spawnedMonsters = new List<GameObject>();
        }

        // Check if this wave has remaining patterns to spawn
        public bool HasRemainingPatterns() => currentPatternIndex < wavePattern.wavePattern.patterns.Count;

        // Get next pattern's scheduled start time
        public float GetNextPatternStartTime() => 
            waveStartTime + wavePattern.wavePattern.patterns[currentPatternIndex].startTime;

        // Check if all patterns have been executed and monsters are cleared
        public bool IsCompleted() => !HasRemainingPatterns() && spawnedMonsters.Count == 0;
    }

    // Pattern management class with clearer responsibilities
    public class PatternInstance
    {
        public SpawnPatternData patternData;
        public float spawnTime;
        public List<GameObject> spawnedMonsters;

        public PatternInstance(SpawnPatternData patternData, float spawnTime)
        {
            this.patternData = patternData;
            this.spawnTime = spawnTime;
            spawnedMonsters = new List<GameObject>();
        }

        // Check if pattern duration has expired
        public bool HasExpired(float currentTime) => 
            patternData.duration > 0f && currentTime >= spawnTime + patternData.duration;

        // Handle monster cleanup for this pattern
        public void CleanupMonsters()
        {
            if (patternData.patternType == SpawnPatternType.Random)
            {
                MonsterSpawner.Instance.StopSpawningMonster(patternData.monsterName);
            }
            else
            {
                foreach (var monster in spawnedMonsters)
                {
                    if (monster) 
                        MonsterPoolManager.Instance.ReturnMonsterToPool(monster, monster.GetComponent<MonsterIdentify>().monsterName);
                }
            }
        }
    }

    // Private fields
    private List<PatternInstance> activePatterns = new();
    private float nextWaveSpawnTime = 0f;

    void Start()
    {
        gameStartTime = Time.time;
    }

    public void SetStageSpawnPattern(StageSpawnPattern stageSpawnPattern)
    {
        stageSpawnPattern.SortPatternsByStartTime();
        this.stageSpawnPattern = stageSpawnPattern;
    }

    public void SetWaveSpawnPattern(WavePatternData waveSpawnPattern)
    {
        this.waveSpawnPattern = waveSpawnPattern;
        currentWaveIndex = 0;
        
        // Calculate absolute time for first wave
        if (waveSpawnPattern != null && waveSpawnPattern.wavePattern.Count > 0)
            nextWaveSpawnTime = gameStartTime + waveSpawnPattern.wavePattern[0].waveStartTime;
    }

    public SpawnPatternData? GetNextPattern()
    {
        if (stageSpawnPattern == null || stageSpawnPattern.patterns.Count == 0)
            return null;
        
        return currentPatternIndex < stageSpawnPattern.patterns.Count ? 
            stageSpawnPattern.patterns[currentPatternIndex] : null;
    }

    // 스폰 패턴에 따라 몬스터들을 스폰하고 적절한 컬렉션에 추가
    private PatternInstance SpawnMonstersWithPattern(SpawnPatternData pattern, WaveInstance currentWave = null)
    {
        var patternInstance = new PatternInstance(pattern, Time.time);
        var formation = SpawnFormation.CreateFormation(pattern, pattern.wiggle);
        Vector2 playerPos = GameManager.Instance.player.transform.position;
        
        if (pattern.patternType == SpawnPatternType.Random)
        {
            MonsterSpawner.Instance.StartRandomSpawning(pattern.monsterName);
        }
        else
        {
            Vector2[] spawnPositions = formation.GetSpawnPositions(playerPos);
            if (spawnPositions != null && spawnPositions.Length > 0)
            {
                foreach (var position in spawnPositions)
                {
                    var monster = MonsterSpawner.Instance.SpawnMonster(pattern.monsterName, position);
                    if (monster != null)
                    {
                        patternInstance.spawnedMonsters.Add(monster);
                        
                        // Add monster to wave if applicable
                        currentWave?.spawnedMonsters.Add(monster);
                    }
                }
            }
        }
        
        activePatterns.Add(patternInstance);
        return patternInstance;
    }

    // Spawn the next pattern from the stage spawn pattern
    public void SpawnNextPattern()
    {
        if (stageSpawnPattern == null || currentPatternIndex >= stageSpawnPattern.patterns.Count)
            return;

        var pattern = stageSpawnPattern.patterns[currentPatternIndex];
        currentPatternIndex++;
        
        SpawnMonstersWithPattern(pattern, activeWaves.Count > 0 ? activeWaves[^1] : null);
    }

    // Process wave pattern spawning without modifying global state
    private void ProcessWavePattern(WaveInstance wave)
    {
        if (wave.HasRemainingPatterns())
        {
            var pattern = wave.wavePattern.wavePattern.patterns[wave.currentPatternIndex];
            SpawnMonstersWithPattern(pattern, wave);
            wave.currentPatternIndex++;
        }
    }

    // Start a new wave from the wave pattern data
    public void StartNewWave()
    {
        if (waveSpawnPattern == null || currentWaveIndex >= waveSpawnPattern.wavePattern.Count)
            return;

        WaveInstance newWave = new WaveInstance(waveSpawnPattern.wavePattern[currentWaveIndex], Time.time);
        activeWaves.Add(newWave);
        currentWaveIndex++;
    }

    // Update wave management - removes dead monsters and processes finished waves
    private void UpdateWaveMonsters()
    {
        for (int i = activeWaves.Count - 1; i >= 0; i--)
        {
            WaveInstance wave = activeWaves[i];
            CleanupDeadMonsters(wave);
            
            // Check if this wave has completed
            if (wave.IsCompleted())
            {
                activeWaves.RemoveAt(i);
            }
        }
    }

    // Clean up monsters that have been destroyed
    private void CleanupDeadMonsters(WaveInstance wave)
    {
        for (int i = wave.spawnedMonsters.Count - 1; i >= 0; i--)
        {
            var monster = wave.spawnedMonsters[i];
            if (monster == null || !monster.activeInHierarchy)
            {
                wave.spawnedMonsters.RemoveAt(i);
            }
        }
    }

    // Check if it's time to spawn the next pattern in a wave
    private void CheckWavePatternSpawns()
    {
        foreach (var wave in activeWaves)
        {
            if (wave.HasRemainingPatterns() && Time.time >= wave.GetNextPatternStartTime())
            {
                ProcessWavePattern(wave);
            }
        }
    }

    // Check if it's time to start a new wave
    private void CheckNewWaveStart()
    {
        if (waveSpawnPattern != null && 
            currentWaveIndex < waveSpawnPattern.wavePattern.Count && 
            activeWaves.Count == 0 && 
            Time.time >= gameStartTime + waveSpawnPattern.wavePattern[currentWaveIndex].waveStartTime)
        {
            StartNewWave();
        }
    }

    // Clean up patterns that have exceeded their duration
    private void CleanupExpiredPatterns()
    {
        for (int i = activePatterns.Count - 1; i >= 0; i--)
        {
            var pattern = activePatterns[i];
            if (pattern.HasExpired(Time.time))
            {
                pattern.CleanupMonsters();
                activePatterns.RemoveAt(i);
            }
        }
    }

    private void Update()
    {
        // Main update sequence
        UpdateWaveMonsters();
        CheckWavePatternSpawns();
        CheckNewWaveStart();
        CleanupExpiredPatterns();
    }
}