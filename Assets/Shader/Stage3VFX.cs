using System.Collections;
using UnityEngine;

public class Stage3VFX : MonoBehaviour
{
    [SerializeField] GameObject journal;
    [SerializeField] GameObject handParticle;
    [SerializeField] GameObject bloodParticle;
    [SerializeField] GameObject bloodParticle2;

    private SpriteRenderer bloodParticle2SpriteRenderer;

    void Start()
    {
        VFXManager.Instance.journal = journal;
        VFXManager.Instance.handParticle = handParticle;
        VFXManager.Instance.bloodParticle = bloodParticle;

        bloodParticle2SpriteRenderer = bloodParticle2.GetComponent<SpriteRenderer>();
        Material _material = Instantiate(bloodParticle2SpriteRenderer.material);
        bloodParticle2SpriteRenderer.material = _material;
        bloodParticle2SpriteRenderer.material.SetFloat("_T", 0f);

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
        yield return new WaitForSeconds(180f);
        VFXManager.Instance.AnimateNoise();
        yield return new WaitForSeconds(2f);
        VFXManager.Instance.Normalize();
        VFXManager.Instance.Journal(true);
        yield return new WaitForSeconds(5f);
        VFXManager.Instance.Journal(false);
        yield return new WaitForSeconds(203f);
        StartCoroutine(BloodParticle());
        // TODO: 피가 흐르는 효과
    }

    IEnumerator BloodParticle()
    {
        for (int i = 0; i < 100; i++) {
            bloodParticle2SpriteRenderer.material.SetFloat("_T", i / 100f);
            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator HandVFX()
    {
        yield return new WaitForSeconds(2.5f);
        handParticle.SetActive(false);
    }

    IEnumerator BloodVFX()
    {
        yield return new WaitForSeconds(2.5f);
        bloodParticle.SetActive(false);
    }
}
