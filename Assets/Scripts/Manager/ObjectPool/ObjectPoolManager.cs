using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using System;

[DefaultExecutionOrder(-100)]
public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class ObjectInfo
    {
        // 오브젝트 이름
        public string objectName;
        // 오브젝트 풀에서 관리할 오브젝트
        public GameObject prefab;
        // parent
        public Transform parent;
        // 몇개를 미리 생성 해놓을건지
        public int count;
    }


    public static ObjectPoolManager Instance;

    // 오브젝트풀 매니저 준비 완료표시
    public bool IsReady { get; private set; }

    // default parent transform of object pool
    public Transform defaultParent;

    [SerializeField]
    private List<ObjectInfo> objectInfos = new();

    // 생성할 오브젝트의 key값지정을 위한 변수
    private string objectName;

    // parent transfrom of object pool
    private Transform objectParent;

    // 오브젝트풀들을 관리할 딕셔너리
    private Dictionary<string, IObjectPool<GameObject>> objectPoolDic = new Dictionary<string, IObjectPool<GameObject>>();

    // 오브젝트풀에서 오브젝트를 새로 생성할때 사용할 딕셔너리
    private Dictionary<string, GameObject> goDic = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this.gameObject);
        }

        Init();
    }

    private void Init()
    {
        IsReady = false;

        for (int idx = 0; idx < objectInfos.Count; idx++)
        {
            RegisterObjectPool(objectInfos[idx].objectName, objectInfos[idx].prefab, objectInfos[idx].parent, objectInfos[idx].count);
        }

        Debug.Log("오브젝트풀링 준비 완료");
        IsReady = true;
    }

    // dynamic object pool register
    public void RegisterObjectPool(string objectName, GameObject prefab, Transform parent, int count)
    {
        IObjectPool<GameObject> pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
            OnDestroyPoolObject, true, count, count);

        if (goDic.ContainsKey(objectName))
        {
            Debug.LogFormat("{0} 이미 등록된 오브젝트입니다.", objectName);
            return;
        }

        goDic.Add(objectName, prefab);
        objectPoolDic.Add(objectName, pool);
        if (parent != null)
        {
            objectParent = parent;
        }
        else
        {
            GameObject parentGo = GameObject.Find($"{prefab.tag}Pool");
            if (parentGo == null)
            {
                objectParent = defaultParent;
            }
            else
            {
                objectParent = parentGo.transform;
            }
        }
        // 미리 오브젝트 생성 해놓기
        for (int i = 0; i < count; i++)
        {
            this.objectName = objectName;

            PoolAble poolAbleGo = CreatePooledItem().GetComponent<PoolAble>();
            if (objectInfos.Find(x => x.objectName == objectName) == null)
            {
                ObjectInfo objectInfo = new()
                {
                    objectName = objectName,
                    prefab = prefab,
                    parent = objectParent,
                    count = count
                };
                objectInfos.Add(objectInfo);
            }
            poolAbleGo.Pool.Release(poolAbleGo.gameObject);
            objectParent = null;
        }
    }

    // unregister object pool
    public void UnregisterObjectPool(string objectName)
    {
        if (objectPoolDic.ContainsKey(objectName) == false)
        {
            Debug.LogFormat("{0} 오브젝트풀에 등록되지 않은 오브젝트입니다.", objectName);
            return;
        }
        IObjectPool<GameObject> pool = objectPoolDic[objectName];
        pool.Clear();
        objectPoolDic.Remove(objectName);
        goDic.Remove(objectName);
        objectInfos.Remove(objectInfos.Find(x => x.objectName == objectName));

    }

    // 생성
    private GameObject CreatePooledItem()
    {
        Transform objectParent = objectInfos.Find(x => x.objectName == objectName)?.parent;
        GameObject poolGo = Instantiate(goDic[objectName], this.objectParent ? this.objectParent : objectParent);
        poolGo.GetComponent<PoolAble>().Pool = objectPoolDic[objectName];
        return poolGo;
    }

    // 대여
    private void OnTakeFromPool(GameObject poolGo)
    {
        poolGo.SetActive(true);
    }

    // 반환
    private void OnReturnedToPool(GameObject poolGo)
    {
        poolGo.SetActive(false);
    }

    // 삭제
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        Destroy(poolGo);
    }

    public GameObject GetGo(string goName)
    {
        objectName = goName;

        if (goDic.ContainsKey(goName) == false)
        {
            Debug.LogFormat("{0} 오브젝트풀에 등록되지 않은 오브젝트입니다.", goName);
            return null;
        }

        return objectPoolDic[goName].Get();
    }
}