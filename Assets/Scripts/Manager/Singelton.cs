using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject obj;
                obj = GameObject.Find(typeof(T).Name);
                if(obj == null)
                {
                    obj = new GameObject(typeof(T).Name);
                    instance = obj.AddComponent<T>();
                }
                else
                {
                    instance = obj.GetComponent<T>();
                }
            }
            return instance;
        }
    }

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
//출처: https://sillyknight.tistory.com/30 [실리의 프로그램 사이트:티스토리]
