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

    // 레벨업 이벤트 수정 - 이벤트를 null 체크 없이 호출할 수 있도록 함
    // 델리게이트 정의
    public delegate void OnPlayerLevelChanged();
    // 이벤트 초기화하여 null이 되지 않도록 함
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

    // playerLevel 프로퍼티 수정
    public long playerLevel
    {
        get => _playerLevel;
        set
        {
            Debug.Log($"[GameManager] 레벨 변경: {_playerLevel} -> {value}");
            _playerLevel = value;

            // value > 1 체크는 제거하고 항상 이벤트 발생
            Debug.Log($"[GameManager] onPlayerLevelChanged 이벤트 호출");
            // null 체크가 필요 없음 - 빈 델리게이트로 초기화했기 때문
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

    // 레벨업 대기열 관리를 위한 변수 추가
    public bool isProcessingLevelUp { get; private set; } = false;
    private Queue<Action> levelUpQueue = new Queue<Action>();

    public override void Awake()
    {
        base.Awake();
        Debug.Log("[GameManager] Awake 호출됨");

        // 테스트를 위해 레벨업 이벤트 기본 구독자 추가
        onPlayerLevelChanged += () => { Debug.Log("[GameManager] 레벨업 이벤트 기본 처리기 호출됨"); };

        onGamePaused += () =>
        {
            Time.timeScale = IsGamePaused ? 0 : 1;
            Debug.Log($"[GameManager] Time.timeScale 설정됨: {Time.timeScale}");
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
        // 경험치 추가
        playerExperience += experience * playerExperienceMultiplier;
        Debug.Log($"[GameManager] 경험치 {experience} 추가됨. 현재 경험치: {playerExperience}/{experienceToLevelUp}");

        // 중요: 레벨업 체크 전에 레벨업 처리 중인지 확인
        if (!isProcessingLevelUp && playerExperience >= experienceToLevelUp)
        {
            Debug.Log($"[GameManager] 레벨업 조건 충족: {playerExperience} >= {experienceToLevelUp}");
            ProcessLevelUp();
        }
    }

    // 레벨업 처리를 시작하는 메서드
    private void ProcessLevelUp()
    {
        if (isProcessingLevelUp)
        {
            Debug.Log("[GameManager] 이미 레벨업 처리 중입니다.");
            return;
        }

        isProcessingLevelUp = true;
        Debug.Log("[GameManager] 레벨업 처리 시작");

        // 한 번에 한 레벨만 상승
        float expToUse = experienceToLevelUp;
        playerExperience -= expToUse;

        // 레벨 증가
        long previousLevel = playerLevel;
        playerLevel = playerLevel + 1;

        Debug.Log($"[GameManager] 레벨 변경: {previousLevel} -> {playerLevel}");

        // 레벨에 따른 경험치 요구량 설정
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

        Debug.Log($"[GameManager] 다음 레벨까지 필요 경험치: {experienceToLevelUp}");

        // 중요: 레벨업 이벤트를 마지막에 발생시켜 UI가 표시되도록 함
        if (onPlayerLevelChanged != null)
        {
            Debug.Log($"[GameManager] onPlayerLevelChanged 이벤트 호출. 구독자 수: {onPlayerLevelChanged?.GetInvocationList()?.Length ?? 0}");
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
        Debug.Log($"[GameManager] LevelUp 시작: 현재 경험치={playerExperience}/{experienceToLevelUp}, 현재 레벨={playerLevel}");

        // 중요: 여러 레벨을 건너뛰지 않고 한 번에 한 레벨씩 증가시킴
        float requiredExp = experienceToLevelUp;
        playerExperience -= requiredExp;
        long previousLevel = playerLevel;
        playerLevel = playerLevel + 1;

        Debug.Log($"[GameManager] 레벨 변경: {previousLevel} -> {playerLevel}");

        // 레벨에 따른 경험치 요구량 설정
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

        Debug.Log($"[GameManager] 다음 레벨까지 필요 경험치: {experienceToLevelUp}");

        // 남은 경험치로 추가 레벨업 가능한지 체크
        if (playerExperience >= experienceToLevelUp)
        {
            Debug.Log($"[GameManager] 추가 레벨업 가능: 남은 경험치 {playerExperience} >= 필요 경험치 {experienceToLevelUp}");
            // 다음 레벨업을 대기열에 추가
            QueueNextLevelUp();
        }
    }

    private void QueueNextLevelUp()
    {
        // 대기열에 다음 레벨업 체크 작업 추가
        levelUpQueue.Enqueue(() => {
            if (playerExperience >= experienceToLevelUp)
            {
                Debug.Log($"[GameManager] 대기열에서 다음 레벨업 처리");
                LevelUp();
            }
            else
            {
                isProcessingLevelUp = false;
            }
        });

        // 다음 작업 처리 시작
        ProcessNextLevelUp();
    }

    private void ProcessNextLevelUp()
    {
        if (levelUpQueue.Count > 0)
        {
            Debug.Log($"[GameManager] 레벨업 대기열 크기: {levelUpQueue.Count}");
            // LevelUpUI가 닫힐 때 호출될 콜백 등록
            LevelUpUI levelUpUI = FindAnyObjectByType<LevelUpUI>();
            if (levelUpUI != null)
            {
                levelUpUI.onPanelClosed += OnLevelUpPanelClosed;
            }
            else
            {
                // UI 찾을 수 없으면 바로 다음 처리
                OnLevelUpPanelClosed();
            }
        }
    }

    // LevelUpUI 패널이 닫혔을 때 호출될 콜백
    public void OnLevelUpPanelClosed()
    {
        Debug.Log("[GameManager] OnLevelUpPanelClosed 호출됨");

        // 대기열에서 다음 작업 가져와 실행
        if (levelUpQueue.Count > 0)
        {
            Action nextAction = levelUpQueue.Dequeue();
            nextAction?.Invoke();
        }
        else
        {
            // 중요: 이 부분이 먼저 실행되어야 함 - 다음 레벨업이 즉시 처리될 수 있도록
            isProcessingLevelUp = false;

            // 남은 경험치로 추가 레벨업 가능한지 확인
            if (playerExperience >= experienceToLevelUp)
            {
                Debug.Log($"[GameManager] 패널 닫힌 후 추가 레벨업 확인: {playerExperience} >= {experienceToLevelUp}");
                // 직접 ProcessLevelUp 호출
                ProcessLevelUp();
            }
        }
    }
}