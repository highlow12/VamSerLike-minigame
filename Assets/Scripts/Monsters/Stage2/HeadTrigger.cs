using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class HeadTrigger : MonoBehaviour
{
    private Teacher teacherScript; // Teacher 스크립트 참조

    void Start()
    {
        // 부모 오브젝트 또는 상위 계층에서 Teacher 스크립트를 찾습니다.
        // 필요에 따라 Teacher 스크립트를 찾는 방식을 수정할 수 있습니다. (예: 직접 할당)
        teacherScript = GetComponentInParent<Teacher>();

        if (teacherScript == null)
        {
            Debug.LogError("HeadTriggerHandler: Teacher 스크립트를 찾을 수 없습니다!", this);
        }

        // Collider2D가 트리거로 설정되어 있는지 확인합니다.
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("HeadTriggerHandler: Collider2D의 IsTrigger가 활성화되어 있지 않습니다. 트리거로 자동 설정합니다.", this);
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 오브젝트가 "Player" 태그를 가지고 있는지 확인합니다.
        if (other.CompareTag("Player"))
        {
            Debug.Log("HeadTriggerHandler: Player 감지됨!");
            // Teacher 스크립트의 메서드를 호출합니다.
            if (teacherScript != null)
            {
                teacherScript.OnHeadDetectedPlayer();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // 충돌 영역을 벗어난 오브젝트가 "Player" 태그를 가지고 있는지 확인합니다.
        if (other.CompareTag("Player"))
        {
            Debug.Log("HeadTriggerHandler: Player 감지 해제됨!");
            // Teacher 스크립트의 메서드를 호출합니다. (선택 사항)
            if (teacherScript != null)
            {
                teacherScript.OnHeadLostPlayer();
            }
        }
    }
}
