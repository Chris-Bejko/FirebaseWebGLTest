// Copyright (C) 2022 Myria - All rights reserved
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Globalization;
using UnityEngine.Networking;
using UnityEngine;



public class FirebaseSDK : MonoBehaviour
{
    private const string kURL = "http://ip-api.com/json";
    private string kUsername = "ExampleUsername";
    private const int kCoins = 5;
    private const int kGems = 1;

    public string currentDate;

    private string _formattedDate;
    private int _elapsedDays;
    private string _path;
    private string _generalData;
    private float _startTime;
    private float _quitTime;
    private bool _isDoingOperation;

    /// What operation was requested
    private AnalyticsEventID _id;
    /// What data was given from the operation, in case we want to re-do it
    private object _data;

    public static FirebaseSDK Instance { get; private set; }
    AnalysisManager analysisManager;
    /// <summary>
    /// Extern methods from Firebase plugin
    /// </summary>
    [DllImport("__Internal")]
    private static extern void GetJSON(string path, string objectName, string callback, string fallback);
    [DllImport("__Internal")]
    private static extern void PostJSON(string path, string value, string objectName, string callback, string fallback);
    [DllImport("__Internal")]
    private static extern void ModifyNumberWithTransaction(string path, float amount, string objectName, string callback, string fallback);
    [DllImport("__Internal")]
    private static extern void SignInUserAnonymously(string objectName, string callback, string fallback);
    private void Awake()
    {
        Instance = this;
        analysisManager = FindObjectOfType<AnalysisManager>();
    }

    /// <summary>
    /// On session start we initialize some stats, and update wanted data (marketing, retention, etc)
    /// </summary>
    /// <param name="e"></param>
    public void StartSession(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.StringData;
        _isDoingOperation = false;
        _startTime = Time.time;
        _generalData = "";
        currentDate = DateTime.Now.ToString("dd/MM/yy");
        kUsername = e.StringData;
        GetMarketingData();
        SignInUserAnonymously(gameObject.name, "OnRequestSuccess" , "OnRequestFailed");
        GetJSON($"{kUsername}/Retention/D1", gameObject.name, "PerformStartActions", "OnRequestFailed"); ///Player retention by days, and daily active users\
    }

    /// <summary>
    /// Date Comparing to determine which day of activity it is for this Player
    /// </summary>
    /// <param name="date"></param>
    public void PerformStartActions(string date)
    {
        StartCoroutine(IEPerformStartActions(date));
    }

    /// <summary>
    /// Compares the last date the user logged in, determines Player retention by days (D1, D4 etc), and Daily Active Users (by date:amount)
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    IEnumerator IEPerformStartActions(string date)
    {
        ModifyNumberWithTransaction($"{kUsername}/TotalSessions", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed"); ///Update total sessions

        var wait = new WaitForSeconds(1f);
        var day1 = date;

        if (date.Equals("null"))
        {
            date = currentDate;
            day1 = currentDate;

            ///update retention
            PostJSON($"{kUsername}/Retention/D1", day1.ToString().Replace("/", "-"), gameObject.name, "ReturnData", "OnRequestFailed");

            ///update total days active
            ModifyNumberWithTransaction(kUsername + "/Retention/TotalDaysActive", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");

            ///update daily active users
            ModifyNumberWithTransaction($"Main/DAU/{date.Replace("/", "-").Replace("\"", "")}", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        }

        _formattedDate = day1.Replace("\"", "");
        var firstDateTime = DateTime.Parse(_formattedDate, new CultureInfo("fr-FR"));
        var currentDateTime = DateTime.Parse(currentDate, new CultureInfo("fr-FR"));
        _elapsedDays = (int)(currentDateTime - firstDateTime).TotalDays;

        GetJSON($"{kUsername}/Retention/D{_elapsedDays + 1}", gameObject.name, "ReturnData", "OnRequestFailed");
        yield return wait;

        /// first login for today
        if (_generalData.Equals("null"))
        {
            /// update retention
            PostJSON($"{kUsername}/Retention/D{_elapsedDays + 1}", currentDate.Replace("/", "-"), gameObject.name, "ReturnData", "OnRequestFailed");

            /// update total days active;
            ModifyNumberWithTransaction(kUsername + "/Retention/TotalDaysActive", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");

            /// update daily active users
            ModifyNumberWithTransaction($"Main/DAU/{currentDate.Replace("/", "-").Replace("\"", "")}", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        }
    }

    /// <summary>
    /// Updates the average session duration on quit();
    /// </summary>
    /// <returns></returns>
    private IEnumerator IEUpdateAverageSessionDuration()
    {
        yield return new WaitUntil(() => !_isDoingOperation);
        _isDoingOperation = true;
        var wait = new WaitForSeconds(0.8f);
        _quitTime = Time.time;

        /// first we get the total sessions
        GetJSON($"{kUsername}/TotalSessions", gameObject.name, "ReturnData", "OnRequestFailed");
        yield return wait;
        OnRequestSuccess("Received total sessions: " + _generalData);
        if (_generalData.Equals("null"))
            _generalData = "0";

        var totalSessions = int.Parse(_generalData.Replace("\"", ""));
        _generalData = "";

        /// then we get the total sessions durations (in seconds)
        GetJSON($"{kUsername}/TotalSessionsDuration", gameObject.name, "ReturnData", "OnRequestFailed");
        yield return wait;

        if (_generalData.Equals("null"))
            _generalData = "\"0\"";

        var totalSessionsDuration = float.Parse(_generalData.Replace(".", ",").Replace("\"", ""));
        OnRequestSuccess(totalSessionsDuration.ToString());
        yield return wait;

        _generalData = "";

        /// now we calculate this sessions's duration
        var totalDuration = _quitTime - _startTime;
        totalSessionsDuration += totalDuration;
        PostJSON($"{kUsername}/TotalSessionsDuration", totalSessionsDuration.ToString(), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        var averageSessionsDuration = totalSessionsDuration / totalSessions;

        /// we post it, and also add the new totalsessionsduration to the database
        PostJSON($"{kUsername}/AverageSessionDuration", averageSessionsDuration.ToString(), gameObject.name, "OnRequestSuccess", "OnRequestFailed");

        _isDoingOperation = false;
    }

    /// <summary>
    /// Coins spent by day
    /// </summary>
    /// <param name="amount"></param>
    public void SpendCoins(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.IntData;
        ModifyNumberWithTransaction($"{kUsername}/CoinsSpent/D{_elapsedDays + 1}", e.IntData, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Coins earned by day
    /// </summary>
    /// <param name="amount"></param>
    public void EarnCoins(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.IntData;
        ModifyNumberWithTransaction($"{kUsername}/CoinsEarned/D{_elapsedDays + 1}", e.IntData, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Gems earned by day
    /// </summary>
    /// <param name="amount"></param>
    public void EarnGems(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.IntData;
        ModifyNumberWithTransaction($"{kUsername}/GemsEarned/D{_elapsedDays + 1}", e.IntData, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Gems accumulated by day
    /// </summary>
    /// <param name="amount"></param>
    public void AccumulateGems(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.IntData;
        ModifyNumberWithTransaction($"{kUsername}/GemsAccumulated/D{_elapsedDays + 1}", e.IntData, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Gets called when a building is placed
    /// </summary>
    public void PlaceRegularBuilding(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.StringData;
        StartCoroutine(IEAVerageBuildings(e.StringData));
    }

    /// <summary>
    /// Calculates the new DailyAverageBuildingsPlaced
    /// </summary>
    /// <returns></returns>
    IEnumerator IEAVerageBuildings(string building)
    {
        yield return new WaitUntil(() => !_isDoingOperation);

        _isDoingOperation = true;
        var wait = new WaitForSeconds(1f);
        ModifyNumberWithTransaction($"{kUsername}/TotalRegularBuildingsPlaced", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        yield return wait;

        ModifyNumberWithTransaction($"{kUsername}/BuildingsPlaced/D{_elapsedDays + 1}/{building}", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        GetJSON($"{kUsername}/TotalRegularBuildingsPlaced", gameObject.name, "ReturnData", "OnRequestFailed");
        yield return wait;

        int totalBuilding;
        try
        {
            totalBuilding = int.Parse(_generalData.Replace("\"", ""));
        }
        catch (SystemException exception)
        {
            totalBuilding = 0;
            OnRequestFailed("There was an error parsing: " + exception);
        }
        yield return wait;

        _generalData = "";
        GetJSON($"{kUsername}/Retention/TotalDaysActive", gameObject.name, "ReturnData", "OnRequestFailed");
        yield return wait;

        int daysActive = int.TryParse(_generalData.Replace("\"", ""), out daysActive) ? daysActive : 1;
        _generalData = "";
        var dailyAverageBUildingsPlaced = totalBuilding / daysActive;
        PostJSON($"{kUsername}/DailyAverageBuildingsPlaced", dailyAverageBUildingsPlaced.ToString().Replace("\"", ""), gameObject.name, "OnRequestSuccess", "OnRequestFailed");

        _isDoingOperation = false;
    }

    /// <summary>
    /// Sales made (by day)
    /// </summary>
    public void MakeSale(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.StringData;
        ModifyNumberWithTransaction($"{kUsername}/SalesDaily/D{_elapsedDays + 1}/{e.StringData}", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Daily Challenges complete  (by day)
    /// </summary>
    public void CompleteDailyChallenge(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.StringData;
        ModifyNumberWithTransaction($"{kUsername}/DailyChallengesComplete/D{_elapsedDays + 1}", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Score gained
    /// </summary>
    /// <param name="amount"></param>
    public void GainScore(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.FloatData;
        ModifyNumberWithTransaction($"{kUsername}/ScoreEarned/D{_elapsedDays + 1}", e.FloatData, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Score Accumulated
    /// </summary>
    /// <param name="amount"></param>
    public void AccumulateScore(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.FloatData;
        ModifyNumberWithTransaction($"{kUsername}/ScoreAccumulated/D{_elapsedDays + 1}", e.FloatData, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// $Moon Rewarded
    /// </summary>
    /// <param name="amount"></param>
    public void RewardMoon(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.IntData;
        ModifyNumberWithTransaction($"{kUsername}/MoonRewarded/D{_elapsedDays + 1}", e.IntData, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Spends $Moon
    /// </summary>
    /// <param name="amount"></param>
    public void SpendMoon(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.IntData;
        ModifyNumberWithTransaction($"{kUsername}/MoonSpent/D{_elapsedDays + 1}", e.IntData, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Player enters P2E tournament for today (can only do it once , for testing)
    /// </summary>
    public void EnterP2ETournament(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.StringData;
        StartCoroutine(IEEnterP2ETour());
    }

    private IEnumerator IEEnterP2ETour()
    {
        yield return new WaitUntil(() => !_isDoingOperation);
        _isDoingOperation = true;
        GetJSON($"{kUsername}/HasEnteredP2ETour/D{_elapsedDays + 1}", gameObject.name, "ReturnData", "OnRequestFailed");
        yield return new WaitForSeconds(1f);
        if (_generalData == "null")
        {
            PostJSON($"{kUsername}/HasEnteredP2ETour/D{_elapsedDays + 1}", "1", gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            ModifyNumberWithTransaction($"Main/P2EPlayers/D{_elapsedDays + 1}", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        }
        _isDoingOperation = false;
    }

    /// <summary>
    /// When an NFT is placed
    /// </summary>
    public void PlaceNFT(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.StringData;
        ModifyNumberWithTransaction($"{kUsername}/NFTsPlaced/D{_elapsedDays + 1}/{e.StringData}", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// When nft's owned get changed, they should be updated
    /// </summary>
    /// <param name="amount"></param>
    public void UpdateNFTsOwned(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.StringData;
        ModifyNumberWithTransaction($"{kUsername}/NFTsPlaced/D{_elapsedDays + 1}/{e.StringData}", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    private void OnApplicationQuit()
    {
        //PerformApplicationQuitFunctions();
    }

    public void PerformApplicationQuitFunctions(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.StringData;
        UpdateAverageSessionDuration();
        UpdateGemsAndCoins();
        UpdateScoreDailyMargin();
    }

    public void Quit()
    {
        //Application.Quit();
        //PerformApplicationQuitFunctions();
    }

    /// <summary>
    /// Updates score daily Margin
    /// </summary>
    public void UpdateScoreDailyMargin()
    {
        StartCoroutine(IEUpdateScoreDailyMargin());
    }

    private IEnumerator IEUpdateScoreDailyMargin()
    {
        _path = $"Main/ScoreDailyMargin/D{_elapsedDays + 1}";
        GetJSON($"{kUsername}/ScoreAccumulated/D{_elapsedDays + 1}", gameObject.name, "ReturnData", "OnRequestFailed");
        yield return new WaitForSeconds(1f);
        int score = int.TryParse(_generalData.Replace("\"", ""), out score) ? score : 0;
        ModifyNumberWithTransaction(_path, score, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    private void UpdateAverageSessionDuration()
    {
        StartCoroutine(IEUpdateAverageSessionDuration());
    }

    private void UpdateGemsAndCoins()
    {
        _path = $"{kUsername}/Balance/D{_elapsedDays + 1}/Coins";
        PostJSON(_path, kCoins.ToString(), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        _path = $"{kUsername}/Balance/D{_elapsedDays + 1}/Gems";
        PostJSON(_path, kGems.ToString(), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }


    public void GetMarketingData()
    {
        StartCoroutine(GetIP());
    }

    /// <summary>
    /// Not working currently
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private IEnumerator GetIP()
    {
        var www = UnityWebRequest.Get("https://api.ipify.org");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
            OnRequestFailed(www.error);
        else
            OnRequestSuccess(www.result.ToString());

        var ip = www.downloadHandler.text;
        var uri = $"https://ipapi.co/{ip}/json/";
        var webRequest = UnityWebRequest.Get(uri);
        var uriTotext = webRequest.downloadHandler.text;
        OnRequestSuccess("webRequest Made at ip: " + ip);
        yield return webRequest.SendWebRequest();

        var pages = uriTotext.Split('/');
        var page = pages.Length - 1;
        var ipApiData = IpApiData.CreateFromJSON(webRequest.downloadHandler.text);

        PostJSON($"{kUsername}/Country", ipApiData.CountryName, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Updates Sales daily by resource type
    /// </summary>
    /// <param name="resourceName"></param>
    public void MakeResourceSale(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.StringData;
        _path = $"{kUsername}/SalesDaily/D{_elapsedDays + 1}/ByType/{e.StringData}";
        ModifyNumberWithTransaction(_path, 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Updates progress resets daily by number and by terrain
    /// </summary>
    /// <param name="terrain"></param>
    public void UpdateProgressResets(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.StringData;
        _path = $"{kUsername}/ProgressResets/D{_elapsedDays + 1}";
        ModifyNumberWithTransaction(_path, 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        _path = $"{kUsername}/ProgressResets/D{_elapsedDays + 1}/{e.StringData}";
        ModifyNumberWithTransaction(_path, 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Adds the type of terrain chosen
    /// </summary>
    /// <param name="terrain"></param>
    public void TerrainChosen(AnalyticsEvent e)
    {
        _id = e._id;
        _data = e.StringData;
        _path = $"{kUsername}/TerrainChosen/D{_elapsedDays + 1}/{e.StringData}";
        ModifyNumberWithTransaction(_path, 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Call this as a callback
    /// </summary>
    /// <param name="data"></param>
    public void OnRequestSuccess(string data)
    {
        var returnEvent = new AnalyticsEvent(_id, AnalyticsEventReturnType.OperationSuccesful, _data);
        analysisManager.ProccessReturnResult(returnEvent);
    }

    /// <summary>
    /// Call this as a fallback
    /// </summary>
    /// <param name="error"></param>
    public void OnRequestFailed(string error)
    {
        var returnEvent = new AnalyticsEvent(_id, AnalyticsEventReturnType.OperationFailed, _data);
        analysisManager.ProccessReturnResult(returnEvent);
    }

    public void ReturnData(string data)
    {
        var e = new AnalyticsEvent(_id, AnalyticsEventReturnType.OperationSuccesful, _data);
        analysisManager.ProccessReturnResult(e);
        _generalData = data;
    }

    /*private IEnumerator EditorTest()
    {
        var wait = new WaitForSeconds(2f);
        _quitTime = Time.time;
        var totalSessions = "6";
        var totalSessionsInt = int.Parse(totalSessions);
        Debug.Log(totalSessionsInt);

        text.text = totalSessionsInt.ToString();
        yield return wait;

        var totalSessionDuration = "3.14124123";
        var fTotalSessionDuration = float.Parse(totalSessionDuration.Replace(".", ","));
        Debug.Log(fTotalSessionDuration);
        text.text = fTotalSessionDuration.ToString();
        yield return wait;

        var totalDuration = _quitTime - _startTime;
        fTotalSessionDuration += totalDuration;
        var averageSessionsDuration = fTotalSessionDuration / totalSessionsInt;
        Debug.Log(averageSessionsDuration);
    }*/
}

public class IpApiData
{
    public readonly string CountryName = string.Empty;

    public static IpApiData CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<IpApiData>(jsonString);
    }
}

