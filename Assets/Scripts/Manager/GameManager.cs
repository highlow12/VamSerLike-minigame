using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        InGame,
        Pause,
        GameOver
    }

    // ������ �̺�Ʈ ���� - �̺�Ʈ�� null üũ ���� ȣ���� �� �ֵ��� ��
    // ��������Ʈ ����
    public delegate void OnPlayerLevelChanged();
    // �̺�Ʈ �ʱ�ȭ�Ͽ� null�� ���� �ʵ��� ��
    public event OnPlayerLevelChanged onPlayerLevelChanged = delegate { };
    public delegate void OnGamePaused();
    public event OnGamePaused onGamePaused;

    public Player playerScript;
    public Light2D playerLight;
    public PlayerMove player;
    public PlayerAttack playerAttack;
    public GameState gameState
    {
        get => _GameState;
        set
        {
            _GameState = value;
        }
    }
    private GameState _GameState;
    public List<Player.PlayerBuffEffect> playerBuffEffects = new();
    public List<Player.PlayerBonusStat> playerBonusStats = new();
    public JsonData playerAssetData;
    public long gameTimer;
    public int currentStage = 0;
    public float dragDistanceMultiplier = 1.0f;
    public float dragSpeedMultiplier = 1.0f;
    public float playerExperienceMultiplier = 1.0f;
    public float experienceToLevelUp = 100;
    public float playerExperience = 0;

    // playerLevel ������Ƽ ����
    public long playerLevel
    {
        get => _playerLevel;
        set
        {
            Debug.Log($"[GameManager] ���� ����: {_playerLevel} -> {value}");
            _playerLevel = value;

            // value > 1 üũ�� �����ϰ� �׻� �̺�Ʈ �߻�
            Debug.Log($"[GameManager] onPlayerLevelChanged �̺�Ʈ ȣ��");
            // null üũ�� �ʿ� ���� - �� ��������Ʈ�� �ʱ�ȭ�߱� ����
            onPlayerLevelChanged.Invoke();
        }
    }
    private long _playerLevel = 1;
    public static bool IsGamePaused
    {
        get { return Instance._isGamePaused; }
        set
        {
            Instance._isGamePaused = value;
            Instance.onGamePaused?.Invoke();
        }
    }
    private bool _isGamePaused = false;

    // ������ ��⿭ ������ ���� ���� �߰�
    public bool isProcessingLevelUp { get; private set; } = false;
    private Queue<Action> levelUpQueue = new Queue<Action>();

    public override void Awake()
    {
        base.Awake();
        Debug.Log("[GameManager] Awake ȣ���");

        // �׽�Ʈ�� ���� ������ �̺�Ʈ �⺻ ������ �߰�
        onPlayerLevelChanged += () => { Debug.Log("[GameManager] ������ �̺�Ʈ �⺻ ó���� ȣ���"); };

        onGamePaused += () =>
        {
            Time.timeScale = IsGamePaused ? 0 : 1;
            Debug.Log($"[GameManager] Time.timeScale ������: {Time.timeScale}");
        };

        SetGameState(GameState.InGame);
        playerAssetData = BackendDataManager.Instance.GetUserAssetData();
    }

    // Set game state
    public void SetGameState(GameState newState)
    {
        gameState = newState;
        switch (gameState)
        {
            default:
                break;
        }
    }

    public bool SetStage(int stageNumber)
    {
        List<string> existingDropItems = DropItemManager.Instance.GetDropItems();
        if (existingDropItems.Count > 0)
        {
            foreach (string dropItem in existingDropItems)
            {
                ObjectPoolManager.Instance.UnregisterObjectPool(dropItem);
            }
        }
        bool result = DropItemManager.Instance.SetProbabilityTitle($"Stage{stageNumber}_DropItemProb");
        if (!result)
        {
#if UNITY_EDITOR
            DebugConsole.Line errorLog = new()
            {
                text = $"[{gameTimer}] Failed to set probability card for stage {stageNumber}",
                messageType = DebugConsole.MessageType.Local,
                tick = gameTimer
            };
            DebugConsole.Instance.MergeLine(errorLog, "#FF0000");
#endif
        }
        else
        {
            currentStage = stageNumber;
            List<string> dropItems = DropItemManager.Instance.GetDropItems();
            foreach (string dropItem in dropItems)
            {
                string path = $"Prefabs/In-game/DropItem/{dropItem}";
                GameObject dropItemPrefab = Resources.Load<GameObject>(path);
                if (dropItemPrefab == null)
                {
#if UNITY_EDITOR
                    DebugConsole.Line errorLog = new()
                    {
                        text = $"[{gameTimer}] Failed to load drop item prefab {dropItem}",
                        messageType = DebugConsole.MessageType.Local,
                        tick = gameTimer
                    };
                    DebugConsole.Instance.MergeLine(errorLog, "#FF0000");
#endif
                    continue;

                }
                else
                {
                    ObjectPoolManager.Instance.RegisterObjectPool(dropItem, dropItemPrefab, null, 10);
                }
            }
        }
        return result;
    }

    void FixedUpdate()
    {
        if (gameState == GameState.InGame)
        {
            foreach (Player.PlayerBuffEffect buffEffect in playerBuffEffects)
            {
                if (buffEffect.endTime <= gameTimer)
                {
                    RemoveBuffEffect(buffEffect);
                }
            }
            gameTimer += 1;
        }
    }

    public void AddBonusStat(Player.PlayerBonusStat bonusStat)
    {
        if (Equals(bonusStat.playerBuffEffect, default))
        {
            Debug.LogError("PlayerBuffEffect is null");
            return;
        }
        playerBonusStats.Add(bonusStat);
        playerBuffEffects.Add(bonusStat.playerBuffEffect);
    }

    public float GetPlayerStatValue(Player.BonusStat statEnum, float value)
    {
        float result = value;
        foreach (Player.PlayerBonusStat bonusStat in playerBonusStats)
        {
            if (bonusStat.bonusStat == statEnum)
            {
                switch (bonusStat.bonusStatType)
                {
                    case Player.BonusStatType.Fixed:
                        result += bonusStat.value;
                        break;
                    case Player.BonusStatType.Percentage:
                        result *= bonusStat.value;
                        break;
                }
            }
        }
        return result;
    }

    public void RemoveBuffEffect(Player.PlayerBuffEffect buffEffect)
    {
        playerBuffEffects.Remove(buffEffect);
        playerBonusStats.Remove(playerBonusStats.FirstOrDefault(x => x.playerBuffEffect.Equals(buffEffect)));
    }

    public void ChangeValueForDuration(Action<float> setter, Func<float> getter, float changeValue, float duration)
    {
        StartCoroutine(ChangeValueForDurationCoroutine(setter, getter, changeValue, duration));
    }

    private IEnumerator ChangeValueForDurationCoroutine(Action<float> setter, Func<float> getter, float changeValue, float duration)
    {
        setter(getter() + changeValue);
        yield return new WaitForSeconds(duration);
        setter(getter() - changeValue);
    }

    public void AddExperience(float experience)
    {
        // ����ġ �߰�
        playerExperience += experience * playerExperienceMultiplier;
        Debug.Log($"[GameManager] ����ġ {experience} �߰���. ���� ����ġ: {playerExperience}/{experienceToLevelUp}");

        // �߿�: ������ üũ ���� ������ ó�� ������ Ȯ��
        if (!isProcessingLevelUp && playerExperience >= experienceToLevelUp)
        {
            Debug.Log($"[GameManager] ������ ���� ����: {playerExperience} >= {experienceToLevelUp}");
            ProcessLevelUp();
        }
    }

    // ������ ó���� �����ϴ� �޼���
    private void ProcessLevelUp()
    {
        if (isProcessingLevelUp)
        {
            Debug.Log("[GameManager] �̹� ������ ó�� ���Դϴ�.");
            return;
        }

        isProcessingLevelUp = true;
        Debug.Log("[GameManager] ������ ó�� ����");

        // �� ���� �� ������ ���
        float expToUse = experienceToLevelUp;
        playerExperience -= expToUse;

        // ���� ����
        long previousLevel = playerLevel;
        playerLevel = playerLevel + 1;

        Debug.Log($"[GameManager] ���� ����: {previousLevel} -> {playerLevel}");

        // ������ ���� ����ġ �䱸�� ����
        switch (playerLevel)
        {
            case 2:
                experienceToLevelUp = 300;
                break;
            case 3:
                experienceToLevelUp = 1000;
                break;
            default:
                experienceToLevelUp += 1000;
                break;
        }

        Debug.Log($"[GameManager] ���� �������� �ʿ� ����ġ: {experienceToLevelUp}");

        // �߿�: ������ �̺�Ʈ�� �������� �߻����� UI�� ǥ�õǵ��� ��
        if (onPlayerLevelChanged != null)
        {
            Debug.Log($"[GameManager] onPlayerLevelChanged �̺�Ʈ ȣ��. ������ ��: {onPlayerLevelChanged?.GetInvocationList()?.Length ?? 0}");
            onPlayerLevelChanged.Invoke();
        }
        else
        {
            Debug.LogError("[GameManager] onPlayerLevelChanged is null!");
            isProcessingLevelUp = false;
        }
    }

    private void LevelUp()
    {
        Debug.Log($"[GameManager] LevelUp ����: ���� ����ġ={playerExperience}/{experienceToLevelUp}, ���� ����={playerLevel}");

        // �߿�: ���� ������ �ǳʶ��� �ʰ� �� ���� �� ������ ������Ŵ
        float requiredExp = experienceToLevelUp;
        playerExperience -= requiredExp;
        long previousLevel = playerLevel;
        playerLevel = playerLevel + 1;

        Debug.Log($"[GameManager] ���� ����: {previousLevel} -> {playerLevel}");

        // ������ ���� ����ġ �䱸�� ����
        switch (playerLevel)
        {
            case 2:
                experienceToLevelUp = 300;
                break;
            case 3:
                experienceToLevelUp = 1000;
                break;
            default:
                experienceToLevelUp += 1000;
                break;
        }

        Debug.Log($"[GameManager] ���� �������� �ʿ� ����ġ: {experienceToLevelUp}");

        // ���� ����ġ�� �߰� ������ �������� üũ
        if (playerExperience >= experienceToLevelUp)
        {
            Debug.Log($"[GameManager] �߰� ������ ����: ���� ����ġ {playerExperience} >= �ʿ� ����ġ {experienceToLevelUp}");
            // ���� �������� ��⿭�� �߰�
            QueueNextLevelUp();
        }
    }

    private void QueueNextLevelUp()
    {
        // ��⿭�� ���� ������ üũ �۾� �߰�
        levelUpQueue.Enqueue(() => {
            if (playerExperience >= experienceToLevelUp)
            {
                Debug.Log($"[GameManager] ��⿭���� ���� ������ ó��");
                LevelUp();
            }
            else
            {
                isProcessingLevelUp = false;
            }
        });

        // ���� �۾� ó�� ����
        ProcessNextLevelUp();
    }

    private void ProcessNextLevelUp()
    {
        if (levelUpQueue.Count > 0)
        {
            Debug.Log($"[GameManager] ������ ��⿭ ũ��: {levelUpQueue.Count}");
            // LevelUpUI�� ���� �� ȣ��� �ݹ� ���
            LevelUpUI levelUpUI = FindAnyObjectByType<LevelUpUI>();
            if (levelUpUI != null)
            {
                levelUpUI.onPanelClosed += OnLevelUpPanelClosed;
            }
            else
            {
                // UI ã�� �� ������ �ٷ� ���� ó��
                OnLevelUpPanelClosed();
            }
        }
    }

    // LevelUpUI �г��� ������ �� ȣ��� �ݹ�
    public void OnLevelUpPanelClosed()
    {
        Debug.Log("[GameManager] OnLevelUpPanelClosed ȣ���");

        // ��⿭���� ���� �۾� ������ ����
        if (levelUpQueue.Count > 0)
        {
            Action nextAction = levelUpQueue.Dequeue();
            nextAction?.Invoke();
        }
        else
        {
            // �߿�: �� �κ��� ���� ����Ǿ�� �� - ���� �������� ��� ó���� �� �ֵ���
            isProcessingLevelUp = false;

            // ���� ����ġ�� �߰� ������ �������� Ȯ��
            if (playerExperience >= experienceToLevelUp)
            {
                Debug.Log($"[GameManager] �г� ���� �� �߰� ������ Ȯ��: {playerExperience} >= {experienceToLevelUp}");
                // ���� ProcessLevelUp ȣ��
                ProcessLevelUp();
            }
        }
    }
}