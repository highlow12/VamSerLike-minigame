using UnityEngine;

public class MonsterPatternTestUi : MonoBehaviour
{
    public void MonsterPatternGUI()
    {
        float width = 190;
        float height = 50;
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft
        };

        // 패턴 스폰 버튼
        if (GUILayout.Button("Spawn Next Pattern", buttonStyle, GUILayout.Width(width), GUILayout.Height(height)))
        {
            MonsterPatternManager.Instance.SpawnNextPattern();
        }

        // 웨이브 시작 버튼
        if (GUILayout.Button("Start New Wave", buttonStyle, GUILayout.Width(width), GUILayout.Height(height)))
        {
            MonsterPatternManager.Instance.StartNewWave();
        }

        // 웨이브 정보 표시
        string waveInfo = $"Game Time: {Time.time - MonsterPatternManager.Instance.gameStartTime:F1}s\n";
        waveInfo += $"Active Waves: {MonsterPatternManager.Instance.activeWaves.Count}\n";
        waveInfo += $"Current Wave: {MonsterPatternManager.Instance.currentWaveIndex}/{MonsterPatternManager.Instance.waveSpawnPattern?.wavePattern.Count}\n";

        if (MonsterPatternManager.Instance.activeWaves.Count > 0)
        {
            var lastWave = MonsterPatternManager.Instance.activeWaves[^1];
            var patterns = lastWave.wavePattern.wavePattern.patterns;
            waveInfo += $"Last Wave Pattern: {lastWave.currentPatternIndex}/{patterns.Count}\n";
            if (lastWave.currentPatternIndex < patterns.Count)
            {
                float nextPatternTime = lastWave.waveStartTime + patterns[lastWave.currentPatternIndex].startTime;
                waveInfo += $"Next Pattern Time: {nextPatternTime - Time.time:F1}s\n";
            }
            waveInfo += $"Monsters Alive: {lastWave.spawnedMonsters.Count}";
        }
        GUILayout.Label(waveInfo, labelStyle);

        // 다음 패턴 정보 표시
        var nextPattern = MonsterPatternManager.Instance.GetNextPattern();
        if (nextPattern != null)
        {
            string info = $"Next: {nextPattern?.patternType}\nCount: {nextPattern?.monsterCount}";
            GUILayout.Label(info, labelStyle);
        }
    }
}

