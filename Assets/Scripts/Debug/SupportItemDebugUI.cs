using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 지원 아이템 레벨업 테스트용 디버그 UI 클래스입니다.
/// </summary>
public class SupportItemDebugUI : MonoBehaviour
{
    // Unity Inspector에 노출할 필드
    [Header("UI 설정")]
    [Tooltip("UI 창 표시 토글 키")]
    public KeyCode toggleKey = KeyCode.F2;
    [Tooltip("UI 창의 너비")]
    public float windowWidth = 300f;
    [Tooltip("UI 창의 높이")]
    public float windowHeight = 500f;
    [Tooltip("UI 창의 좌측 상단 X 좌표")]
    public float windowX = 20f;
    [Tooltip("UI 창의 좌측 상단 Y 좌표")]
    public float windowY = 50f;

    // 내부 상태 관리 필드
    private bool showWindow = false;
    private Vector2 scrollPosition;
    private List<SupportItemSO> supportItems = new List<SupportItemSO>();
    private GUIStyle headerStyle;
    private GUIStyle itemBoxStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;
    private GUIStyle levelStyle;
    private GUIStyle descStyle;

    private void Start()
    {
        // 지원 아이템 ScriptableObject 불러오기
        LoadSupportItems();
    }

    private void LoadSupportItems()
    {
        // Resources 폴더에서 모든 SupportItemSO 에셋 로드
        SupportItemSO[] items = Resources.LoadAll<SupportItemSO>("ScriptableObjects/SupportItems");
        if (items != null && items.Length > 0)
        {
            supportItems.AddRange(items);
            Debug.Log($"지원 아이템 {items.Length}개가 로드되었습니다.");
        }
        else
        {
            Debug.LogWarning("Resources/ScriptableObjects/SupportItems 폴더에서 지원 아이템을 찾을 수 없습니다!");
        }
    }

    private void Update()
    {
        // 토글 키로 UI 표시 여부 전환
        if (Input.GetKeyDown(toggleKey))
        {
            showWindow = !showWindow;
        }
    }

    private void InitStyles()
    {
        if (headerStyle == null)
        {
            // 헤더 스타일
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.margin = new RectOffset(5, 5, 10, 10);

            // 아이템 박스 스타일
            itemBoxStyle = new GUIStyle(GUI.skin.box);
            itemBoxStyle.margin = new RectOffset(5, 5, 5, 5);
            itemBoxStyle.padding = new RectOffset(10, 10, 10, 10);

            // 라벨 스타일
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.margin = new RectOffset(0, 0, 5, 5);

            // 버튼 스타일
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin = new RectOffset(2, 2, 2, 2);
            buttonStyle.padding = new RectOffset(8, 8, 3, 3);

            // 레벨 스타일
            levelStyle = new GUIStyle(GUI.skin.label);
            levelStyle.fontSize = 14;
            levelStyle.alignment = TextAnchor.MiddleCenter;
            levelStyle.normal.textColor = Color.yellow;

            // 설명 스타일
            descStyle = new GUIStyle(GUI.skin.label);
            descStyle.wordWrap = true;
            descStyle.fontSize = 10;
            descStyle.margin = new RectOffset(0, 0, 5, 5);
        }
    }

    private void OnGUI()
    {
        if (!showWindow) return;

        InitStyles();

        // UI 창 그리기
        GUI.Box(new Rect(windowX, windowY, windowWidth, windowHeight), "");
        
        // 헤더
        GUI.Label(new Rect(windowX, windowY, windowWidth, 30), "지원 아이템 테스트", headerStyle);

        // 스크롤뷰 영역
        Rect scrollViewRect = new Rect(windowX, windowY + 30, windowWidth, windowHeight - 30);
        Rect contentRect = new Rect(0, 0, windowWidth - 20, supportItems.Count * 140);

        scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, contentRect);

        // 아이템이 없는 경우 안내 메시지 표시
        if (supportItems.Count == 0)
        {
            GUI.Label(new Rect(10, 10, windowWidth - 40, 50), "Resources/ScriptableObjects/SupportItems 폴더에 지원 아이템을 생성해주세요.", descStyle);
        }

        // 각 아이템별 UI 그리기
        for (int i = 0; i < supportItems.Count; i++)
        {
            SupportItemSO item = supportItems[i];
            Rect itemRect = new Rect(10, i * 140, windowWidth - 40, 130);
            GUI.Box(itemRect, "", itemBoxStyle);

            // 아이템 이름 및 현재 레벨
            int currentLevel = 0;
            if (GameManager.Instance != null && GameManager.Instance.supportItemLevels.ContainsKey(item))
            {
                currentLevel = GameManager.Instance.supportItemLevels[item];
            }

            GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 10, itemRect.width - 20, 20), item.itemName, labelStyle);
            GUI.Label(new Rect(itemRect.x + itemRect.width - 90, itemRect.y + 10, 80, 20), $"Lv. {currentLevel}/{item.maxLevel}", levelStyle);

            // 아이템 설명
            string desc = item.description;
            if (string.IsNullOrEmpty(desc))
            {
                desc = "설명 없음";
            }
            GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 35, itemRect.width - 20, 40), desc, descStyle);

            // 효과 타입 및 대상 스탯
            string effectInfo = $"효과 타입: {item.effectType}";
            if (item.effectType == SupportItemEffectType.StatBoost)
            {
                effectInfo += $" | 대상 스탯: {item.targetStat}";
            }
            GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 80, itemRect.width - 20, 20), effectInfo, descStyle);

            // 레벨업 버튼 및 초기화 버튼
            if (GUI.Button(new Rect(itemRect.x + 10, itemRect.y + 100, 80, 25), "레벨업", buttonStyle))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LevelUpSupportItem(item);
                }
                else
                {
                    Debug.LogError("GameManager.Instance가 null입니다!");
                }
            }

            if (GUI.Button(new Rect(itemRect.x + 100, itemRect.y + 100, 80, 25), "초기화", buttonStyle))
            {
                if (GameManager.Instance != null && GameManager.Instance.supportItemLevels.ContainsKey(item))
                {
                    GameManager.Instance.supportItemLevels[item] = 0;
                    Debug.Log($"{item.itemName} 레벨이 0으로 초기화되었습니다.");
                }
            }
        }

        GUI.EndScrollView();
    }
}