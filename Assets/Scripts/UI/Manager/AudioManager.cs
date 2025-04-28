using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance => instance;

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource effectSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            
            // 현재 캔버스의 자식으로 있다면 루트 레벨로 이동
            if (transform.parent != null)
            {
                // 현재 transform 정보 저장
                Transform originalParent = transform.parent;
                
                // 루트 레벨로 이동
                transform.SetParent(null);
                
                // DontDestroyOnLoad 적용
                DontDestroyOnLoad(gameObject);
                
                Debug.Log("AudioManager가 루트 레벨로 이동되었습니다.");
            }
            else
            {
                // 이미 루트 레벨에 있으므로 바로 DontDestroyOnLoad 적용
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetBGMVolume(bool isOn)
    {
        bgmSource.volume = isOn ? 1f : 0f;
    }

    public void SetEffectVolume(bool isOn)
    {
        effectSource.volume = isOn ? 1f : 0f;
    }

    public void PlayEffect(AudioClip clip)
    {
        if (effectSource.volume > 0)
        {
            effectSource.PlayOneShot(clip);
        }
    }
}