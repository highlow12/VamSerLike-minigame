using UnityEngine;
using System; // Guid 사용

// 에디터와 플레이 모드 모두에서 실행되도록 설정
// 씬 저장 시 ID가 함께 저장되도록 함
[ExecuteAlways]
public class UniqueID : MonoBehaviour
{
    // 직렬화하여 ID를 저장
    [SerializeField]
    private string id = "";

    // 외부에서 ID를 읽기 위한 프로퍼티
    public string ID => id;

// 에디터 전용 코드 시작 (#if UNITY_EDITOR)
#if UNITY_EDITOR
    // 컴포넌트가 로드되거나 인스펙터 값이 변경될 때 호출
    private void OnValidate()
    {
        EnsureID();
    }

    // Awake는 에디터 모드에서도 호출될 수 있음 (ExecuteAlways 때문에)
    private void Awake()
    {
        // 플레이 모드가 아닐 때 ID가 있는지 확인
        // 프리팹 편집 모드 등에서 ID 생성을 돕기 위함
        if (!Application.isPlaying)
        {
            EnsureID();
        }
    }

    // ID가 비어있는 경우 새로 생성하는 함수
    private void EnsureID()
    {
        // 프리팹 에셋 자체를 수정하는 것을 방지 (인스턴스에서만 생성/수정)
        // IsPartOfPrefabAsset: 이 오브젝트가 프로젝트 뷰의 프리팹 에셋 자체인지 확인
        bool isPrefabAsset = UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this);

        // ID가 비어있고, 프리팹 에셋이 아닌 경우에만 ID 생성
        if (string.IsNullOrEmpty(id) && !isPrefabAsset)
        {
            // System.Guid를 사용하여 전역적으로 고유한 ID 생성
            id = Guid.NewGuid().ToString();

            // 에디터에서 변경 사항을 저장하도록 표시
            UnityEditor.EditorUtility.SetDirty(this);
#if UNITY_EDITOR // Wrap Debug.Log
            Debug.Log($"Generated Unique ID for {gameObject.name}: {id}", this);
#endif

            // 만약 이 오브젝트가 씬에 있는 프리팹 인스턴스라면,
            // 변경 사항을 프리팹 에셋에 적용할지 여부를 결정할 수 있음 (선택 사항)
            // if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(this))
            // {
            //     UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            // }
        }
    }

    // 인스펙터에서 우클릭 메뉴로 ID를 강제로 재생성하는 기능 (디버깅용)
    [ContextMenu("Generate New ID")]
    private void GenerateNewIDManual()
    {
        bool isPrefabAsset = UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this);
        if (!isPrefabAsset) // 프리팹 에셋 자체에서는 실행 방지
        {
             id = Guid.NewGuid().ToString();
             UnityEditor.EditorUtility.SetDirty(this);
#if UNITY_EDITOR // Wrap Debug.Log
             Debug.Log($"Manually Generated New Unique ID for {gameObject.name}: {id}", this);
#endif
        }
        else
        {
#if UNITY_EDITOR // Wrap Debug.LogWarning
            Debug.LogWarning("Cannot manually generate new ID on a prefab asset directly. Modify an instance.", this);
#endif
        }
    }
#endif
// 에디터 전용 코드 끝 (#endif)
}
