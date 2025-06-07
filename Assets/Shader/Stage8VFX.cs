using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Stage8VFX : MonoBehaviour
{
    [SerializeField] GameObject journal;
    [SerializeField] GameObject handParticle;
    [SerializeField] GameObject bloodParticle;

    void Start()
    {
        VFXManager.Instance.journal = journal;
        VFXManager.Instance.handParticle = handParticle;
        VFXManager.Instance.bloodParticle = bloodParticle;

        VFXManager.Instance.customHandVFX = (bool enable) =>
        {
            handParticle.SetActive(enable);
            if (enable) StartCoroutine(HandVFX());
        };

        VFXManager.Instance.customBloodVFX = (bool enable) =>
        {
            bloodParticle.SetActive(enable);
            if (enable) StartCoroutine(BloodVFX());
        };

        StartCoroutine(VFX());
    }

    IEnumerator VFX()
    {
        VFXManager.Instance.AnimateLensDistortion(0.5f, 1.1f, 1500f);
        yield return new WaitForSeconds(60f);
        VFXManager.Instance.HandParticle(true);

        yield return new WaitForSeconds(20f);
        // TODO: 눈알 굴리는 효과 적용이 안되는 버그 고치기
        // Lens Distortion 컴포넌트에서 center 값이 변경이 되나, 실제 게임 화면에서는 적용이 안됨
        StartCoroutine(AnimateEye(1f, 1f, 2.5f));
        yield return new WaitForSeconds(2.5f);
        StartCoroutine(AnimateEye(1f, 0f, 1.5f));
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(AnimateEye(0f, 0f, 1f));
        yield return new WaitForSeconds(1f);
        StartCoroutine(AnimateEye(0.5f, 0.5f, 5f));
        // TODO: 화면 상단에서 피가 흐르는 효과 추가하기
        yield return new WaitForSeconds(5f);

        yield return new WaitForSeconds(210f);
        VFXManager.Instance.AnimateNoise();
        yield return new WaitForSeconds(2f);
        VFXManager.Instance.Normalize();
        VFXManager.Instance.Journal(true);
        yield return new WaitForSeconds(5f);
        VFXManager.Instance.Journal(false);
        yield return new WaitForSeconds(3f);

        yield return new WaitForSeconds(50f);
        // TODO: 화면 빨갛게
        yield return new WaitForSeconds(10f);
        VFXManager.Instance.AnimateVoronoi(0f, 0.5f, 2500f);
        yield return new WaitForSeconds(2.5f);
        VFXManager.Instance.AnimateVoronoi(0.5f, 0, 2500f);
        yield return new WaitForSeconds(2.5f);
        VFXManager.Instance.Normalize();
        
    }

    IEnumerator HandVFX()
    {
        yield return new WaitForSeconds(5f);
        handParticle.SetActive(false);
    }

    IEnumerator BloodVFX()
    {
        yield return new WaitForSeconds(2.5f);
        bloodParticle.SetActive(false);
    }

    // ? 값은 적용되나, 실제 게임 화면에서 적용이 안됨
    IEnumerator AnimateEye(float x, float y, float duration)
    {
        LensDistortion lensDistortion = VFXManager.Instance.lensDistortion;
        float initialX = lensDistortion.center.value.x;
        float initialY = lensDistortion.center.value.y;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float progress = Mathf.Clamp01(t / duration);
            lensDistortion.center = new Vector2Parameter(new Vector2(
                Mathf.Lerp(initialX, x, progress),
                Mathf.Lerp(initialY, y, progress)
            ), true);
            yield return null;
        }
    }
}
