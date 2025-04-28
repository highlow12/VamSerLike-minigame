using UnityEngine;
using Item;
using DG.Tweening.Core.Easing;

public class Player : MonoBehaviour
{
    public enum HealType
    {
        Fixed,
        PercentageByMaxHealth,
        PercentageByLostHealth,
    }
    public enum PlayerSkin
    {
        Normal,
        Black,
        Brown,
        Green,
        Red,
        White
    }

    public enum BonusStat
    {
        AttackDamage,
        AttackSpeed,
        AttackRange,
        AttackTarget,
        AttackProjectileCount,
        AttackProjectileSpeed,
        MaxHealth,
        MovementSpeed,
        Experience
    }

    public enum BonusStatType
    {
        Fixed,
        Percentage
    }
    [System.Serializable]
    public struct PlayerBuffEffect
    {
        public string buffName;
        public float endTime;
    }

    public struct PlayerBonusStat
    {
        public PlayerBuffEffect playerBuffEffect;
        public BonusStat bonusStat;
        public BonusStatType bonusStatType;
        public float value;
    }

    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private SpriteRenderer healthBar;
    private SpriteRenderer _playerSprite;
    private Animator _playerAnimator;
    [SerializeField]
    private PlayerSkin _currentPlayerSkin = PlayerSkin.Normal;
    private PlayerSkin _lastSkin;

    public PlayerSkin currentPlayerSkin
    {
        get => _currentPlayerSkin;
        set
        {
            if (_currentPlayerSkin != value)
            {
                _currentPlayerSkin = value;
                ChangeSkin(value);
            }
        }
    }

    private void ChangeSkin(PlayerSkin skin)
    {
        string path = $"ScriptableObjects/Player/Skin/{skin}";
        PlayerSkinSO playerSkinData = Resources.Load<PlayerSkinSO>(path);
        if (playerSkinData != null && _playerSprite != null && _playerAnimator != null)
        {
            _playerSprite.sprite = playerSkinData.playerSprite;
            _playerAnimator.runtimeAnimatorController = playerSkinData.playerAnimator;
        }
        for (int i = GameManager.Instance.playerBonusStats.Count - 1; i >= 0; i--)
        {
            PlayerBonusStat bonusStat = GameManager.Instance.playerBonusStats[i];
            if (bonusStat.playerBuffEffect.buffName.StartsWith($"[Skin buff]"))
            {
                GameManager.Instance.RemoveBuffEffect(bonusStat.playerBuffEffect);
            }
        }
        PlayerBonusStatsDictionary bonusStats = playerSkinData.playerBonusStats;
        foreach (var bonusStat in bonusStats)
        {
            GameManager.Instance.AddBonusStat(new PlayerBonusStat
            {
                playerBuffEffect = new PlayerBuffEffect
                {
                    buffName = $"[Skin buff] {skin} {bonusStat.Key}",
                    endTime = long.MaxValue
                },
                bonusStat = bonusStat.Key,
                bonusStatType = BonusStatType.Percentage,
                value = bonusStat.Value
            });
        }
    }

    private void OnValidate()
    {
        if (_lastSkin != _currentPlayerSkin)
        {
            _lastSkin = _currentPlayerSkin;
            if (!Application.isPlaying)
            {
                _playerSprite ??= GetComponent<SpriteRenderer>();
                _playerAnimator ??= GetComponent<Animator>();
            }
            ChangeSkin(_currentPlayerSkin);
        }
    }

    public float baseMaxHealth = 100f;
    public float maxHealth;
    public float health;
    private float normalizedHealth;
    private bool isDead = false;

    void Awake()
    {
        _playerSprite = GetComponent<SpriteRenderer>();
        _playerAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        ChangeSkin(_currentPlayerSkin);
        maxHealth = GameManager.Instance.GetPlayerStatValue(BonusStat.MaxHealth, baseMaxHealth);
        health = maxHealth;
    }

    void Update()
    {
        maxHealth = GameManager.Instance.GetPlayerStatValue(BonusStat.MaxHealth, baseMaxHealth);
        normalizedHealth = Mathf.Clamp(health, 0f, maxHealth) / maxHealth;
        healthBar.size = new Vector2(normalizedHealth, healthBar.size.y);

        if (health <= 0 && !isDead)
        {
            isDead = true;
            FadeManager.Instance.FadeOutAndRestart();
        }
    }

    public void Heal(float amount, HealType healType)
    {
        float newAmount = healType switch
        {
            HealType.Fixed => amount,
            HealType.PercentageByMaxHealth => maxHealth * amount,
            HealType.PercentageByLostHealth => (maxHealth - health) * amount,
            _ => 0,
        };
        health = Mathf.Clamp(health + newAmount, 0f, maxHealth);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("DropItem") && col.TryGetComponent<DropItem>(out var dropItem))
        {
            dropItem.UseItem();
        }
    }
}
