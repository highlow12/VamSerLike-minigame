using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WavePattern
{
    public StageSpawnPattern wavePattern;
    public int waveStartTime;
}

[CreateAssetMenu(fileName = "WavePatternData", menuName = "Scriptable Objects/WavePatternData")]
public class WavePatternData : ScriptableObject
{
    [Tooltip("웨이브 패턴 데이터를 저장하는 리스트")]
    public List<WavePattern> wavePattern = new List<WavePattern>();
}

