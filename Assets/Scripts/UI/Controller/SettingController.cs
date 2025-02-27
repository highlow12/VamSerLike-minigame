using UnityEngine;
using UnityEngine.UI;
using System;

public class SettingsController : MonoBehaviour
{
    [Header("Toggle Components")]
    [SerializeField] private Toggle effectSoundToggle;
    [SerializeField] private Toggle backgroundMusicToggle;
    [SerializeField] private Toggle vibrationToggle;

    [Header("Toggle Images")]
    [SerializeField] private Image effectSoundImage;
    [SerializeField] private Image backgroundMusicImage;
    [SerializeField] private Image vibrationImage;

    [Header("Toggle Sprites")]
    [SerializeField] private Sprite toggleOnSprite;
    [SerializeField] private Sprite toggleOffSprite;

    [Header("Quit Game")]
    [SerializeField] private Button quitButton;

    // PlayerPrefs Keys
    private const string EFFECT_SOUND_KEY = "EffectSound";
    private const string BACKGROUND_MUSIC_KEY = "BackgroundMusic";
    private const string VIBRATION_KEY = "Vibration";

    private void Start()
    {
        InitializeToggles();
        SetupToggleListeners();
        SetupQuitButton();
    }

    private void InitializeToggles()
    {
        // 저장된 설정 불러오기
        effectSoundToggle.isOn = PlayerPrefs.GetInt(EFFECT_SOUND_KEY, 1) == 1;
        backgroundMusicToggle.isOn = PlayerPrefs.GetInt(BACKGROUND_MUSIC_KEY, 1) == 1;
        vibrationToggle.isOn = PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;

        // 초기 이미지 설정
        UpdateToggleImage(effectSoundImage, effectSoundToggle.isOn);
        UpdateToggleImage(backgroundMusicImage, backgroundMusicToggle.isOn);
        UpdateToggleImage(vibrationImage, vibrationToggle.isOn);
    }

    private void SetupToggleListeners()
    {
        effectSoundToggle.onValueChanged.AddListener((value) =>
        {
            UpdateToggleImage(effectSoundImage, value);
            PlayerPrefs.SetInt(EFFECT_SOUND_KEY, value ? 1 : 0);
            ApplyEffectSoundSetting(value);
        });

        backgroundMusicToggle.onValueChanged.AddListener((value) =>
        {
            UpdateToggleImage(backgroundMusicImage, value);
            PlayerPrefs.SetInt(BACKGROUND_MUSIC_KEY, value ? 1 : 0);
            ApplyBackgroundMusicSetting(value);
        });

        vibrationToggle.onValueChanged.AddListener((value) =>
        {
            UpdateToggleImage(vibrationImage, value);
            PlayerPrefs.SetInt(VIBRATION_KEY, value ? 1 : 0);
            ApplyVibrationSetting(value);
        });
    }

    private void UpdateToggleImage(Image toggleImage, bool isOn)
    {
        toggleImage.sprite = isOn ? toggleOnSprite : toggleOffSprite;
    }

    // 효과음 설정 적용
    private void ApplyEffectSoundSetting(bool isOn)
    {
        // AudioManager 등을 통해 효과음 설정 적용
        Debug.Log($"효과음 {(isOn ? "켜짐" : "꺼짐")}");
    }

    // 배경음악 설정 적용
    private void ApplyBackgroundMusicSetting(bool isOn)
    {
        // AudioManager 등을 통해 배경음악 설정 적용
        Debug.Log($"배경음악 {(isOn ? "켜짐" : "꺼짐")}");
    }

    // 진동 설정 적용
    private void ApplyVibrationSetting(bool isOn)
    {
        if (isOn)
        {
            // 진동 활성화
#if UNITY_ANDROID
                Handheld.Vibrate();
#endif
        }
        Debug.Log($"진동 {(isOn ? "켜짐" : "꺼짐")}");
    }

    private void SetupQuitButton()
    {
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("게임 종료");
    }
}