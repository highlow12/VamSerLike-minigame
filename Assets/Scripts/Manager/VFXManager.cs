using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    Volume volume;
    ChromaticAberration chromaticAberration;
    Vignette vignette;
    Bloom bloom;
    LensDistortion lensDistortion;

    SpriteRenderer spriteRenderer;

    public Material defaultMaterial;
    public Material _invertMaterial;
    Material invertMaterial;
    public Material _noiseMaterial;
    Material noiseMaterial;
    public Material _eyeMaterial;
    Material eyeMaterial;
    public Material _voronoiMaterial;
    Material voronoiMaterial;

    public GameObject up;
    public GameObject down;

    bool eyeBlink = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.material = defaultMaterial;
        spriteRenderer.color = new Color(1, 1, 1);

        invertMaterial = Instantiate(_invertMaterial);
        noiseMaterial = Instantiate(_noiseMaterial);
        eyeMaterial = Instantiate(_eyeMaterial);
        voronoiMaterial = Instantiate(_voronoiMaterial);

        volume = GetComponent<Volume>();
        volume.profile.TryGet(out chromaticAberration);
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out bloom);
        volume.profile.TryGet(out lensDistortion);

        eyeBlink = true;

        // AnimateInvertColor(0, 1000f);
        // AnimateNoise();
        // AnimateChromaticAberration(1, 5000f);
        // AnimateLensDistortion(1, 1, 5000f);
        // AnimateLensDistortion(0.5f, 1f, 2500f);
        // StartCoroutine(EyeBlinkLoop());
    }

    IEnumerator Voronoi(float a, float b, float duration)
    {
        spriteRenderer.material = voronoiMaterial;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime * 1000;
            float t = Mathf.Clamp01(elapsedTime / duration);
            voronoiMaterial.SetFloat("_Intense", Mathf.Lerp(a, b, t));
            yield return null;
        }
    }

    // -0.75f ~ 0.75f
    IEnumerator EyeBlink(float value)
    {
        spriteRenderer.material = eyeMaterial;
        
        float oldValue = eyeMaterial.GetFloat("_T");
        float elapsedTime = 0f;

        while (elapsedTime < 500f)
        {
            elapsedTime += Time.deltaTime * 1000;
            float t = Mathf.Clamp01(elapsedTime / 500f);
            eyeMaterial.SetFloat("_T", Mathf.Lerp(oldValue, value, t));
            yield return null;
        }
    }

    IEnumerator EyeBlinkLoop() {
        while (eyeBlink) {
            StartCoroutine(EyeBlink(1));
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(EyeBlink(0f));
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Animate chromatic aberration effect
    IEnumerator ChromaticAberration(float value, float duration)
    {
        float oldValue = chromaticAberration.intensity.value;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime * 1000;
            float t = Mathf.Clamp01(elapsedTime / duration);
            chromaticAberration.intensity.value = Mathf.Lerp(oldValue, value, t);
            yield return null;
        }
    }

    // Animate lens distortion effect
    IEnumerator LensDistortion(float value, float scale, float duration)
    {
        float oldValue = lensDistortion.intensity.value;
        float oldScale = lensDistortion.scale.value;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime * 1000;
            float t = Mathf.Clamp01(elapsedTime / duration);
            lensDistortion.intensity.value = Mathf.Lerp(oldValue, value, t);
            lensDistortion.scale.value = Mathf.Lerp(oldScale, scale, t);
            yield return null;
        }
    }

    // Invert color effect
    // value: 0 = inverted, 1 = normal
    // duration: time to complete the effect (in milliseconds)
    IEnumerator InvertColor(float value, float duration)
    {
        float oldValue = invertMaterial.GetFloat("_T");
        float elapsedTime = 0f;
        spriteRenderer.material = invertMaterial;

        while (elapsedTime < duration)
        {
            Debug.Log("invertMaterial");
            Debug.Log($"elapsedTime: {elapsedTime} duration: {duration} value: {value}");
            elapsedTime += Time.deltaTime * 1000;
            float t = Mathf.Clamp01(elapsedTime / duration);
            invertMaterial.SetFloat("_T", Mathf.Lerp(oldValue, value, t));
            yield return null;
        }
    }

    // Animate chromatic aberration effect
    // value: 0 = no effect, 1 = full effect
    public void AnimateChromaticAberration(float value, float duration)
    {
        StartCoroutine(ChromaticAberration(value, duration));
    }

    // Animate lens distortion effect
    // value: 0 = no effect, 1 = full effect
    // scale: 0 = no effect, 1 = full effect
    public void AnimateLensDistortion(float value, float scale, float duration)
    {
        StartCoroutine(LensDistortion(value, scale, duration));
    }

    public void AnimateInvertColor(float value, float duration)
    {
        StartCoroutine(InvertColor(value, duration));
    }

    public void AnimateNoise() {
        spriteRenderer.material = noiseMaterial;
    }

    public void Normalize()
    {
        spriteRenderer.material = defaultMaterial;
        spriteRenderer.color = new Color(1, 1, 1);
    }

    public void AnimateVoronoi(float a, float b, float duration)
    {
        StartCoroutine(Voronoi(a, b, duration));
    }

    public void BlackOut()
    {
        spriteRenderer.color = new Color(0, 0, 0);
    }
}
