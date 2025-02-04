using System.Collections.Generic;
using UnityEngine;

public class MonsterPatternManager : Singleton<MonsterPatternManager>
{
    int currentPatternIndex = 0;
    int currentWaveIndex = 0;  // 현재 실행 중인 웨이브 인덱스
    public StageSpawnPattern stageSpawnPattern;
    public WavePatternData waveSpawnPattern;
    float gameStartTime;  // 게임 시작 시간 저장용 변수 추가

    // 웨이브 관리용 내부 클래스 (수정됨)
    class WaveInstance
    {
        public WavePattern wavePattern; // WavePattern을 저장
        public int currentPatternIndex;
        public float waveStartTime;  // 웨이브가 시작된 실제 시간
        public List<GameObject> spawnedMonsters; // 추가: 웨이브에서 생성된 몬스터들

        public WaveInstance(WavePattern wavePattern, float startTime)
        {
            this.wavePattern = wavePattern;
            currentPatternIndex = 0;
            waveStartTime = startTime;
            spawnedMonsters = new List<GameObject>();
        }
    }

    class PatternInstance
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
    }

    List<WaveInstance> activeWaves = new();
    List<PatternInstance> activePatterns = new();
    float nextWaveSpawnTime = 0f;

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
        // 게임 시작부터 첫 웨이브 시작까지의 절대 시간 설정
        nextWaveSpawnTime = gameStartTime + waveSpawnPattern.wavePattern[0].waveStartTime;
    }
    public SpawnPatternData? GetNextPattern()
    {
        if (stageSpawnPattern == null || stageSpawnPattern.patterns.Count == 0)
            return null;
        return stageSpawnPattern.patterns[currentPatternIndex];
    }
    public void SpawnNextPattern() // SpawnNextPattern()은 다음 패턴을 스폰합니다. 게임 매니저에서 호출합니다.
    {
        if (stageSpawnPattern == null || stageSpawnPattern.patterns.Count == 0 || stageSpawnPattern.patterns.Count <= currentPatternIndex)
            return;

        var pattern = stageSpawnPattern.patterns[currentPatternIndex];
        currentPatternIndex++;
        //stageSpawnPattern.patterns.RemoveAt(0);

        SpawnFormation formation = pattern.patternType switch
        {
            SpawnPatternType.Circle => new CircleFormation(pattern, pattern.wiggle),
            SpawnPatternType.Square => new SquareFormation(pattern, pattern.wiggle),
            SpawnPatternType.Line => new LineFormation(pattern, pattern.wiggle),
            SpawnPatternType.Triangle => new TriangleFormation(pattern, pattern.wiggle),
            SpawnPatternType.Random => new RandomFormation(pattern, pattern.wiggle),
            _ => throw new System.NotImplementedException("Not implemented pattern type")
        };

        Vector2 playerPos = GameManager.Instance.player.transform.position;
        Vector2[] spawnPositions = formation.GetSpawnPositions(playerPos);

        var patternInstance = new PatternInstance(pattern, Time.time);

        // RandomFormation일 경우 StartRandomSpawning()을 호출합니다.
        if (formation.GetType() == typeof(RandomFormation))
        {
            MonsterSpawner.Instance.StartRandomSpawning(pattern.monsterName);
        }
        else // 그 외의 경우 몬스터를 직접 생성합니다.
        {
            foreach (var position in spawnPositions)
            {
                var monster = MonsterSpawner.Instance.SpawnMonster(
                    pattern.monsterName,
                    position
                );
                // 현재 활성화된 웨이브가 있다면 생성된 몬스터를 리스트에 추가
                if (activeWaves.Count > 0)
                {
                    activeWaves[^1].spawnedMonsters.Add(monster);
                }
                patternInstance.spawnedMonsters.Add(monster);
            }
        }
        activePatterns.Add(patternInstance);
    }

    // 웨이브 전용 SpawnNextPattern 호출 (수정됨)
    void ProcessWave(WaveInstance wave)
    {
        // 전역 값 임시 저장
        var origStage = stageSpawnPattern;
        var origIndex = currentPatternIndex;

        stageSpawnPattern = wave.wavePattern.wavePattern; // WavePattern 내부의 StageSpawnPattern 사용
        currentPatternIndex = wave.currentPatternIndex;
        SpawnNextPattern();
        wave.currentPatternIndex = currentPatternIndex;

        // 전역 값 복원
        stageSpawnPattern = origStage;
        currentPatternIndex = origIndex;
    }

    // 새로운 웨이브 시작 : WavePatternData의 내부 WavePattern 이용 (수정됨)
    void StartNewWave()
    {
        if (waveSpawnPattern == null || currentWaveIndex >= waveSpawnPattern.wavePattern.Count)
            return;
            
        WaveInstance newWave = new WaveInstance(waveSpawnPattern.wavePattern[currentWaveIndex], Time.time);
        activeWaves.Add(newWave);
        currentWaveIndex++;
    }

    // Update에서 웨이브 진행 및 신규 웨이브 시작 처리 (수정됨)
    private void Update()
    {
        // 기존 활성 웨이브 처리 (먼저 체크하여 웨이브 종료 여부 확인)
        for (int i = activeWaves.Count - 1; i >= 0; i--)
        {
            WaveInstance wave = activeWaves[i];
            var patterns = wave.wavePattern.wavePattern.patterns;
            
            // 패턴은 시간에 따라 진행
            if (wave.currentPatternIndex < patterns.Count)
            {
                float patternStartTime = wave.waveStartTime + patterns[wave.currentPatternIndex].startTime;
                if (Time.time >= patternStartTime)
                {
                    ProcessWave(wave);
                }
            }

            // 웨이브 종료 조건: 모든 패턴이 실행되고 AND 모든 몬스터가 죽었을 때
            if (wave.currentPatternIndex >= patterns.Count && AllMonstersDead(wave))
            {
                activeWaves.RemoveAt(i);
            }
        }

        // 다음 웨이브 시작 조건 체크
        if (waveSpawnPattern != null && currentWaveIndex < waveSpawnPattern.wavePattern.Count)
        {
            float currentWaveStartTime = gameStartTime + waveSpawnPattern.wavePattern[currentWaveIndex].waveStartTime;
            bool timeToStart = Time.time >= currentWaveStartTime;
            bool previousWaveFinished = activeWaves.Count == 0;

            // 시작 시간이 되었고 이전 웨이브가 끝났다면 새 웨이브 시작
            if (timeToStart && previousWaveFinished)
            {
                StartNewWave();
            }
        }

        // Remove patterns after duration
        for (int i = activePatterns.Count - 1; i >= 0; i--)
        {
            var pat = activePatterns[i];
            // duration이 0 이하라면 삭제하지 않음
            if (pat.patternData.duration <= 0f) continue;

            if (Time.time >= pat.spawnTime + pat.patternData.duration)
            {
                if (pat.patternData.patternType == SpawnPatternType.Random)
                {
                    MonsterSpawner.Instance.StopSpawningMonster(pat.patternData.monsterName);
                }
                else
                {
                    foreach (var monster in pat.spawnedMonsters)
                    {
                        if (monster) MonsterPoolManager.Instance.ReturnMonsterToPool(monster, monster.GetComponent<MonsterIdentify>().monsterName);
                    }
                }
                activePatterns.RemoveAt(i);
            }
        }
    }

    // 해당 웨이브의 몬스터들이 모두 죽었는지 확인하는 함수 (구현 필요)
    bool AllMonstersDead(WaveInstance wave)
    {
        // 웨이브에 등록된 몬스터가 없으면 true 반환
        if (wave.spawnedMonsters.Count == 0)
            return true;

        // 모든 몬스터를 순회하면서 상태 확인
        for (int i = wave.spawnedMonsters.Count - 1; i >= 0; i--)
        {
            var monster = wave.spawnedMonsters[i];

            // 몬스터가 없거나 비활성화되었다면 리스트에서 제거
            if (monster == null || !monster.activeInHierarchy)
            {
                wave.spawnedMonsters.RemoveAt(i);
                continue;
            }
        }

        // 남은 몬스터가 없으면 true, 있으면 false 반환
        return wave.spawnedMonsters.Count == 0;
    }

    private void OnGUI()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        float width = 190 * 3;
        float height = 50 * 3;
        float labelHeight = 100 * 3;

        // 모든 GUI 요소의 폰트 크기를 36으로 설정
        GUI.skin.button.fontSize = 36;
        GUI.skin.label.fontSize = 36;

        // 패턴 스폰 버튼 (크기 3배)
        if (GUI.Button(new Rect(Screen.width - width, 10, width, height), "Spawn Next Pattern"))
        {
            SpawnNextPattern();
        }

        // 웨이브 시작 버튼 (크기 3배)
        if (GUI.Button(new Rect(Screen.width - width, height + 30, width, height), "Start New Wave"))
        {
            StartNewWave();
        }

        // 웨이브 정보 표시 (크기 3배)
        GUI.skin.label.fontSize = 36; // 폰트 크기도 증가
        string waveInfo = $"Game Time: {Time.time - gameStartTime:F1}s\n";
        waveInfo += $"Active Waves: {activeWaves.Count}\n";
        waveInfo += $"Current Wave: {currentWaveIndex}/{waveSpawnPattern?.wavePattern.Count}\n";
        if (activeWaves.Count > 0)
        {
            var lastWave = activeWaves[^1];
            var patterns = lastWave.wavePattern.wavePattern.patterns;
            waveInfo += $"Last Wave Pattern: {lastWave.currentPatternIndex}/{patterns.Count}\n";
            if (lastWave.currentPatternIndex < patterns.Count)
            {
                float nextPatternTime = lastWave.waveStartTime + patterns[lastWave.currentPatternIndex].startTime;
                waveInfo += $"Next Pattern Time: {nextPatternTime - Time.time:F1}s\n";
            }
            waveInfo += $"Monsters Alive: {lastWave.spawnedMonsters.Count}";
        }
        GUI.Label(new Rect(Screen.width - width, (height * 2) + 60, width, labelHeight), waveInfo);

        // 다음 패턴 정보 표시 (크기 3배)
        var nextPattern = GetNextPattern();
        if (nextPattern != null)
        {
            string info = $"Next: {nextPattern?.patternType}\nCount: {nextPattern?.monsterCount}";
            GUI.Label(new Rect(Screen.width - width, (height * 3) + 90, width, height), info);
        }
#endif
    }
}