using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverAlpha : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 마우스가 올라갔을 때의 알파값 (완전 불투명 = 1)
    [Range(0f, 1f)]
    public float alphaOnHover = 1.0f;
    
    // 마우스가 벗어났을 때의 알파값 (완전 투명 = 0)
    [Range(0f, 1f)]
    public float alphaOnExit = 0.2f;
    
    // 알파값 변경 속도 (값이 클수록 더 빠르게 변함)
    [Range(1f, 10f)]
    public float fadeSpeed = 5.0f;
    
    // 이미지 컴포넌트 참조
    private Image buttonImage;
    
    // 버튼 텍스트가 있는 경우를 위한 참조
    private Text buttonText;
    private TMPro.TextMeshProUGUI buttonTMPText;
    
    // 현재 알파값
    private float currentAlpha;
    
    // 목표 알파값
    private float targetAlpha;
    
    private void Awake()
    {
        // 버튼 이미지 컴포넌트 가져오기
        buttonImage = GetComponent<Image>();
        
        // 버튼에 텍스트 컴포넌트가 있는지 확인
        buttonText = GetComponentInChildren<Text>();
        buttonTMPText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        
        if (buttonImage == null)
        {
            Debug.LogError("ButtonHoverAlpha는 Image 컴포넌트가 있는 오브젝트에 추가해야 합니다!");
            return;
        }
        
        // Raycast Target이 켜져있는지 확인
        if (!buttonImage.raycastTarget)
        {
            Debug.LogWarning("Button의 Image 컴포넌트의 Raycast Target이 꺼져 있습니다. 마우스 이벤트가 감지되지 않을 수 있습니다.");
            buttonImage.raycastTarget = true;
        }
        
        // 초기 알파값 설정
        currentAlpha = alphaOnExit;
        targetAlpha = alphaOnExit;
        SetAlpha(currentAlpha);
    }
    
    // 프레임마다 호출
    private void Update()
    {
        // 현재 알파값이 목표 알파값과 다르면 서서히 변경
        if (!Mathf.Approximately(currentAlpha, targetAlpha))
        {
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
            SetAlpha(currentAlpha);
        }
    }
    
    // 알파값 설정 함수
    private void SetAlpha(float alpha)
    {
        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = alpha;
            buttonImage.color = color;
        }
        
        // 버튼 텍스트가 있다면 텍스트의 알파값도 변경
        if (buttonText != null)
        {
            Color textColor = buttonText.color;
            textColor.a = alpha;
            buttonText.color = textColor;
        }
        
        // TextMeshPro 텍스트가 있다면 알파값 변경
        if (buttonTMPText != null)
        {
            Color tmpColor = buttonTMPText.color;
            tmpColor.a = alpha;
            buttonTMPText.color = tmpColor;
        }
    }
    
    // 마우스가 버튼 위에 올라갔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetAlpha = alphaOnHover;
    }
    
    // 마우스가 버튼에서 벗어났을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        targetAlpha = alphaOnExit;
    }
}
