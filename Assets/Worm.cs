using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Worm : MonoBehaviour
{
    public bool isMain = true;
    public int maxWormCount = 10;
    [SerializeField] private GameObject worm;
    List<GameObject> worms = new List<GameObject>();

    public static Worm Instance { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WormReset();
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        WormReset();
    }

    void OnEnable()
    {
        WormReset();
    }

    void WormReset()
    {
        if (isMain) { worms.Add(worm); return; }
        
        Vector3 randomPos = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0f, 1f), 1.2f));
        transform.position = new Vector3(randomPos.x, randomPos.y, 0);

        // top to bottom
        transform.DOLocalMoveY(-14, 1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            // disable the game object
            gameObject.SetActive(false);
        });
    }

    public void test()
    {
        int activeWormCount = GetActiveWormCount();
        if (activeWormCount >= maxWormCount) return;
        else if (worms.Count <= maxWormCount)
        {
            GameObject clone = Instantiate(worm, transform);
            Worm _ = clone.GetComponent<Worm>();
            _.isMain = false;
            worms.Add(clone);
            clone.SetActive(true);
        }
        else if (worms.Count - activeWormCount > 0)
        {
            GameObject inactiveWorm = GetInactiveWorm();
            inactiveWorm.SetActive(true);
        }
    }

    int GetActiveWormCount()
    {
        return worms.FindAll((x) => x.activeSelf).Count;
    }

    GameObject GetInactiveWorm()
    {
        return worms.Find((x) => !x.activeSelf);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Worm))]
public class WormInspector: Editor
{
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();
    var worm = target as Worm;
    if (GUILayout.Button("Test"))
    {
        worm.test();
    }
  }
}
#endif