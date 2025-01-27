using UnityEngine;

[CreateAssetMenu(fileName = "SpawnPatternData", menuName = "Scriptable Objects/SpawnPatternData")]
public class SpawnPatternData : ScriptableObject
{
    public string patternName;
    public SpawnPatternType patternType;
    public float startTime;
    public float duration;
    public int monsterCount;
    public float radius = 20f;
    public float angle = 0f;
    public string monsterType;
}
