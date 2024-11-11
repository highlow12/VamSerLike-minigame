using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using System;


[DefaultExecutionOrder(-100)]
public class BackendManager : Singleton<BackendManager>
{
    public struct UserData
    {
        public string gamerId;
        public string nickname;
        public string countryCode;
        public string emailForFindPassword;
        public string subscriptionType;
        public string federationId;
        public string inDate;
    }

    public bool isSignedIn = false;

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

    public static UserData GetUserData()
    {
        UserData userData = new();
        var bro = Backend.BMember.GetUserInfo();
        var row = bro.GetReturnValuetoJSON()["row"];
        userData.gamerId = row["gamerId"] == null ? "null" : row["gamerId"].ToString();
        userData.nickname = row["nickname"] == null ? "null" : row["nickname"].ToString();
        userData.countryCode = row["countryCode"] == null ? "null" : row["countryCode"].ToString();
        userData.emailForFindPassword = row["emailForFindPassword"] == null ? "null" : row["emailForFindPassword"].ToString();
        userData.subscriptionType = row["subscriptionType"] == null ? "null" : row["subscriptionType"].ToString();
        userData.federationId = row["federationId"] == null ? "null" : row["federationId"].ToString();
        userData.inDate = row["inDate"] == null ? "null" : row["inDate"].ToString();
        return userData;
    }

}
