using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct SpawnPatternData
{
    [Tooltip("패턴 타입")]
    public SpawnPatternType patternType;
    [Tooltip("시작 시간")]
    public float startTime;
    [Tooltip("지속 시간")]
    public float duration;
    [Tooltip("몬스터 수")]
    public int monsterCount;
    [Tooltip("반지름")]
    public float radius;
    [Tooltip("랜덤 위치")]
    public float wiggle;
    [Tooltip("회전 각도")]
    public float angle;
    [Tooltip("몬스터 이름름")]
    public string monsterName;
}

[CreateAssetMenu(fileName = "StageSpawnPattern", menuName = "Scriptable Objects/StageSpawnPattern")]
public class StageSpawnPattern : ScriptableObject
{
    [Tooltip("데이터를 불러올 JSON 파일")]
    public TextAsset jsonFile;
    [Tooltip("스폰 패턴 데이터를 저장하는 리스트")]
    public List<SpawnPatternData> patterns = new List<SpawnPatternData>();

    // 패턴을 시작 시간 순으로 정렬하는 메서드
    public void SortPatternsByStartTime()
    {
        patterns.Sort((a, b) => a.startTime.CompareTo(b.startTime));
    }
}
