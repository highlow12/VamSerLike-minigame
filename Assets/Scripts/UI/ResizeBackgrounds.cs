using UnityEngine;

public class ResizeBackgrounds : MonoBehaviour
{
    void Start()
    {
        // 배경 오브젝트들을 찾습니다
        Transform bgController = GameObject.Find("BG controller").transform;
        RectTransform main = bgController.Find("main").GetComponent<RectTransform>();
        RectTransform equipment = bgController.Find("equipment").GetComponent<RectTransform>();
        RectTransform store = bgController.Find("store").GetComponent<RectTransform>();

        // PC 해상도에 맞게 크기 조정
        Vector2 newSize = new Vector2(1920f, 1080f);
        
        // 각 배경화면의 크기 설정
        if (main != null) {
            main.sizeDelta = newSize;
            Debug.Log("Main 크기 변경: " + newSize);
        }
        
        if (equipment != null) {
            equipment.sizeDelta = newSize;
            equipment.anchoredPosition = new Vector2(-1920f, 0);
            Debug.Log("Equipment 크기 변경: " + newSize);
        }
        
        if (store != null) {
            store.sizeDelta = newSize;
            store.anchoredPosition = new Vector2(1920f, 0);
            Debug.Log("Store 크기 변경: " + newSize);
        }
        
        Debug.Log("배경 크기 조정 완료!");
    }
}
