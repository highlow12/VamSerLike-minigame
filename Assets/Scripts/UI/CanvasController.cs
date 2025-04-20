using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    // 비활성화할 캔버스 참조
    [SerializeField] private GameObject targetCanvas;
    
    // 시작 버튼이 클릭되었을 때 호출될 메서드
    public void DisableCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Target Canvas is not assigned!");
        }
    }
}
