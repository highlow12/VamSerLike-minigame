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
        rt.DOMoveX(windowW + (sizeX / 2), 5f).SetEase(Ease.Linear);
        yield return null;
    }

    IEnumerator VFX()
    {
        // TODO: 심해로 가라앉는 효과
        yield return new WaitForSeconds(2f);
        VFXManager.Instance.Normalize();
        yield return new WaitForSeconds(28f);
        StartCoroutine(AnimateShadow());
        yield return new WaitForSeconds(130f);
        // TODO: 물 색 변경
        yield return new WaitForSeconds(30f);
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
}
