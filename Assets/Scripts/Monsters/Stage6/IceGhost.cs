using UnityEngine;

public class IceGhost : NormalMonster
{
    //public float moveSpeed = 5f; // 이동 속도
    public float amplitude = 2f; // 좌우 흔들림의 폭
    public float frequency = 2f; // 좌우 흔들림의 빈도
    private float poisonDuration = 5f; // 중독 지속시간 (초)
    private float attackCooldown = 5f; // 공격 쿨다운 시간
    private float lastAttackTime = 0f; // 마지막 공격 시간
    
    protected override void Start()
    {
        movement = new SinMovement(amplitude, frequency, moveSpeed);
        base.Start();
    }
    
    protected override void CheckAttackRange()
    {
        // 플레이어가 공격 범위 내에 있고 쿨다운이 지났으면 공격
        if (Vector2.Distance(transform.position, playerTransform.position) < attackRange 
            && Time.time > lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }
    
    protected override void Attack()
    {
        // 플레이어를 찾고 존재하는 경우에만 공격 실행
        Player player = playerTransform.GetComponent<Player>();
        if (player != null)
        {            
            // 중독 효과 코루틴 시작 (몬스터가 사라져도 효과가 지속되도록 GameManager에서 실행)
            GameManager.Instance.StartCoroutine(ApplyPoisonEffect(player, damage * 0.2f, poisonDuration));
        }
    }
    
    // 플레이어에게 독 효과를 주는 코루틴
    private System.Collections.IEnumerator ApplyPoisonEffect(Player player, float poisonDamage, float duration)
    {
        float elapsedTime = 0;
        float tickInterval = 1f; // 1초마다 데미지
        
        while (elapsedTime < duration)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsedTime += tickInterval;
            
            // 플레이어가 아직 존재하는지 확인
            if (player != null)
            {
                //player.TakeDamage(poisonDamage);
                //디버그 로그
                Debug.Log($"Poison damage applied: {poisonDamage} to {player.name}");
                // 시각적 피드백을 위한 특수 효과 (선택적)
                //player.ApplySpecialEffect(Player.PlayerSpecialEffect.Slow, 0.5f);
            }
            else
            {
                // 플레이어가 사라졌으면 코루틴 종료
                yield break;
            }
        }
    }
}
