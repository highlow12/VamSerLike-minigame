# 사운드 매니저 (SoundManager) 사용 설명서

## 1. 개요

`SoundManager`는 유니티 내장 오브젝트 풀을 활용한 효율적인 사운드 관리 시스템입니다. 2D/3D 효과음, 배경음악, UI 사운드 등을 관리하고 믹서를 통한 볼륨 제어 및 발자국 소리와 같은 특정 사운드의 동시 발음 수 제한 기능을 제공합니다.

## 2. 기능 요약

- 유니티 내장 오브젝트 풀을 사용한 효율적인 오디오 소스 관리
- 오디오 믹서를 통한 종류별(배경음악, 효과음, UI) 음량 제어
- 특정 사운드 그룹(예: 발자국)의 동시 재생 개수 및 재생 간격 제한
- 2D와 3D 위치 기반 사운드 재생
- 배경음악 페이드 인/아웃 기능
- 게임 일시정지 시 자동 사운드 일시정지/재개 기능

## 3. 초기 설정

### 3.1. 오디오 믹서 생성

1. 프로젝트 창에서 우클릭 후 **Create > Audio > Audio Mixer** 선택
2. 새로 생성된 오디오 믹서의 이름을 "GameAudioMixer"로 변경
3. 오디오 믹서를 더블클릭하여 Audio Mixer 창을 열기
4. "Master" 그룹에 우클릭하여 다음 그룹 추가:
   - "SFX" 그룹
   - "Music" 그룹
   - "UI" 그룹
5. 각 그룹에 볼륨 파라미터 이름 설정:
   - Master 그룹: "MasterVolume"
   - SFX 그룹: "SFXVolume"
   - Music 그룹: "MusicVolume"
   - UI 그룹: "UIVolume"

### 3.2. SoundManager 게임오브젝트 생성

1. 게임 씬에서 빈 게임오브젝트 생성: `GameObject > Create Empty`
2. 이름을 "SoundManager"로 변경
3. SoundManager 스크립트를 오브젝트에 추가
4. Inspector에서 생성한 AudioMixer와 각 믹서 그룹을 할당

## 4. 코드 사용 방법

### 4.1. 효과음 재생 (2D)

```csharp
// 2D 효과음 재생
AudioClip sfxClip = Resources.Load<AudioClip>("Sounds/SFX/Explosion");
int sfxID = SoundManager.Instance.PlaySFX(sfxClip);

// 볼륨 조절과 함께 재생
SoundManager.Instance.PlaySFX(sfxClip, 0.5f);

// 반복 재생
int loopingSfxID = SoundManager.Instance.PlaySFX(sfxClip, 1.0f, "", true);
```

### 4.2. 효과음 재생 (3D 위치 기반)

```csharp
// 3D 위치 기반 효과음 재생
AudioClip enemyAttackClip = Resources.Load<AudioClip>("Sounds/SFX/EnemyAttack");
int enemyAttackID = SoundManager.Instance.PlaySFXAtPosition(enemyAttackClip, transform.position);

// 거리 설정 (최소/최대 거리)
SoundManager.Instance.PlaySFXAtPosition(enemyAttackClip, transform.position, 0.7f, 2f, 15f);
```

### 4.3. 배경음악 재생

```csharp
// 배경음악 재생
AudioClip bgmClip = Resources.Load<AudioClip>("Sounds/BGM/MainTheme");
int bgmID = SoundManager.Instance.PlayMusic(bgmClip);

// 볼륨 및 페이드인 시간 설정
SoundManager.Instance.PlayMusic(bgmClip, 0.7f, true, 2.0f);
```

### 4.4. UI 사운드 재생

```csharp
// UI 사운드 재생
AudioClip buttonClip = Resources.Load<AudioClip>("Sounds/UI/ButtonClick");
SoundManager.Instance.PlayUISound(buttonClip);
```

### 4.5. 발자국 소리 재생 (제한된 동시 발음 수)

```csharp
// 발걸음 소리 재생 ("Footstep" 그룹에 제한 적용)
AudioClip footstepClip = Resources.Load<AudioClip>("Sounds/SFX/Footstep");
SoundManager.Instance.PlayFootstep(footstepClip, transform.position);
```

### 4.6. 사운드 제어

```csharp
// 특정 사운드 중지
SoundManager.Instance.StopSound(sfxID);

// 모든 사운드 중지
SoundManager.Instance.StopAllSounds();

// 사운드 속성(볼륨, 위치) 수정
SoundManager.Instance.ModifySound(sfxID, 0.3f);
SoundManager.Instance.ModifySound(soundID, null, new Vector3(10, 0, 5));
```

### 4.7. 볼륨 조절

```csharp
// 마스터 볼륨 설정 (0 ~ 1 사이)
SoundManager.Instance.SetMasterVolume(0.8f);

// 배경음악 볼륨 설정
SoundManager.Instance.SetMusicVolume(0.6f);

// 효과음 볼륨 설정
SoundManager.Instance.SetSFXVolume(0.7f);

// UI 사운드 볼륨 설정
SoundManager.Instance.SetUIVolume(0.9f);

// 전체 음소거/해제
SoundManager.Instance.SetMute(true);  // 음소거
SoundManager.Instance.SetMute(false); // 음소거 해제
```

### 4.8. 게임 일시정지 연동

```csharp
// 게임 일시정지 시 모든 사운드 일시정지
void PauseGame()
{
    Time.timeScale = 0;
    SoundManager.Instance.PauseAllSounds();
}

// 게임 재개 시 모든 사운드 재개
void ResumeGame()
{
    Time.timeScale = 1;
    SoundManager.Instance.ResumeAllSounds();
}
```

### 4.9. 사운드 그룹 관리

```csharp
// 새 사운드 그룹 추가 (이름, 최대 동시 재생 수, 최소 재생 간격(초))
SoundManager.Instance.AddSoundGroup("Explosion", 3, 0.2f);

// 특정 그룹으로 사운드 재생
SoundManager.Instance.PlaySFX(explosionClip, 1.0f, "Explosion");
```

## 5. 모든 메서드 설명

### 5.1. 초기화 및 핵심 메서드

| 메서드명 | 설명 | 매개변수 | 반환 값 |
|---------|------|---------|---------|
| `Awake()` | 싱글톤 초기화, 풀 및 믹서 초기화 호출 | 없음 | 없음 |
| `Start()` | 기본 사운드 그룹(발자국) 설정 | 없음 | 없음 |
| `InitializePool()` | 유니티 내장 오브젝트 풀 초기화 | 없음 | 없음 |
| `InitializeMixer()` | 오디오 믹서 초기화 및 볼륨 설정 로드 | 없음 | 없음 |
| `CreateAudioSource()` | 새 오디오 소스 생성 (풀 콜백용) | 없음 | AudioSource |
| `OnTakeFromPool(AudioSource)` | 풀에서 오디오소스 가져올 때 호출되는 콜백 | audioSource: 풀에서 가져온 오디오소스 | 없음 |
| `OnReturnToPool(AudioSource)` | 오디오소스를 풀로 반환할 때 호출되는 콜백 | audioSource: 반환할 오디오소스 | 없음 |
| `OnDestroyAudioSource(AudioSource)` | 오디오소스 객체 파괴 시 호출되는 콜백 | audioSource: 제거할 오디오소스 | 없음 |

### 5.2. 사운드 재생 메서드

| 메서드명 | 설명 | 매개변수 | 반환 값 |
|---------|------|---------|---------|
| `PlaySFX(AudioClip, float, string, bool)` | 2D 효과음 재생 | clip: 오디오 클립<br>volume: 볼륨(0~1)<br>groupName: 사운드 그룹 이름<br>loop: 반복 재생 여부 | 사운드 ID (int) |
| `PlaySFXAtPosition(AudioClip, Vector3, float, float, float, string, bool)` | 3D 위치 기반 효과음 재생 | clip: 오디오 클립<br>position: 재생 위치<br>volume: 볼륨(0~1)<br>minDistance: 최소 거리<br>maxDistance: 최대 거리<br>groupName: 사운드 그룹 이름<br>loop: 반복 재생 여부 | 사운드 ID (int) |
| `PlayMusic(AudioClip, float, bool, float)` | 배경 음악 재생 | clip: 오디오 클립<br>volume: 볼륨(0~1)<br>loop: 반복 재생 여부<br>fadeInTime: 페이드인 시간 | 사운드 ID (int) |
| `PlayUISound(AudioClip, float)` | UI 사운드 재생 | clip: 오디오 클립<br>volume: 볼륨(0~1) | 사운드 ID (int) |
| `PlayFootstep(AudioClip, Vector3, float)` | 발자국 소리 재생 (제한된 동시 발음) | clip: 오디오 클립<br>position: 재생 위치<br>volume: 볼륨(0~1) | 사운드 ID (int) |

### 5.3. 사운드 제어 메서드

| 메서드명 | 설명 | 매개변수 | 반환 값 |
|---------|------|---------|---------|
| `ReturnAudioSource(int)` | 특정 ID의 사운드를 풀로 반환 | soundID: 사운드 ID | 없음 |
| `StopSound(int)` | 특정 ID의 사운드 중지 | soundID: 사운드 ID | 없음 |
| `StopAllSounds()` | 모든 사운드 중지 및 풀로 반환 | 없음 | 없음 |
| `ModifySound(int, float?, Vector3?)` | 재생 중인 사운드 속성 수정 | soundID: 사운드 ID<br>volume: 볼륨(NULL 시 변경 안 함)<br>position: 위치(NULL 시 변경 안 함) | 성공 여부(bool) |
| `PauseAllSounds()` | 모든 사운드 일시 정지 | 없음 | 없음 |
| `ResumeAllSounds()` | 모든 사운드 재생 재개 | 없음 | 없음 |

### 5.4. 볼륨 및 설정 관련 메서드

| 메서드명 | 설명 | 매개변수 | 반환 값 |
|---------|------|---------|---------|
| `LoadVolumeSettings()` | 저장된 볼륨 설정 로드 | 없음 | 없음 |
| `SaveVolumeSettings(string, float)` | 볼륨 설정 저장 | name: 저장 키<br>volume: 볼륨 값 | 없음 |
| `SetMasterVolume(float)` | 마스터 볼륨 설정 | volume: 볼륨 값(0~1) | 없음 |
| `SetMusicVolume(float)` | 음악 볼륨 설정 | volume: 볼륨 값(0~1) | 없음 |
| `SetSFXVolume(float)` | 효과음 볼륨 설정 | volume: 볼륨 값(0~1) | 없음 |
| `SetUIVolume(float)` | UI 사운드 볼륨 설정 | volume: 볼륨 값(0~1) | 없음 |
| `ConvertToDecibel(float)` | 선형 볼륨을 데시벨로 변환 | volume: 선형 볼륨 값(0~1) | 데시벨 값(float) |
| `SetMute(bool)` | 전체 음소거 설정 | isMuted: 음소거 여부 | 없음 |

### 5.5. 사운드 그룹 관련 메서드

| 메서드명 | 설명 | 매개변수 | 반환 값 |
|---------|------|---------|---------|
| `AddSoundGroup(string, int, float)` | 새 사운드 그룹 추가 | groupName: 그룹 이름<br>maxConcurrentSounds: 최대 동시 재생 수<br>minTimeBetweenSounds: 최소 재생 간격(초) | 없음 |

### 5.6. 코루틴 메서드

| 메서드명 | 설명 | 매개변수 | 반환 값 |
|---------|------|---------|---------|
| `FadeIn(AudioSource, float, float)` | 페이드인 효과 적용 | audioSource: 오디오소스<br>targetVolume: 목표 볼륨<br>fadeTime: 페이드 시간 | IEnumerator |
| `FadeOutAndStop(AudioSource, float)` | 페이드아웃 후 정지 | audioSource: 오디오소스<br>fadeTime: 페이드 시간 | IEnumerator |
| `AutoReturnRoutine(int, float)` | 지정 시간 후 사운드 자동 반환 | soundID: 사운드 ID<br>clipLength: 클립 길이 | IEnumerator |

## 6. 주의사항 및 팁

1. **사운드 ID 관리**: 
   - 루프 사운드를 재생하는 경우 반환되는 ID를 저장하여 나중에 해당 사운드를 정지할 수 있도록 합니다.

2. **메모리 관리**: 
   - 사용하지 않는 AudioClip은 Resources.UnloadUnusedAssets()를 통해 메모리에서 해제할 수 있습니다.

3. **사운드 그룹 활용**:
   - 자주 발생하는 사운드(발자국, 총소리 등)는 사운드 그룹으로 제한하여 오디오 채널 포화를 방지하세요.

4. **음질과 파일 크기**:
   - 게임의 효과음은 일반적으로 44.1kHz, 128kbps 정도로 설정하는 것이 좋습니다.
   - 배경 음악은 품질과 파일 크기의 균형을 고려하여 설정하세요.

## 7. 확장 방법

### 7.1. 새 믹서 그룹 추가

1. GameAudioMixer에서 필요한 새 그룹(예: "Ambient", "Voice" 등) 추가
2. SoundManager 클래스에 해당 믹서 그룹 변수와 메서드 추가
3. 기존 PlaySFX/PlayMusic 패턴을 따라 새 재생 메서드 구현

### 7.2. 추가 사운드 효과

페이드 인/아웃 외에도 다음과 같은 효과를 추가할 수 있습니다:
- 피치 변조 (동일한 사운드에 약간의 무작위성 추가)
- 3D 사운드에 대한 도플러 효과
- 리버브, 에코 등의 실시간 오디오 효과

## 8. 트러블슈팅

### 8.1. 일반적인 문제

- **사운드가 재생되지 않음**: 
  - AudioMixer 그룹이 올바르게 할당되었는지 확인
  - 음소거 상태가 아닌지 확인
  - 볼륨이 0이 아닌지 확인

- **메모리 사용량 증가**: 
  - 대용량 오디오 클립은 적절한 압축 설정 확인
  - 불필요한 루프 사운드가 정리되지 않는지 확인

- **사운드 딜레이**: 
  - 너무 많은 오디오가 동시에 재생되는지 확인
  - 오디오 클립이 너무 크지 않은지 확인

### 8.2. 플랫폼별 고려사항

- **모바일**: 
  - 메모리와 배터리 사용량 최적화를 위해 오디오 품질과 동시 재생 수 제한
  - 압축 포맷 사용 권장 (MP3, Vorbis 등)

- **웹**: 
  - 로딩 시간 단축을 위한 스트리밍 오디오 설정 고려
  - 브라우저 정책에 따른 자동 재생 제한 확인