using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlayerBonusStatsDictionary : SerializableDictionary<Player.BonusStat, float> { }

[CreateAssetMenu(fileName = "PlayerSkinSO", menuName = "Scriptable Objects/PlayerSkinSO")]
public class PlayerSkinSO : ScriptableObject
{
    public const string toolTipText = "플레이어 스킨을 설정합니다.\n플레이어 스킨은 플레이어의 외형과 애니메이션을 결정합니다.\n"
    + "Player Bonus Stats의 모든 스탯들은 모두 배율로 적용됩니다.\n모든 스탯 배율은 곱연산으로 적용됩니다.";
    public Player.PlayerSkin playerSkin;
    public Sprite playerSprite;
    public AnimatorOverrideController playerAnimator;
    [Header("Player Bonus Stats")]
    public PlayerBonusStatsDictionary playerBonusStats = new();
}
