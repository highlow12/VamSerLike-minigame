using UnityEngine;

public class CrawlingMonster : NormalMonster
{
    // 이동 애니메이션(거미처럼 기어다니는 형태)
    public Animator animator;
    public AudioSource crawlAudioSource;
    public AudioClip crawlClip;

    // 마취총 공격 효과
    public float stunDuration = 3f;
    public int attackDamage = 10;

    protected override void Start()
    {
        movement = new StraightMovement(moveSpeed); // 거미처럼 기어다니는 이동(기본 직선 이동)
        base.Start();
        if (crawlAudioSource && crawlClip)
        {
            crawlAudioSource.clip = crawlClip;
            crawlAudioSource.loop = true;
            crawlAudioSource.Play();
        }
        if (animator)
        {
            animator.SetBool("IsCrawling", true);
        }
    }

    // 공격 함수: 공격 애니메이션 없이 효과만 적용
    protected override void Attack()
    {
        var player = playerTransform.GetComponent<Player>();
        if (player != null)
        {
            player.ApplySpecialEffect(Player.PlayerSpecialEffect.Stun, stunDuration);
            player.TakeDamage(attackDamage);
        }
    }
}
