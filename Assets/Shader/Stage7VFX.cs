using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Stage7VFX : MonoBehaviour
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
        if (Worm.Instance != null) Worm.Instance.test();
        yield return new WaitForSeconds(30f);
        VFXManager.Instance.AnimateNoise();
        yield return new WaitForSeconds(2f);
        VFXManager.Instance.Normalize();
        VFXManager.Instance.Journal(true);
        yield return new WaitForSeconds(5f);
        VFXManager.Instance.Journal(false);
        yield return new WaitForSeconds(3f);
        yield return new WaitForSeconds(10f);
        // ?
        yield return new WaitForSeconds(30f);
        VFXManager.Instance.AnimateVoronoi(0f, 0.5f, 2500f);
        yield return new WaitForSeconds(2.5f);
        VFXManager.Instance.AnimateVoronoi(0.5f, 0, 2500f);
        yield return new WaitForSeconds(2.5f);
    }
}
