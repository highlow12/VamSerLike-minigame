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
        // ����� ���� �ҷ�����
        effectSoundToggle.isOn = PlayerPrefs.GetInt(EFFECT_SOUND_KEY, 1) == 1;
        backgroundMusicToggle.isOn = PlayerPrefs.GetInt(BACKGROUND_MUSIC_KEY, 1) == 1;
        vibrationToggle.isOn = PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;

        // �ʱ� �̹��� ����
        UpdateToggleImage(effectSoundImage, effectSoundToggle.isOn);
        UpdateToggleImage(backgroundMusicImage, backgroundMusicToggle.isOn);
        UpdateToggleImage(vibrationImage, vibrationToggle.isOn);
    }

    private void SetupToggleListeners()
    {
        effectSoundToggle.onValueChanged.AddListener((value) => {
            UpdateToggleImage(effectSoundImage, value);
            PlayerPrefs.SetInt(EFFECT_SOUND_KEY, value ? 1 : 0);
            ApplyEffectSoundSetting(value);
        });

        backgroundMusicToggle.onValueChanged.AddListener((value) => {
            UpdateToggleImage(backgroundMusicImage, value);
            PlayerPrefs.SetInt(BACKGROUND_MUSIC_KEY, value ? 1 : 0);
            ApplyBackgroundMusicSetting(value);
        });

        vibrationToggle.onValueChanged.AddListener((value) => {
            UpdateToggleImage(vibrationImage, value);
            PlayerPrefs.SetInt(VIBRATION_KEY, value ? 1 : 0);
            ApplyVibrationSetting(value);
        });
    }

    private void UpdateToggleImage(Image toggleImage, bool isOn)
    {
        toggleImage.sprite = isOn ? toggleOnSprite : toggleOffSprite;
    }

    // ȿ���� ���� ����
    private void ApplyEffectSoundSetting(bool isOn)
    {
        // AudioManager ���� ���� ȿ���� ���� ����
        Debug.Log($"ȿ���� {(isOn ? "����" : "����")}");
    }

    // ������� ���� ����
    private void ApplyBackgroundMusicSetting(bool isOn)
    {
        // AudioManager ���� ���� ������� ���� ����
        Debug.Log($"������� {(isOn ? "����" : "����")}");
    }

    // ���� ���� ����
    private void ApplyVibrationSetting(bool isOn)
    {
        if (isOn)
        {
            // ���� Ȱ��ȭ
#if UNITY_ANDROID
                Handheld.Vibrate();
#endif
        }
        Debug.Log($"���� {(isOn ? "����" : "����")}");
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
        Debug.Log("���� ����");
    }
}