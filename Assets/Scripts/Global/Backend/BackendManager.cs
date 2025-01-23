using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BackEnd;
using System;
using UnityEditor.UI;


[DefaultExecutionOrder(-100)]
public class BackendManager : MonoBehaviour
{
    void Awake()
    {
        bool bro = BackendInitialize();
        if (!bro)
        {
            Debug.Log("Backend Initialize Failed");
            Application.Quit();
        }
    }

    bool BackendInitialize()
    {
        var bro = Backend.Initialize();
        return bro.IsSuccess();
    }

}
