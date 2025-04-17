using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VFXManager : MonoBehaviour
{
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.material = defaultMaterial;
        spriteRenderer.color = new Color(1, 1, 1);

        invertMaterial = Instantiate(_invertMaterial);
        noiseMaterial = Instantiate(_noiseMaterial);

        volume = GetComponent<Volume>();
        volume.profile.TryGet(out chromaticAberration);
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out bloom);
        volume.profile.TryGet(out lensDistortion);

        // AnimateInvertColor(0, 10000f);
    }

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

    public void AnimateChromaticAberration(float value, float duration)
    {
        StartCoroutine(ChromaticAberration(value, duration));
    }

    public void AnimateLensDistortion(float value, float scale, float duration)
    {
        StartCoroutine(LensDistortion(value, scale, duration));
    }

    public void AnimateInvertColor(float value, float duration)
    {
        StartCoroutine(InvertColor(value, duration));
    }

    public void BlackOut()
    {
        spriteRenderer.color = new Color(0, 0, 0);
    }
}
