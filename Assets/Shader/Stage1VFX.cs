using System.Collections;
using UnityEngine;

public class Stage1VFX : MonoBehaviour
{
    // float time = 0f;
    [SerializeField] GameObject journal;

    void Start()
    {
        VFXManager.Instance.journal = journal;
        StartCoroutine(VFX());
    }

    // Update is called once per frame
    void Update()
    {
        // time += Time.deltaTime;
    }

    IEnumerator VFX()
    {
        VFXManager.Instance.AnimateVoronoi(0f, 0.5f, 1500f);
        yield return new WaitForSeconds(1.5f);
        VFXManager.Instance.AnimateVoronoi(0.5f, 0, 1500f);
         yield return new WaitForSeconds(1.5f);
        VFXManager.Instance.Normalize();
        yield return new WaitForSeconds(177f);

        VFXManager.Instance.BlackOut();
        yield return new WaitForSeconds(1f);
        VFXManager.Instance.Normalize();
        yield return new WaitForSeconds(29f);

        VFXManager.Instance.AnimateNoise();
        yield return new WaitForSeconds(2f);
        VFXManager.Instance.Normalize();
        VFXManager.Instance.Journal(true);
        yield return new WaitForSeconds(1f);
        VFXManager.Instance.Journal(false);
    }
}
