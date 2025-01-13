using UnityEngine;
using System.Collections.Generic;

// StageMonsterData는 씬이나 인스펙터에서 여러 몬스터 데이터를 관리하는 ScriptableObject입니다.
[CreateAssetMenu(fileName = "StageMonsterData", menuName = "Scriptable Objects/StageMonsterData")]
public class StageMonsterData : ScriptableObject
{
    [System.Serializable]
    // MonsterData 구조체에는 몬스터 이름, 프리팹, 스폰율 등이 정의되어 있습니다.
    public struct MonsterData
    {
        [Tooltip("몬스터 이름.")]
        public string monsterName;
        [Tooltip("몬스터 프리팹.")]
        public GameObject monsterPrefab;
        [Tooltip("몬스터의 초당 스폰 횟수.")]
        public float spawnRate;
    }

    // monsters 리스트에 여러 몬스터 정보를 추가해 사용할 수 있습니다.
    [Tooltip("몬스터 데이터를 저장하는 리스트.")]
    public List<MonsterData> monsters;
}
