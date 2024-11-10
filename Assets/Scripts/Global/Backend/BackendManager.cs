using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;


[DefaultExecutionOrder(-100)]
public class BackendManager : Singleton<BackendManager>
{
    void Start()
    {
        bool bro = BackendInitialize();
        if (!bro)
        {
            Debug.Log("Backend Initialize Failed");
            Application.Quit();
        }
    }

    void Update()
    {

    }

    bool BackendInitialize()
    {
        var bro = Backend.Initialize();
        return bro.IsSuccess();
    }

}
