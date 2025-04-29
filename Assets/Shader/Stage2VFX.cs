using System.Collections;
using UnityEngine;

public class Stage2VFX : MonoBehaviour
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

    void Update()
    {

    }

    IEnumerator VFX()
    {
        VFXManager.Instance.Normalize();
        // VFXManager.Instance.HandParticle();
        // yield return new WaitForSeconds(5f);
        // VFXManager.Instance.HandParticle();
        yield return new WaitForSeconds(180f);
        VFXManager.Instance.HandParticle();

        yield return new WaitForSeconds(88f);
        VFXManager.Instance.AnimateNoise();

        yield return new WaitForSeconds(2f);
        VFXManager.Instance.Normalize();
        VFXManager.Instance.Journal(true);
        yield return new WaitForSeconds(5f);
        VFXManager.Instance.Journal(false);
    }
}
