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
            DontDestroyOnLoad(gameObject);
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