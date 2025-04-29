using System;
using System.Collections.Generic;

/// <summary>
/// 게임에서 저장해야 하는 모든 데이터를 담는 컨테이너를 나타냅니다.
/// </summary>
[Serializable] // 이 속성은 직렬화(예: BinaryFormatter 또는 JsonUtility 사용)에 중요합니다.
public class SaveData
{
    /// <summary>
    /// 저장이 생성된 타임스탬프입니다.
    /// </summary>
    public DateTime lastSaved;

    /// <summary>
    /// 각 저장 가능한 객체의 상태 데이터를 보유하는 딕셔너리입니다.
    /// 키는 ISaveable.GetUniqueIdentifier()에서 반환된 고유 식별자입니다.
    /// 값은 ISaveable.CaptureState()에서 반환된 상태 객체입니다.
    /// </summary>
    public Dictionary<string, object> objectStates;

    // 선택 사항: 필요한 경우 여기에 다른 전역 저장 데이터를 추가합니다.
    // public string gameVersion;
    // public int currentLevelIndex;
    // public float totalPlayTime;

    /// <summary>
    /// 생성자는 딕셔너리를 초기화합니다.
    /// </summary>
    public SaveData()
    {
        objectStates = new Dictionary<string, object>();
        lastSaved = DateTime.MinValue; // 기본값으로 초기화합니다.
    }
}
