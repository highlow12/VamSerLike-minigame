using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// OnGUI를 사용한 사운드 매니저 테스트 클래스
/// </summary>
public class SoundManagerTest : MonoBehaviour
{
    [Header("오디오 클립")]
    [SerializeField] private AudioClip[] sfxClips;         // 효과음 클립 배열
    [SerializeField] private AudioClip[] musicClips;       // 배경음악 클립 배열
    [SerializeField] private AudioClip[] uiSoundClips;     // UI 사운드 클립 배열
    [SerializeField] private AudioClip[] footstepClips;    // 발자국 소리 클립 배열

    // GUI 상태 변수들
    private float masterVolume = 0.75f;
    private float sfxVolume = 0.75f;
    private float musicVolume = 0.75f;
    private float uiVolume = 0.75f;
    
    private int currentSfxIndex = 0;
    private int currentMusicIndex = 0;
    private int currentUiSoundIndex = 0;
    private int currentFootstepIndex = 0;
    
    private int activeMusicID = -1;
    private string statusMessage = "사운드 매니저 테스트를 시작합니다.";
    private Vector2 scrollPosition = Vector2.zero;
    
    // GUI 스타일 변수
    private GUIStyle titleStyle;
    private GUIStyle headerStyle;
    private GUIStyle buttonStyle;
    private GUIStyle statusStyle;
    private GUIStyle sliderLabelStyle;

    private void Start()
    {
        // PlayerPrefs에서 볼륨 설정 로드
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        uiVolume = PlayerPrefs.GetFloat("UIVolume", 0.75f);
        
        // 볼륨 값을 사운드 매니저에 적용
        SoundManager.Instance.SetMasterVolume(masterVolume);
        SoundManager.Instance.SetSFXVolume(sfxVolume);
        SoundManager.Instance.SetMusicVolume(musicVolume);
        SoundManager.Instance.SetUIVolume(uiVolume);
    }

    private void InitializeStyles()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 24;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = Color.white;
            titleStyle.margin = new RectOffset(10, 10, 10, 20);
        }
        
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 18;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = new Color(0.9f, 0.9f, 0.2f);
            headerStyle.margin = new RectOffset(10, 10, 10, 10);
        }
        
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.padding = new RectOffset(10, 10, 5, 5);
            buttonStyle.margin = new RectOffset(5, 5, 5, 5);
            buttonStyle.fixedHeight = 30;
        }
        
        if (statusStyle == null)
        {
            statusStyle = new GUIStyle(GUI.skin.box);
            statusStyle.fontSize = 14;
            statusStyle.alignment = TextAnchor.MiddleLeft;
            statusStyle.padding = new RectOffset(10, 10, 10, 10);
            statusStyle.normal.textColor = Color.white;
            statusStyle.wordWrap = true;
        }
        
        if (sliderLabelStyle == null)
        {
            sliderLabelStyle = new GUIStyle(GUI.skin.label);
            sliderLabelStyle.fontSize = 14;
            sliderLabelStyle.alignment = TextAnchor.MiddleLeft;
            sliderLabelStyle.margin = new RectOffset(5, 5, 5, 5);
        }
    }

    private void OnGUI()
    {
        InitializeStyles();
        
        // 화면 가운데 정렬을 위한 좌표 계산
        float panelWidth = Mathf.Min(600, Screen.width - 40);
        float panelHeight = Screen.height - 40;
        Rect windowRect = new Rect((Screen.width - panelWidth) * 0.5f, 20, panelWidth, panelHeight);
        
        GUILayout.BeginArea(windowRect);
        
        // 제목
        GUILayout.Label("사운드 매니저 테스트", titleStyle);
        
        // 스크롤 뷰 시작
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(panelWidth), GUILayout.Height(panelHeight - 40));
        
        // 효과음 섹션
        GUILayout.Label("효과음 테스트", headerStyle);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("2D 효과음 재생", buttonStyle))
        {
            PlaySFX();
        }
        if (GUILayout.Button("3D 효과음 재생", buttonStyle))
        {
            PlaySFX3D();
        }
        GUILayout.EndHorizontal();
        
        // 배경음악 섹션
        GUILayout.Label("배경음악 테스트", headerStyle);
        if (GUILayout.Button("배경음악 재생/전환", buttonStyle))
        {
            PlayMusic();
        }
        
        // UI 사운드 섹션
        GUILayout.Label("UI 사운드 테스트", headerStyle);
        if (GUILayout.Button("UI 사운드 재생", buttonStyle))
        {
            PlayUISound();
        }
        
        // 발자국 소리 섹션
        GUILayout.Label("발자국 소리 테스트 (동시 발음 수 제한)", headerStyle);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("발자국 소리 재생", buttonStyle))
        {
            PlayFootstep();
        }
        if (GUILayout.Button("5회 빠르게 재생", buttonStyle))
        {
            // 발자국 소리 5회 연속 재생 (동시 발음 제한 확인)
            for (int i = 0; i < 5; i++)
            {
                PlayFootstep();
            }
        }
        GUILayout.EndHorizontal();
        
        // 모든 사운드 정지 버튼
        GUILayout.Space(10);
        if (GUILayout.Button("모든 사운드 정지", buttonStyle))
        {
            StopAllSounds();
        }
        
        // 볼륨 컨트롤 섹션
        GUILayout.Space(20);
        GUILayout.Label("볼륨 조절", headerStyle);
        
        // 마스터 볼륨
        GUILayout.BeginHorizontal();
        GUILayout.Label("마스터 볼륨:", sliderLabelStyle, GUILayout.Width(100));
        float newMasterVolume = GUILayout.HorizontalSlider(masterVolume, 0f, 1f, GUILayout.Width(panelWidth - 180));
        GUILayout.Label($"{masterVolume:F2}", sliderLabelStyle, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (newMasterVolume != masterVolume)
        {
            masterVolume = newMasterVolume;
            SetMasterVolume(masterVolume);
        }
        
        // 효과음 볼륨
        GUILayout.BeginHorizontal();
        GUILayout.Label("효과음 볼륨:", sliderLabelStyle, GUILayout.Width(100));
        float newSfxVolume = GUILayout.HorizontalSlider(sfxVolume, 0f, 1f, GUILayout.Width(panelWidth - 180));
        GUILayout.Label($"{sfxVolume:F2}", sliderLabelStyle, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (newSfxVolume != sfxVolume)
        {
            sfxVolume = newSfxVolume;
            SetSFXVolume(sfxVolume);
        }
        
        // 배경음악 볼륨
        GUILayout.BeginHorizontal();
        GUILayout.Label("음악 볼륨:", sliderLabelStyle, GUILayout.Width(100));
        float newMusicVolume = GUILayout.HorizontalSlider(musicVolume, 0f, 1f, GUILayout.Width(panelWidth - 180));
        GUILayout.Label($"{musicVolume:F2}", sliderLabelStyle, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (newMusicVolume != musicVolume)
        {
            musicVolume = newMusicVolume;
            SetMusicVolume(musicVolume);
        }
        
        // UI 사운드 볼륨
        GUILayout.BeginHorizontal();
        GUILayout.Label("UI 볼륨:", sliderLabelStyle, GUILayout.Width(100));
        float newUiVolume = GUILayout.HorizontalSlider(uiVolume, 0f, 1f, GUILayout.Width(panelWidth - 180));
        GUILayout.Label($"{uiVolume:F2}", sliderLabelStyle, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (newUiVolume != uiVolume)
        {
            uiVolume = newUiVolume;
            SetUIVolume(uiVolume);
        }
        
        // 상태 표시
        GUILayout.Space(20);
        GUILayout.Label("실행 상태", headerStyle);
        GUILayout.Box(statusMessage, statusStyle);
        
        // 스크롤 뷰 종료
        GUILayout.EndScrollView();
        
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// 2D 효과음 재생
    /// </summary>
    private void PlaySFX()
    {
        if (sfxClips == null || sfxClips.Length == 0)
        {
            UpdateStatus("효과음 클립이 없습니다!");
            return;
        }
            
        AudioClip clip = sfxClips[currentSfxIndex];
        int soundID = SoundManager.Instance.PlaySFX(clip, 1.0f);
            
        UpdateStatus($"효과음 재생: {clip.name} (ID: {soundID})");
            
        // 다음 클립으로 순환
        currentSfxIndex = (currentSfxIndex + 1) % sfxClips.Length;
    }
    
    /// <summary>
    /// 3D 위치 기반 효과음 재생
    /// </summary>
    private void PlaySFX3D()
    {
        if (sfxClips == null || sfxClips.Length == 0)
        {
            UpdateStatus("효과음 클립이 없습니다!");
            return;
        }
            
        AudioClip clip = sfxClips[currentSfxIndex];
        
        // 3D 사운드를 위한 랜덤 위치 생성
        Vector3 randomPosition = new Vector3(
            Random.Range(-5f, 5f),
            Random.Range(-5f, 5f),
            Random.Range(-5f, 5f)
        );
        
        int soundID = SoundManager.Instance.PlaySFXAtPosition(
            clip,
            randomPosition,
            1.0f,
            1.0f, // 최소 거리
            20.0f // 최대 거리
        );
            
        UpdateStatus($"3D 효과음 재생: {clip.name} (위치: {randomPosition}, ID: {soundID})");
            
        // 다음 클립으로 순환
        currentSfxIndex = (currentSfxIndex + 1) % sfxClips.Length;
    }
    
    /// <summary>
    /// 배경음악 재생
    /// </summary>
    private void PlayMusic()
    {
        if (musicClips == null || musicClips.Length == 0)
        {
            UpdateStatus("배경음악 클립이 없습니다!");
            return;
        }
            
        AudioClip clip = musicClips[currentMusicIndex];
        
        // 이전 음악 정지 없이 새 음악을 재생하면 자동으로 전환됨
        activeMusicID = SoundManager.Instance.PlayMusic(
            clip,
            musicVolume, // 현재 설정된 음악 볼륨 사용
            true, // 반복 재생
            1.0f  // 페이드 인 시간
        );
            
        UpdateStatus($"배경음악 재생: {clip.name} (ID: {activeMusicID})");
            
        // 다음 클립으로 순환
        currentMusicIndex = (currentMusicIndex + 1) % musicClips.Length;
    }
    
    /// <summary>
    /// UI 사운드 재생
    /// </summary>
    private void PlayUISound()
    {
        if (uiSoundClips == null || uiSoundClips.Length == 0)
        {
            UpdateStatus("UI 사운드 클립이 없습니다!");
            return;
        }
            
        AudioClip clip = uiSoundClips[currentUiSoundIndex];
        int soundID = SoundManager.Instance.PlayUISound(clip);
            
        UpdateStatus($"UI 사운드 재생: {clip.name} (ID: {soundID})");
            
        // 다음 클립으로 순환
        currentUiSoundIndex = (currentUiSoundIndex + 1) % uiSoundClips.Length;
    }
    
    /// <summary>
    /// 발자국 소리 재생 (동시 발음 수 제한 테스트)
    /// </summary>
    private void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0)
        {
            UpdateStatus("발자국 소리 클립이 없습니다!");
            return;
        }
            
        AudioClip clip = footstepClips[currentFootstepIndex];
        
        // 플레이어 위치를 중심으로 약간의 오프셋 적용
        Vector3 playerPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        Vector3 soundPosition = playerPosition + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        
        int soundID = SoundManager.Instance.PlayFootstep(clip, soundPosition);
            
        UpdateStatus($"발자국 소리 재생: {clip.name} (ID: {soundID})");
            
        // 다음 클립으로 순환
        currentFootstepIndex = (currentFootstepIndex + 1) % footstepClips.Length;
    }
    
    /// <summary>
    /// 모든 사운드 중지
    /// </summary>
    private void StopAllSounds()
    {
        SoundManager.Instance.StopAllSounds();
        activeMusicID = -1;
        UpdateStatus("모든 사운드가 중지되었습니다.");
    }
    
    /// <summary>
    /// 마스터 볼륨 설정
    /// </summary>
    private void SetMasterVolume(float volume)
    {
        SoundManager.Instance.SetMasterVolume(volume);
        UpdateStatus($"마스터 볼륨: {volume:F2}");
    }
    
    /// <summary>
    /// 효과음 볼륨 설정
    /// </summary>
    private void SetSFXVolume(float volume)
    {
        SoundManager.Instance.SetSFXVolume(volume);
        UpdateStatus($"효과음 볼륨: {volume:F2}");
    }
    
    /// <summary>
    /// 배경음악 볼륨 설정
    /// </summary>
    private void SetMusicVolume(float volume)
    {
        SoundManager.Instance.SetMusicVolume(volume);
        UpdateStatus($"배경음악 볼륨: {volume:F2}");
    }
    
    /// <summary>
    /// UI 사운드 볼륨 설정
    /// </summary>
    private void SetUIVolume(float volume)
    {
        SoundManager.Instance.SetUIVolume(volume);
        UpdateStatus($"UI 사운드 볼륨: {volume:F2}");
    }
    
    /// <summary>
    /// 상태 텍스트 업데이트
    /// </summary>
    private void UpdateStatus(string message)
    {
        statusMessage = message;
        
        // 디버그 로그에도 출력
        Debug.Log($"[사운드 테스트] {message}");
    }
}