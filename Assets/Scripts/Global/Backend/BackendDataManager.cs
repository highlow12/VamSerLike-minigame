using UnityEngine;
using BackEnd;
using System.Collections.Generic;

public class BackendDataManager : Singleton<BackendDataManager>
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

    public class ChartCardV2
    {
        public string chartName; // 차트이름
        public string chartExplain; // 차트 설명
        public string selectedChartFileId; // 차트 파일 아이디

        public override string ToString()
        {
            return $"chartName: {chartName}\n" +
            $"chartExplain: {chartExplain}\n" +
            $"selectedChartFileId: {selectedChartFileId}\n";
        }
    }

    private bool _isSignedIn;
    public bool isSignedIn
    {
        get
        {
            return _isSignedIn;
        }
        set
        {
            _isSignedIn = value;
            if (_isSignedIn)
            {
                GetChartList();
            }
        }
    }

    public List<ChartCardV2> chartCardList = new();

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

    public void GetChartList()
    {
        var bro = Backend.Chart.GetChartListV2();
        if (bro.IsSuccess() == false)
        {
            Debug.LogError($"GetChartListV2 Failed: {bro.GetStatusCode()}\n{bro.GetMessage()}\n{bro}");
            return;
        }
        LitJson.JsonData chartList = bro.FlattenRows();
        for (int i = 0; i < chartList.Count; i++)
        {
            ChartCardV2 chartCard = new()
            {
                chartName = chartList[i]["chartName"].ToString(),
                chartExplain = chartList[i]["chartExplain"].ToString(),
                selectedChartFileId = chartList[i]["selectedChartFileId"].ToString()
            };

            chartCardList.Add(chartCard);
        }
    }

    public LitJson.JsonData GetChartData(string chartName)
    {
        Debug.Log($"GetChartData: {chartName}");
        string chartFileId = chartCardList.Find(x => x.chartName == chartName).selectedChartFileId;
        var bro = Backend.Chart.GetChartContents(chartFileId);
        if (bro.IsSuccess() == false)
        {
            Debug.LogError($"GetChartContents Failed: {bro.GetStatusCode()}\n{bro.GetMessage()}\n{bro}");
            return null;
        }
        LitJson.JsonData chartData = new();
        chartData = bro.GetReturnValuetoJSON()["rows"];
        return chartData;
    }
}
