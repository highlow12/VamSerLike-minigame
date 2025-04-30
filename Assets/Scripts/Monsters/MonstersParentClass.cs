using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [SerializeField] protected float maxHealth = 1;
    [SerializeField] protected float currentHealth = 1;
    [SerializeField] protected float damage = 1;
    [SerializeField] protected float attackRange = 0.1f;

    [Header("사운드 설정")]
    [SerializeField] protected List<AudioClip> moveSounds = new List<AudioClip>(); // 이동 사운드 리스트
    [SerializeField] protected List<AudioClip> attackSounds = new List<AudioClip>(); // 공격 사운드 리스트
    [SerializeField] protected List<AudioClip> hurtSounds = new List<AudioClip>(); // 피격 사운드 리스트
    [SerializeField] protected List<AudioClip> deathSounds = new List<AudioClip>(); // 사망 사운드 리스트
    [SerializeField] protected float soundVolume = 0.7f;
    [SerializeField] protected float minSoundDistance = 1f;
    [SerializeField] protected float maxSoundDistance = 15f;

    // 사운드 ID 관련 변수들
    protected int moveSoundID = -1;
    protected bool isMoveSoundPlaying = false;

    protected bool isDead;
    protected Transform playerTransform;

    protected Animator anim;
    protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        playerTransform = GameManager.Instance.player.transform;
    }

    public virtual void TakeDamage(float damage)
    {
        float actualDamage = Mathf.Max(damage, 1);
        currentHealth -= actualDamage;

        // 피격 사운드 재생
        PlayHurtSound();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Debug.Log("Dead: " + gameObject.name);
        isDead = true;
        
        anim.Play("Dead");

        // 죽음 사운드 재생
        PlayDeathSound();
        
        // 이동 사운드가 재생중이면 정지
        StopMoveSound();
        
        DropLoot();
    }

    public void DieAnimFinished()
    {
        Debug.Log("Deaddddd");
        // 사망 처리 로직
        MonsterPoolManager.Instance.ReturnMonsterToPool(gameObject, GetComponent<MonsterIdentify>().monsterName);
    }

    protected virtual void DropLoot()
    {
        // 임시 로직
        DropItemManager.Instance.DropItem(transform.position);
    }

    //protected abstract void DropLoot();
    protected abstract void Attack();

    public float DemoAttack()
    {
        return damage;
    }

    /// <summary>
    /// Resets the monster's health to its maximum value.
    /// </summary>
    public void ResetHealth()
    {
        sr.color = new Color(1, 1, 1, 1);
        currentHealth = maxHealth;
        isDead = false;
    }

    /// <summary>
    /// Resets the monster's position to a default or predefined value.
    /// </summary>
    public void ResetPosition()
    {
        transform.position = Vector3.zero; // 기본 위치로 초기화 (필요에 따라 수정 가능)
    }
    
    #region 사운드 관련 메서드
    
    /// <summary>
    /// 몬스터 이동 사운드를 재생합니다. 지정된 리스트에서 랜덤하게 선택됩니다.
    /// </summary>
    public virtual void PlayMoveSound()
    {
        if (moveSounds.Count == 0 || isMoveSoundPlaying) return;
        
        // 이동 사운드가 이미 재생 중이면 중지
        StopMoveSound();
        
        // 리스트에서 랜덤한 사운드 선택
        AudioClip selectedSound = GetRandomSound(moveSounds);
        
        if (selectedSound != null)
        {
            // 3D 위치 사운드로 재생 (루프로 설정)
            moveSoundID = SoundManager.Instance.PlaySFXAtPosition(
                selectedSound, 
                transform.position, 
                soundVolume, 
                minSoundDistance, 
                maxSoundDistance, 
                "MonsterMove", 
                false // 루프 설정
            );
            
            isMoveSoundPlaying = true;
        }
    }
    
    /// <summary>
    /// 몬스터 이동 사운드를 정지합니다.
    /// </summary>
    public virtual void StopMoveSound()
    {
        if (moveSoundID != -1)
        {
            SoundManager.Instance.StopSound(moveSoundID);
            moveSoundID = -1;
            isMoveSoundPlaying = false;
        }
    }
    
    /// <summary>
    /// 몬스터 공격 사운드를 재생합니다. 지정된 리스트에서 랜덤하게 선택됩니다.
    /// </summary>
    public virtual void PlayAttackSound()
    {
        if (attackSounds.Count == 0) return;
        
        // 리스트에서 랜덤한 사운드 선택
        AudioClip selectedSound = GetRandomSound(attackSounds);
        
        if (selectedSound != null)
        {
            SoundManager.Instance.PlaySFXAtPosition(
                selectedSound, 
                transform.position, 
                soundVolume, 
                minSoundDistance, 
                maxSoundDistance, 
                "MonsterAttack"
            );
        }
    }
    
    /// <summary>
    /// 몬스터 피격 사운드를 재생합니다. 지정된 리스트에서 랜덤하게 선택됩니다.
    /// </summary>
    public virtual void PlayHurtSound()
    {
        if (hurtSounds.Count == 0) return;
        
        // 리스트에서 랜덤한 사운드 선택
        AudioClip selectedSound = GetRandomSound(hurtSounds);
        
        if (selectedSound != null)
        {
            SoundManager.Instance.PlaySFXAtPosition(
                selectedSound, 
                transform.position, 
                soundVolume, 
                minSoundDistance, 
                maxSoundDistance, 
                "MonsterHurt"
            );
        }
    }
    
    /// <summary>
    /// 몬스터 사망 사운드를 재생합니다. 지정된 리스트에서 랜덤하게 선택됩니다.
    /// </summary>
    public virtual void PlayDeathSound()
    {
        if (deathSounds.Count == 0) return;
        
        // 리스트에서 랜덤한 사운드 선택
        AudioClip selectedSound = GetRandomSound(deathSounds);
        
        if (selectedSound != null)
        {
            SoundManager.Instance.PlaySFXAtPosition(
                selectedSound, 
                transform.position, 
                soundVolume, 
                minSoundDistance, 
                maxSoundDistance, 
                "MonsterDeath"
            );
        }
    }
    
    /// <summary>
    /// 이동 사운드의 위치를 업데이트합니다.
    /// </summary>
    protected virtual void UpdateMoveSoundPosition()
    {
        if (moveSoundID != -1 && isMoveSoundPlaying)
        {
            SoundManager.Instance.ModifySound(moveSoundID, null, transform.position);
        }
    }
    
    /// <summary>
    /// 리스트에서 랜덤한 오디오 클립을 선택합니다.
    /// </summary>
    /// <param name="soundList">선택할 오디오 클립 리스트</param>
    /// <returns>선택된 오디오 클립 또는 리스트가 비어있을 경우 null</returns>
    protected AudioClip GetRandomSound(List<AudioClip> soundList)
    {
        if (soundList == null || soundList.Count == 0) return null;
        
        // 최근 재생한 사운드를 추적하는 변수를 추가하여 같은 사운드가 연속해서 재생되지 않게 할 수도 있음
        int randomIndex = Random.Range(0, soundList.Count);
        return soundList[randomIndex];
    }
    
    #endregion
}


// 일반 몬스터 클래스
public class NormalMonster : Monster
{
    [SerializeField] protected float moveSpeed = 1;
    protected IMovement movement/* = Enter Movement*/;

    protected override void Start()
    {
        base.Start();
        // 일반 몬스터는 한 가지 이동 패턴만 사용
        if (movement == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Enter Movement");
#endif
            Destroy(this);
        }
    }

    protected virtual void Update()
    {
        if (!isDead)
        {
            if (Vector2.Distance(transform.position, playerTransform.position) < attackRange)
            {
                CheckAttackRange();
                // 공격 범위에서는 이동 사운드 중지
                StopMoveSound();
            }
            else
            {
                // 이동 중에는 이동 사운드 재생
                PlayMoveSound();
                // 이동 사운드 위치 업데이트
                UpdateMoveSoundPosition();
                Move();
            }

        }
    }

    /// <summary>
    /// Moves the monster based on the movement behavior.
    /// </summary>
    private void Move()
    {
        // Calculate movement using the movement interface
        movement.CalculateMovement(transform);

        // Assuming movementVector is updated within CalculateMovement
        Vector2 movementVector = transform.position - (Vector3)playerTransform.position;
        int sign = Mathf.Sign(transform.localScale.x) == Mathf.Sign(movementVector.x) ? 1 : -1;
        // Update local scale based on movement direction
        transform.localScale = new Vector3(
            transform.localScale.x * sign,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    /// <summary>
    /// Checks if the player is within attack range.
    /// </summary>
    protected virtual void CheckAttackRange()
    {
#if UNITY_EDITOR
        ////throw new System.NotImplementedException();
#endif
    }

    /// <summary>
    /// Executes the monster's attack behavior.
    /// </summary>
    protected override void Attack()
    {
        // 공격 사운드 재생
        PlayAttackSound();
        
#if UNITY_EDITOR
        ////throw new System.NotImplementedException();
#endif
    }
}

// 보스 몬스터

public abstract class BossMonster : Monster
{
    [SerializeField] private float phaseChangeHealthThreshold;
    [SerializeField] private float berserkHealthThreshold;
    protected Dictionary<int, BaseState> states;
    private float stateTimer;
    protected MonsterFSM monsterFSM;


    protected override void Start()
    {
        base.Start();
        initState();
    }
    protected abstract void initState();
    protected abstract void checkeState();
    public void Update()
    {
        monsterFSM.UpdateState();
        checkeState();
        
        // 이동 사운드 위치 업데이트
        UpdateMoveSoundPosition();
    }
    // Boss Take Damage
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        // 보스 특수 피해 처리
        if (currentHealth <= phaseChangeHealthThreshold)
        {
            // 페이즈 변경 로직
        }
    }

    protected override void Attack()
    {
        // 공격 사운드 재생
        PlayAttackSound();
        
#if UNITY_EDITOR
        //throw new System.NotImplementedException();
#endif
    }
}