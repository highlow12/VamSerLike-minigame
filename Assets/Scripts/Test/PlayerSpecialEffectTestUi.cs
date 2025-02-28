using UnityEngine;
using System;
using System.Linq;

public class PlayerSpecialEffectTestUi : MonoBehaviour
{
    // 특수 효과 지속 시간 설정
    private float effectDuration = 5f;

    public void PlayerSpecialEffectGUI()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("특수 효과 지속 시간", GUILayout.Width(150));

        // 지속 시간 입력 필드
        string durationStr = GUILayout.TextField(effectDuration.ToString(), GUILayout.Width(100));
        float.TryParse(durationStr, out effectDuration);

        // PlayerSpecialEffect enum의 모든 값들을 버튼으로 표시
        Array effectValues = Enum.GetValues(typeof(Player.PlayerSpecialEffect));

        foreach (Player.PlayerSpecialEffect effect in effectValues)
        {
            // None 효과는 제외
            if (effect == Player.PlayerSpecialEffect.None)
                continue;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);

            // 현재 플레이어에게 적용된 효과는 다른 색상으로 표시
            if (GameManager.Instance.playerScript.playerSpecialEffect.HasFlag(effect))
            {
                buttonStyle.normal.textColor = Color.green;
            }

            if (GUILayout.Button(effect.ToString(), buttonStyle, GUILayout.Width(150)))
            {
                ApplySpecialEffect(effect);
            }
        }

        // 모든 효과 제거 버튼
        if (GUILayout.Button("모든 효과 제거", GUILayout.Width(150)))
        {
            GameManager.Instance.playerScript.playerSpecialEffect = Player.PlayerSpecialEffect.None;
        }

        GUILayout.EndVertical();
    }

    private void ApplySpecialEffect(Player.PlayerSpecialEffect effect)
    {
        if (GameManager.Instance.playerScript != null)
        {
            GameManager.Instance.playerScript.ApplySpecialEffect(effect, effectDuration);
            Debug.Log($"{effect} 효과가 {effectDuration}초 동안 적용되었습니다.");
        }
        else
        {
            Debug.LogError("플레이어가 존재하지 않습니다!");
        }
    }
}
