using UnityEngine;
using UnityEngine.Rendering.Universal;

public class globalLight : MonoBehaviour
{
    private Light2D lightComponent;
    
    // 디버깅을 위한 변수들
    public bool debugMode = false;
    public float defaultIntensity = 0.3f;

    void Awake()
    {
        lightComponent = GetComponent<Light2D>();
        if (lightComponent == null)
        {
            Debug.LogError("globalLight: Light2D 컴포넌트가 존재하지 않습니다!");
            return;
        }
        
        // 기본 설정
        if (debugMode)
        {
            lightComponent.intensity = defaultIntensity; // 디버그 모드에서는 기본 밝기 설정
            Debug.Log($"globalLight Awake: 기본 밝기 {defaultIntensity}로 설정");
        }
        else
        {
            // 기본적으로는 아주 낮은 밝기로 설정 (완전히 끄지 않음)
            lightComponent.intensity = 0.01f;
            Debug.Log("globalLight Awake: 초기 밝기 0.01로 설정");
        }
    }
    
    private void Start()
    {
        // 시작 시 라이트가 활성화되어 있는지 확인
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("globalLight: GameObject가 비활성화되어 있습니다!");
            gameObject.SetActive(true);
        }
        
        // GameManager에 globalLight 참조 자동 등록 (선택 사항)
        if (GameManager.Instance != null && GameManager.Instance.globalLight == null)
        {
            Debug.Log("globalLight: GameManager.globalLight 참조 자동 등록");
            GameManager.Instance.globalLight = lightComponent;
        }
    }

    // 외부에서 밝기를 조절할 수 있는 함수
    public void SetIntensity(float intensity)
    {
        if (lightComponent != null)
        {
            float prevIntensity = lightComponent.intensity;
            lightComponent.intensity = intensity;
            Debug.Log($"globalLight.SetIntensity: {prevIntensity} -> {intensity}");
        }
        else
        {
            Debug.LogError("globalLight.SetIntensity: lightComponent가 null입니다!");
        }
    }
    
    // 디버그용 함수 - 에디터에서 밝기를 쉽게 테스트할 수 있습니다
#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;
        
        if (lightComponent == null)
            lightComponent = GetComponent<Light2D>();
            
        if (debugMode && lightComponent != null)
            lightComponent.intensity = defaultIntensity;
    }
#endif
}
