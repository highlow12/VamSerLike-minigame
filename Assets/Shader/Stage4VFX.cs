using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Stage4VFX : MonoBehaviour
{
    [SerializeField] GameObject journal;
    [SerializeField] GameObject handParticle;
    [SerializeField] GameObject bloodParticle;
    [SerializeField] GameObject Shadow;

    private SpriteRenderer bloodParticle2SpriteRenderer;

    void Start()
    {
        VFXManager.Instance.journal = journal;
        VFXManager.Instance.handParticle = handParticle;
        VFXManager.Instance.bloodParticle = bloodParticle;

        StartCoroutine(VFX());
    }

    IEnumerator AnimateShadow()
    {
        RectTransform rt = Shadow.GetComponent<RectTransform>();
        rt.position = new Vector3(0, rt.position.y, rt.position.z);
        Shadow.SetActive(true);
        float windowW = GetComponent<RectTransform>().rect.width;
        float sizeX = Shadow.GetComponent<RectTransform>().rect.width;
        Debug.Log("windowW: " + windowW + " sizeX: " + sizeX);
        rt.DOMoveX(windowW + (sizeX / 2), 5f);
        yield return null;
    }

    IEnumerator VFX()
    {
        yield return new WaitForSeconds(2f);
        VFXManager.Instance.Normalize();
        yield return new WaitForSeconds(28f);
        StartCoroutine(AnimateShadow());
        yield return new WaitForSeconds(160f);
        StartCoroutine(AnimateShadow());
        yield return new WaitForSeconds(110f);
        VFXManager.Instance.AnimateNoise();
        yield return new WaitForSeconds(2f);
        VFXManager.Instance.Normalize();
        VFXManager.Instance.Journal(true);
        yield return new WaitForSeconds(5f);
        VFXManager.Instance.Journal(false);
        yield return new WaitForSeconds(83f);
        VFXManager.Instance.AnimateShake(0, 5, 2f);
        // StartCoroutine(BloodParticle());
        // 쓰레기
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
