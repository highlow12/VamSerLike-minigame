using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Stage6VFX : MonoBehaviour
{
    [SerializeField] GameObject journal;
    [SerializeField] GameObject handParticle;
    [SerializeField] GameObject bloodParticle;
    [SerializeField] GameObject head;

    void Start()
    {
        VFXManager.Instance.journal = journal;
        VFXManager.Instance.handParticle = handParticle;
        VFXManager.Instance.bloodParticle = bloodParticle;

        StartCoroutine(VFX());
    }

    IEnumerator AnimateHead()
    {
        RectTransform rt = head.GetComponent<RectTransform>();
        rt.position = new Vector3(0, rt.position.y, rt.position.z);
        head.SetActive(true);
        float windowW = GetComponent<RectTransform>().rect.width;
        float sizeX = head.GetComponent<RectTransform>().rect.width;
        Debug.Log("windowW: " + windowW + " sizeX: " + sizeX);
        rt.DOMoveX(windowW + (sizeX / 2), 4f).SetEase(Ease.Linear);
        rt.DORotate(new Vector3(0, 0, -720), 4f, RotateMode.FastBeyond360).SetEase(Ease.Linear);
        yield return null;
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
        yield return new WaitForSeconds(3f);
        yield return new WaitForSeconds(130f);
        yield return new WaitForSeconds(50f);
        StartCoroutine(AnimateHead());
    }
}
