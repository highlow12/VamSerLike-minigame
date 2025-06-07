using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Stage5VFX : MonoBehaviour
{
    [SerializeField] GameObject journal;
    [SerializeField] GameObject handParticle;
    [SerializeField] GameObject bloodParticle;

    void Start()
    {
        VFXManager.Instance.journal = journal;
        VFXManager.Instance.handParticle = handParticle;
        VFXManager.Instance.bloodParticle = bloodParticle;

        StartCoroutine(VFX());
    }

    IEnumerator VFX()
    {
        yield return new WaitForSeconds(30f);
        VFXManager.Instance.AnimateShakeBack(0, 1, 2f);
        yield return new WaitForSeconds(2f);
        yield return new WaitForSeconds(18f);
        VFXManager.Instance.AnimateNoise();
        yield return new WaitForSeconds(2f);
        VFXManager.Instance.Normalize();
        VFXManager.Instance.Journal(true);
        yield return new WaitForSeconds(5f);
        VFXManager.Instance.Journal(false);
        yield return new WaitForSeconds(103f);
        // TODO: 피가 흐르는 효과
        yield return new WaitForSeconds(120f);
        VFXManager.Instance.AnimateShakeBack(0, 1, 2f);
        yield return new WaitForSeconds(110f);
        // TODO: 붉은 화면 연출
    }
}
