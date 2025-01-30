using System.Collections.Generic;
using UnityEngine;

public class MonsterPatternManager : Singleton<MonsterPatternManager>
{
    int currentPatternIndex = 0;
    public StageSpawnPattern stageSpawnPattern;//{get; /*private*/ set;}
    public void SetStageSpawnPattern(StageSpawnPattern stageSpawnPattern)
    {
        stageSpawnPattern.SortPatternsByStartTime();
        this.stageSpawnPattern = stageSpawnPattern;
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

        // RandomFormation일 경우 StartRandomSpawning()을 호출합니다.
        if(formation.GetType() == typeof(RandomFormation)) 
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
            }
        }
    }

    private void OnGUI()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 화면 우측 상단에 버튼 배치
        if (GUI.Button(new Rect(Screen.width - 200, 10, 190, 50), "Spawn Next Pattern"))
        {
            //Debug.Log("Spawn Next Pattern");
            SpawnNextPattern();
        }

        // 다음 패턴 정보 표시
        var nextPattern = GetNextPattern();
        if (nextPattern != null)
        {
            string info = $"Next: {nextPattern?.patternType}\nCount: {nextPattern?.monsterCount}";
            GUI.Label(new Rect(Screen.width - 200, 70, 190, 50), info);
        }
        #endif
    }
}