using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnTestUi : MonoBehaviour
{
    private List<string> monsterPoolObjects = new List<string>();
    private Vector2 scrollPosition;
    private int selectedMonsterIndex = -1;
    private string selectedMonsterName = "";

    private void Start()
    {
        RefreshMonsterPoolObjects();
    }

    public void RefreshMonsterPoolObjects()
    {
        monsterPoolObjects.Clear();

        // ObjectPoolManager의 objectInfos에서 MonsterPool 오브젝트들 가져오기
        foreach (var objectInfo in ObjectPoolManager.Instance.objectInfos)
        {
            if (objectInfo.parent != null && objectInfo.parent.name == "MonsterPool")
            {
                monsterPoolObjects.Add(objectInfo.objectName);
            }
        }
    }

    public void MonsterSpawnGUI()
    {
        GUIStyle titleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft
        };

        GUILayout.Label("Monster Spawn Test", titleStyle);

        // 새로고침 버튼
        if (GUILayout.Button("Refresh Monster List", buttonStyle, GUILayout.Width(150), GUILayout.Height(25)))
        {
            RefreshMonsterPoolObjects();
        }

        // 몬스터 목록이 비었을 경우 메시지 표시
        if (monsterPoolObjects.Count == 0)
        {
            GUILayout.Label("No monsters found in MonsterPool", labelStyle);
            return;
        }

        GUILayout.Space(5);
        GUILayout.Label("Available Monsters:", labelStyle);

        // 스크롤 뷰 시작
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150), GUILayout.Width(300));

        // 몬스터 목록 표시
        for (int i = 0; i < monsterPoolObjects.Count; i++)
        {
            GUIStyle itemStyle = new GUIStyle(GUI.skin.button);
            if (i == selectedMonsterIndex)
            {
                itemStyle.normal.background = MakeTexture(2, 2, new Color(0.5f, 0.7f, 1f, 0.5f));
            }

            if (GUILayout.Button(monsterPoolObjects[i], itemStyle))
            {
                selectedMonsterIndex = i;
                selectedMonsterName = monsterPoolObjects[i];
            }
        }

        GUILayout.EndScrollView();

        GUILayout.Space(10);

        // 선택한 몬스터 이름 표시
        if (selectedMonsterIndex >= 0)
        {
            GUILayout.Label($"Selected: {selectedMonsterName}", labelStyle);

            GUILayout.BeginHorizontal();

            // 스폰 버튼
            if (GUILayout.Button("Spawn at Player", buttonStyle, GUILayout.Width(150), GUILayout.Height(30)))
            {
                SpawnMonsterAtPlayer(selectedMonsterName);
            }

            // 여러 마리 스폰 버튼
            if (GUILayout.Button("Spawn 5", buttonStyle, GUILayout.Width(80), GUILayout.Height(30)))
            {
                for (int i = 0; i < 5; i++)
                {
                    SpawnMonsterAtPlayer(selectedMonsterName, Random.Range(-3f, 3f), Random.Range(-3f, 3f));
                }
            }

            GUILayout.EndHorizontal();
        }
    }

    private void SpawnMonsterAtPlayer(string monsterName, float offsetX = 0, float offsetY = 0)
    {
        if (string.IsNullOrEmpty(monsterName) || GameManager.Instance.player == null) return;

        Vector3 playerPos = GameManager.Instance.player.transform.position;
        Vector3 spawnPos = new Vector3(playerPos.x + offsetX, playerPos.y + offsetY, playerPos.z);

        GameObject monster = ObjectPoolManager.Instance.GetGo(monsterName);
        if (monster != null)
        {
            monster.transform.position = spawnPos;
            Debug.Log($"Spawned {monsterName} at {spawnPos}");
        }
    }

    // 단색 텍스처 생성 (선택 표시용)
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
