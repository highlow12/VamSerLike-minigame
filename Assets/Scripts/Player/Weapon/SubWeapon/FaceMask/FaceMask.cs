using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FaceMask : Weapon.SubWeapon
{
    private Player playerReference;
    private SpriteRenderer playerSpriteRenderer;
    private float originalPlayerAlpha;
    private bool isEffectActive = false;

    protected override void Awake()
    {
        weaponType = WeaponType.FaceMask;
        base.Awake();

        // 플레이어 참조 가져오기
        playerReference = GameManager.Instance.playerScript;
        playerSpriteRenderer = playerReference.GetComponent<SpriteRenderer>();
        originalPlayerAlpha = playerSpriteRenderer.color.a;

        // 마스크 위치 조정
        attackObject.transform.SetParent(playerReference.transform);
        attackObject.transform.localPosition = new Vector3(-0.025f, 0.25f, 0); // 플레이어 얼굴 위치에 맞게 조정

    }

    protected override void InitStat()
    {
        base.InitStat();
        weaponScript.subWeaponSO = weaponData;
        weaponScript.SubWeaponInit();
        if (weaponGrade == 2)
        {
            attackObject.transform.localPosition = new Vector3(-0.025f, 0.5f, 0); // 플레이어 얼굴 위치에 맞게 조정
        }
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        isAttackCooldown = true;
        weaponScript.Init(attackDamage, attackRange, 0, attackTarget);
        ApplyInvincibilityEffect();
        yield return new WaitForSeconds(attackDamage);
        RemoveInvincibilityEffect();
        yield return new WaitForSeconds(1f / attackSpeed);
        isAttackCooldown = false;
    }

    private void ApplyInvincibilityEffect()
    {
        if (isEffectActive)
            return;

        isEffectActive = true;

        // 플레이어에게 무적 효과 적용
        playerReference.ApplySpecialEffect(Player.PlayerSpecialEffect.Invincible, attackDamage);

        // 플레이어 투명도 변경 (70%)
        Color playerColor = playerSpriteRenderer.color;
        playerSpriteRenderer.DOColor(new Color(playerColor.r, playerColor.g, playerColor.b, 0.7f), 0.2f);

        // 마스크 시각 효과
        attackObject.transform.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.2f);

        // 로그 출력
#if UNITY_EDITOR
        DebugConsole.Line effectLog = new()
        {
            text = $"[{GameManager.Instance.gameTimer}] FaceMask: Invincibility activated for {attackDamage} seconds",
            messageType = DebugConsole.MessageType.Local,
            tick = GameManager.Instance.gameTimer
        };
        DebugConsole.Instance.MergeLine(effectLog, "#00FFFF");
#endif
    }

    private void RemoveInvincibilityEffect()
    {
        isEffectActive = false;

        // 플레이어 투명도 복원
        Color playerColor = playerSpriteRenderer.color;
        playerSpriteRenderer.DOColor(new Color(playerColor.r, playerColor.g, playerColor.b, originalPlayerAlpha), 0.2f);

        // 마스크 시각 효과 복원
        attackObject.transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f);

        // 무적 효과는 GameManager의 ChangeValueForDuration에 의해 자동으로 제거됩니다
    }
}
