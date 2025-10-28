using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected virtual bool useDontDestroyOnLoad
    {
        get { return false; }
    }

    private static T instance;
    private static bool applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
                return null;

            if (instance == null)
            {
                // 1) 씬에 존재하는 인스턴스 우선 탐색
                instance = Object.FindObjectOfType<T>();
                if (instance != null)
                    return instance;

                // 2) 이름으로 찾기 (GameObject 이름이 클래스명일 경우)
                var go = GameObject.Find(typeof(T).Name);
                if (go != null)
                {
                    instance = go.GetComponent<T>();
                    if (instance != null)
                        return instance;
                }

                // 3) 없으면 새로 생성 (경고 로그)
                var newObj = new GameObject(typeof(T).Name);
                instance = newObj.AddComponent<T>();
                Debug.LogWarning($"[{typeof(T).Name}] 인스턴스를 자동 생성했습니다. (인스펙터 연결 필드가 비어있을 수 있음)");
            }
            return instance;
        }
    }

    public virtual void Awake()
    {
        // 중복 인스턴스 방지: 이미 존재하면 자기 자신을 파괴
        if (instance == null)
        {
            instance = this as T;
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        if (useDontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }
}
//출처: https://sillyknight.tistory.com/30 [실리의 프로그램 사이트:티스토리]
