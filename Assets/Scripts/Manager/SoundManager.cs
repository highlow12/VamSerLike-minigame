using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool; // 유니티 내장 풀 네임스페이스 추가

public class SoundManager : Singleton<SoundManager>
{
    [System.Serializable]
    public class SoundGroup
    {
        public string groupName;
        public int maxConcurrentSounds = 3; // 동시에 재생 가능한 최대 사운드 수
        public float minTimeBetweenSounds = 0.1f; // 최소 발음 간격
        [HideInInspector] public List<AudioSource> activeSources = new List<AudioSource>();
        [HideInInspector] public float lastPlayedTime = 0f;
    }

    [Header("오디오 믹서 설정")]
    public AudioMixer audioMixer;
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup uiMixerGroup;

    [Header("풀링 설정")]
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 50;
    [SerializeField] private Transform audioSourcesParent;

    [Header("사운드 제한 그룹")]
    [SerializeField] private List<SoundGroup> soundGroups = new List<SoundGroup>();
    [Header("배경음악")]
    public AudioClip bgmClip;

    // 유니티 내장 오브젝트 풀 사용
    private IObjectPool<AudioSource> audioSourcePool;
    private Dictionary<int, AudioSource> activeAudioSources = new Dictionary<int, AudioSource>();
    
    // 현재 재생중인 BGM
    private AudioSource currentBGM;
    private int currentSoundID = 0;

    public override void Awake()
    {
        base.Awake();
        InitializePool();
        InitializeMixer();
    }

    private void Start()
    {
        // 발걸음 소리를 위한 사운드 그룹 추가 (최대 8개 동시 발음, 최소 0.1초 간격)
        AddSoundGroup("Footstep", 8, 0.1f);
        PlayMusic(bgmClip);
    }

    private void InitializePool()
    {
        // 유니티 내장 오브젝트 풀 초기화
        audioSourcePool = new ObjectPool<AudioSource>(
            CreateAudioSource,
            OnTakeFromPool,
            OnReturnToPool,
            OnDestroyAudioSource,
            true,         // Collection checks
            initialPoolSize,
            maxPoolSize
        );
        
        if (audioSourcesParent == null)
        {
            GameObject poolParent = new GameObject("AudioSourcePool");
            poolParent.transform.SetParent(this.transform);
            audioSourcesParent = poolParent.transform;
        }
    }

    private void InitializeMixer()
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("오디오 믹서가 설정되지 않았습니다. 볼륨 조절 기능이 제한됩니다.");
        }
        
        // 저장된 볼륨 설정 불러오기
        LoadVolumeSettings();
    }

    private void LoadVolumeSettings()
    {
        // PlayerPrefs 등에서 저장된 볼륨 설정을 로드
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        float uiVolume = PlayerPrefs.GetFloat("UIVolume", 0.75f);
        
        // 볼륨 설정 적용
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
        SetUIVolume(uiVolume);
    }

    private void SaveVolumeSettings(string name, float volume)
    {
        try
        {
            PlayerPrefs.SetFloat(name, volume);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save PlayerPrefs: " + e.Message);
        }
    }

    // 볼륨 조절 메서드들
    public void SetMasterVolume(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("Master", ConvertToDecibel(volume));
        SaveVolumeSettings("Master", volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("Music", ConvertToDecibel(volume));
        SaveVolumeSettings("Music", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("SFX", ConvertToDecibel(volume));
        SaveVolumeSettings("SFX", volume);
    }

    public void SetUIVolume(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("UI", ConvertToDecibel(volume));
        SaveVolumeSettings("UI", volume);
    }

    // 볼륨값을 데시벨로 변환 (0-1 사이의 값을 -80dB ~ 0dB로 변환)
    private float ConvertToDecibel(float volume)
    {
        return volume == 0 ? -80f : Mathf.Log10(volume) * 20;
    }

    // 오브젝트 풀 콜백 함수들 - 유니티 내장 Object Pool 사용
    private AudioSource CreateAudioSource()
    {
        GameObject audioSourceObj = new GameObject("AudioSource");
        audioSourceObj.transform.SetParent(audioSourcesParent);
        AudioSource audioSource = audioSourceObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        return audioSource;
    }

    private void OnTakeFromPool(AudioSource audioSource)
    {
        audioSource.gameObject.SetActive(true);
    }

    private void OnReturnToPool(AudioSource audioSource)
    {
        // 사운드 그룹에서 제거
        foreach (var group in soundGroups)
        {
            group.activeSources.Remove(audioSource);
        }
        
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.gameObject.SetActive(false);
        audioSource.spatialBlend = 0f; // 2D 모드로 초기화
        audioSource.outputAudioMixerGroup = null;
    }

    private void OnDestroyAudioSource(AudioSource audioSource)
    {
        Destroy(audioSource.gameObject);
    }

    // 사운드 ID로 사운드 반환
    public void ReturnAudioSource(int soundID)
    {
        if (activeAudioSources.TryGetValue(soundID, out AudioSource audioSource))
        {
            audioSourcePool.Release(audioSource);
            activeAudioSources.Remove(soundID);
        }
    }

    // 모든 활성 오디오 소스 정리
    public void StopAllSounds()
    {
        List<int> soundIDsToStop = new List<int>(activeAudioSources.Keys);
        
        foreach (int soundID in soundIDsToStop)
        {
            ReturnAudioSource(soundID);
        }
    }

    // 효과음 재생 (2D)
    public int PlaySFX(AudioClip clip, float volume = 1.0f, string groupName = "", bool loop = false)
    {
        if (clip == null) return -1;
        
        // 그룹이 있는 경우 재생 제한 확인
        if (!string.IsNullOrEmpty(groupName))
        {
            SoundGroup group = soundGroups.FirstOrDefault(g => g.groupName == groupName);
            
            if (group != null)
            {
                // 동시 재생 수 제한 체크
                if (group.activeSources.Count >= group.maxConcurrentSounds)
                {
                    // 가장 오래된 사운드 정지
                    if (group.activeSources.Count > 0)
                    {
                        AudioSource oldestSource = group.activeSources[0];
                        int oldSoundID = activeAudioSources.FirstOrDefault(x => x.Value == oldestSource).Key;
                        ReturnAudioSource(oldSoundID);
                    }
                }
                
                // 시간 간격 제한 체크
                if (Time.time - group.lastPlayedTime < group.minTimeBetweenSounds)
                {
                    return -1; // 재생 간격이 너무 짧음
                }
                
                group.lastPlayedTime = Time.time;
            }
        }
        
        // 풀에서 오디오소스 가져오기
        AudioSource audioSource = audioSourcePool.Get();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.spatialBlend = 0f; // 2D 사운드
        
        // 믹서 그룹 설정
        if (sfxMixerGroup != null)
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        
        audioSource.Play();
        
        int soundID = currentSoundID++;
        activeAudioSources.Add(soundID, audioSource);
        
        // 그룹에 추가
        if (!string.IsNullOrEmpty(groupName))
        {
            SoundGroup group = soundGroups.FirstOrDefault(g => g.groupName == groupName);
            if (group != null)
            {
                group.activeSources.Add(audioSource);
            }
        }
        
        // 루핑 아닌 경우 자동 반환
        if (!loop)
        {
            StartCoroutine(AutoReturnRoutine(soundID, clip.length));
        }
        
        return soundID;
    }

    // 효과음 재생 (3D 위치)
    public int PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1.0f, float minDistance = 1f, float maxDistance = 20f, string groupName = "", bool loop = false)
    {
        if (clip == null) return -1;
        
        // 사운드 그룹 제한 확인 (2D 사운드와 동일)
        if (!string.IsNullOrEmpty(groupName))
        {
            SoundGroup group = soundGroups.FirstOrDefault(g => g.groupName == groupName);
            
            if (group != null)
            {
                // 동시 재생 수 제한 체크
                if (group.activeSources.Count >= group.maxConcurrentSounds)
                {
                    // 가장 오래된 사운드 정지
                    if (group.activeSources.Count > 0)
                    {
                        AudioSource oldestSource = group.activeSources[0];
                        int oldSoundID = activeAudioSources.FirstOrDefault(x => x.Value == oldestSource).Key;
                        ReturnAudioSource(oldSoundID);
                    }
                }
                
                // 시간 간격 제한 체크
                if (Time.time - group.lastPlayedTime < group.minTimeBetweenSounds)
                {
                    return -1; // 재생 간격이 너무 짧음
                }
                
                group.lastPlayedTime = Time.time;
            }
        }
        
        // 풀에서 오디오소스 가져오기
        AudioSource audioSource = audioSourcePool.Get();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.spatialBlend = 1f; // 3D 사운드
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.transform.position = position;
        
        // 믹서 그룹 설정
        if (sfxMixerGroup != null)
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        
        audioSource.Play();
        
        int soundID = currentSoundID++;
        activeAudioSources.Add(soundID, audioSource);
        
        // 그룹에 추가
        if (!string.IsNullOrEmpty(groupName))
        {
            SoundGroup group = soundGroups.FirstOrDefault(g => g.groupName == groupName);
            if (group != null)
            {
                group.activeSources.Add(audioSource);
            }
        }
        
        // 루핑 아닌 경우 자동 반환
        if (!loop)
        {
            StartCoroutine(AutoReturnRoutine(soundID, clip.length));
        }
        
        return soundID;
    }

    // 배경음악 재생
    public int PlayMusic(AudioClip clip, float volume = 1.0f, bool loop = true, float fadeInTime = 1.0f)
    {
        if (clip == null) return -1;
        
        // 현재 재생 중인 BGM이 있으면 페이드 아웃
        if (currentBGM != null && currentBGM.isPlaying)
        {
            StartCoroutine(FadeOutAndStop(currentBGM, 0.5f));
        }
        
        // 풀에서 오디오소스 가져오기
        AudioSource audioSource = audioSourcePool.Get();
        audioSource.clip = clip;
        audioSource.volume = fadeInTime > 0 ? 0f : volume;
        audioSource.loop = loop;
        audioSource.spatialBlend = 0f; // 2D 사운드
        
        // 믹서 그룹 설정
        if (musicMixerGroup != null)
            audioSource.outputAudioMixerGroup = musicMixerGroup;
        
        audioSource.Play();
        
        // 페이드인 적용
        if (fadeInTime > 0)
        {
            StartCoroutine(FadeIn(audioSource, volume, fadeInTime));
        }
        
        currentBGM = audioSource;
        
        int soundID = currentSoundID++;
        activeAudioSources.Add(soundID, audioSource);
        
        return soundID;
    }
    
    // UI 사운드 재생
    public int PlayUISound(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return -1;
        
        // 풀에서 오디오소스 가져오기
        AudioSource audioSource = audioSourcePool.Get();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 2D 사운드
        
        // 믹서 그룹 설정
        if (uiMixerGroup != null)
            audioSource.outputAudioMixerGroup = uiMixerGroup;
        
        audioSource.Play();
        
        int soundID = currentSoundID++;
        activeAudioSources.Add(soundID, audioSource);
        
        // 자동 반환
        StartCoroutine(AutoReturnRoutine(soundID, clip.length));
        
        return soundID;
    }
    
    // 특정 오디오 소스를 중지
    public void StopSound(int soundID)
    {
        if (activeAudioSources.TryGetValue(soundID, out AudioSource audioSource))
        {
            ReturnAudioSource(soundID);
        }
    }

    // 페이드 인 코루틴
    private System.Collections.IEnumerator FadeIn(AudioSource audioSource, float targetVolume, float fadeTime)
    {
        float startVolume = 0f;
        float timer = 0f;
        
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeTime);
            yield return null;
        }
        
        audioSource.volume = targetVolume;
    }
    
    // 페이드 아웃 후 중지 코루틴
    private System.Collections.IEnumerator FadeOutAndStop(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;
        
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            yield return null;
        }
        
        audioSource.Stop();
        
        // 해당 오디오 소스를 풀에 반환
        int soundID = activeAudioSources.FirstOrDefault(x => x.Value == audioSource).Key;
        if (soundID != 0 && activeAudioSources.ContainsKey(soundID)) // Ensure soundID is valid and still active
        {
            ReturnAudioSource(soundID);
        }
    }
    
    // 자동 반환 코루틴
    private System.Collections.IEnumerator AutoReturnRoutine(int soundID, float clipLength)
    {
        // 약간의 여유를 주고 반환
        yield return new WaitForSeconds(clipLength + 0.1f);
        
        ReturnAudioSource(soundID);
    }
    
    // 재생 중인 사운드 수정 (볼륨, 위치 등)
    public bool ModifySound(int soundID, float? volume = null, Vector3? position = null)
    {
        if (activeAudioSources.TryGetValue(soundID, out AudioSource audioSource))
        {
            if (volume.HasValue)
                audioSource.volume = volume.Value;
            
            if (position.HasValue)
                audioSource.transform.position = position.Value;
            
            return true;
        }
        
        return false;
    }
    
    // 음소거 설정
    public void SetMute(bool isMuted)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MasterVolume", isMuted ? -80f : ConvertToDecibel(PlayerPrefs.GetFloat("MasterVolume", 0.75f)));
        }
    }

    // 그룹 추가
    public void AddSoundGroup(string groupName, int maxConcurrentSounds, float minTimeBetweenSounds)
    {
        if (soundGroups.Any(g => g.groupName == groupName))
        {
            Debug.LogWarning($"사운드 그룹 '{groupName}'이(가) 이미 존재합니다.");
            return;
        }
        
        SoundGroup newGroup = new SoundGroup
        {
            groupName = groupName,
            maxConcurrentSounds = maxConcurrentSounds,
            minTimeBetweenSounds = minTimeBetweenSounds,
            activeSources = new List<AudioSource>(),
            lastPlayedTime = 0f
        };
        
        soundGroups.Add(newGroup);
    }

    // 발걸음 소리와 같은 특정 효과음을 위한 헬퍼 메서드
    public int PlayFootstep(AudioClip clip, Vector3 position, float volume = 0.5f)
    {
        // "Footstep" 그룹으로 제한된 사운드 재생
        return PlaySFXAtPosition(clip, position, volume, 1f, 10f, "Footstep");
    }

    // 게임 중지 시 모든 사운드 일시 정지
    public void PauseAllSounds()
    {
        foreach (var source in activeAudioSources.Values)
        {
            source.Pause();
        }
    }

    // 게임 재개 시 모든 사운드 재개
    public void ResumeAllSounds()
    {
        foreach (var source in activeAudioSources.Values)
        {
            source.UnPause();
        }
    }
}
