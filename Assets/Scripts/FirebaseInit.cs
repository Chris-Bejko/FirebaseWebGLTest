using System.Collections;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class FirebaseInit : MonoBehaviour
{
    public Text text;
    private string username = "ExampleUsername";
    public string currentDate;
    private string formattedDate;
    private int elapsedDays;
    string path;
    string generalData;
    float startTime;
    float quitTime;
    /// <summary>
    /// Test variables (coins, gems, etc)
    /// </summary>
    private int coins = 5;
    private int gems = 1;
    public static FirebaseInit Instance { get; private set; }
    [DllImport("__Internal")]
    private static extern void GetJSON(string path, string objectName, string callback, string fallback);
    [DllImport("__Internal")]
    private static extern void PostJSON(string path, string value, string objectName, string callback, string fallback);
    [DllImport("__Internal")]
    private static extern void ModifyNumberWithTransaction(string path, float amount, string objectName, string callback, string fallback);
    IEnumerator Start()
    {
        startTime = Time.time;
        generalData = "";
        WaitForSeconds wait = new WaitForSeconds(2f);
        currentDate = DateTime.Now.ToString("dd/MM/yy");
        text.text = "Waiting 2 seconds";
        yield return wait;
        GetJSON(string.Format("{0}/Retention/D1", username), gameObject.name, "PerformStartActions", "OnRequestFailed"); ///Player retention by days, and daily active users\
        //StartCoroutine(EditorTest());
    }

    /// <summary>
    /// Date Comparing to determine which day of activity it is for this Player
    /// </summary>
    /// <param name="date"></param>
    public void PerformStartActions(string date)
    {
        StartCoroutine(IEPerformStartActions(date));
    }
    private IEnumerator EditorTest()
    {
        var wait = new WaitForSeconds(2f);
        quitTime = Time.time;
        var totalSessions = "6";
        var totalSessionsInt = int.Parse(totalSessions);
        Debug.Log(totalSessionsInt);
        text.text = totalSessionsInt.ToString();
        yield return wait;
        string totalSessionDuration = "3.14124123";
        float fTotalSessionDuration = float.Parse(totalSessionDuration.Replace(".", ","));
        Debug.Log(fTotalSessionDuration);
        text.text = fTotalSessionDuration.ToString();
        yield return wait;
        float totalDuration = quitTime - startTime;
        fTotalSessionDuration += totalDuration;
        float averageSessionsDuration = fTotalSessionDuration / totalSessionsInt;
        Debug.Log(averageSessionsDuration);
    }
    /// <summary>
    /// Compares the last date the user logged in, determines Player retention by days (D1, D4 etc), and Daily Active Users (by date:amount)
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    IEnumerator IEPerformStartActions(string date)
    {
        //PostJSON(string.Format("{0}/Retention/TotalDaysActive", username), "0", gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        ///Update Daily Sessions and Total Sessions for this user
        ModifyNumberWithTransaction(string.Format("{0}/TotalSessions", username), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed"); ///Update total sessions
        if (date.Equals("null")) ///D1 does not exist, so this is day 1 for the username
        {
            path = string.Format("{0}/Retention/D1", username);
            text.text = "date is null";
            ///Adds the current date as D1 , since there is no D1 key
            PostJSON(path, currentDate, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            yield return new WaitForSeconds(0.5f);
            GetJSON(path, gameObject.name, "ReturnData", "OnRequestFailed");
            //yield return new WaitUntil(() => !generalData.Equals(""));
            yield return new WaitForSeconds(0.5f);
            ///Increases DAU by 1
            ModifyNumberWithTransaction(string.Format("Main/DAU/{0}", currentDate.Replace("/", "-").Replace("\"", "")), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            generalData = "";
            //yield return new WaitUntil(() => generalData.Equals(""));
        }
        ///First , we need to remove the {"} symbols from the retrieved date
        formattedDate = date.Replace("\"", "");
        ///Calculates the days between the first day and this day
        DateTime firstDateTime = DateTime.Parse(formattedDate, new CultureInfo("fr-FR"));
        DateTime currentDateTime = DateTime.Parse(currentDate, new CultureInfo("fr-FR"));
        elapsedDays = (int)(currentDateTime - firstDateTime).TotalDays;
        ModifyNumberWithTransaction(string.Format("{0}/SessionsDaily/D{1}", username, elapsedDays + 1), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed"); ///Update daily sessions
        ///Determines the path for this day (e.x D5), and tries to get it
        path = string.Format("{0}/Retention/D{1}", username, elapsedDays + 1);
        GetJSON(path, gameObject.name, "ReturnData", "OnRequestFailed");
        //yield return new WaitUntil(() => !generalData.Equals(""));
        yield return new WaitForSeconds(1f);
        OnRequestSuccess(generalData);
        if (generalData.Equals("null")) ///If the path is null, this is the first log in for today
        {
            ///We add the path, and increment DAU by 1
            text.text = "Path did not exist, adding it now: ";
            PostJSON(path, currentDate, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            yield return new WaitForSeconds(2f);
            ModifyNumberWithTransaction(username + "/Retention/TotalDaysActive", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            GetJSON(string.Format("{0}/Retention/TotalDaysActive", username), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            yield return new WaitForSeconds(2f);
            ModifyNumberWithTransaction(string.Format("Main/DAU/{0}", date.Replace("/", "-")), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed"); //IF the elapsed days were not 0 (meaning that this is a different day), we increment DAU by 1
        }
    }
    private IEnumerator IEUpdateAverageSessionDuration()
    {
        var wait = new WaitForSeconds(0.8f);
        quitTime = Time.time;
        ///First we get the total sessions
        GetJSON(string.Format("{0}/TotalSessions", username), gameObject.name, "ReturnData", "OnRequestFailed");
        yield return wait;
        OnRequestSuccess("Received total sessions: " + generalData);
        if (generalData.Equals("null"))
            generalData = "0";
        int totalSessions = int.Parse(generalData.Replace("\"", ""));
        generalData = "";
        ///Then we get the total sessions durations (in seconds)
        GetJSON(string.Format("{0}/TotalSessionsDuration", username), gameObject.name, "ReturnData", "OnRequestFailed");
        text.text = "Received totalSessionsDuration: " + generalData;
        yield return wait;
        if (generalData.Equals("null"))
            generalData = "\"0\"";
        float totalSessionsDuration = float.Parse(generalData.Replace(".", ",").Replace("\"", ""));
        OnRequestSuccess(totalSessionsDuration.ToString());
        yield return wait;
        generalData = "";
        ///Now we calculate this sessions's duration
        float totalDuration = quitTime - startTime;
        totalSessionsDuration += totalDuration;
        PostJSON(string.Format("{0}/TotalSessionsDuration", username), totalSessionsDuration.ToString(), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        float averageSessionsDuration = totalSessionsDuration / totalSessions;
        ///We post it, and also add the new totalSessionsDuration to the database
        PostJSON(string.Format("{0}/AverageSessionDuration", username), averageSessionsDuration.ToString(), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    public void SpendCoins(int amount)
    {
        ModifyNumberWithTransaction(string.Format("{0}/CoinsSpent/D{1}", username, elapsedDays + 1), amount, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    public void EarnCoins(int amount)
    {
        ModifyNumberWithTransaction(string.Format("{0}/CoinsEarned/D{1}", username, elapsedDays + 1), amount, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    public void AccumulateGems(int amount)
    {
        ModifyNumberWithTransaction(string.Format("{0}/GemsAccumulated/D{1}", username, elapsedDays + 1), amount, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    public void PlaceRegularBuilding()
    {
        StartCoroutine(IEAVerageBuildings());
    }
    /// <summary>
    /// Calculates the new DailyAverageBuildingsPlaced
    /// </summary>
    /// <returns></returns>
    IEnumerator IEAVerageBuildings()
    {
        var wait = new WaitForSeconds(1f);
        ModifyNumberWithTransaction(string.Format("{0}/TotalRegularBuildingsPlaced", username), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        GetJSON(string.Format("{0}/TotalRegularBuildingsPlaced", username), gameObject.name, "ReturnData", "OnRequestFailed");
        yield return wait;
        var totalBuilding = int.Parse(generalData.Replace("\"", ""));
        generalData = "";
        GetJSON(string.Format("{0}/Retention/TotalDaysActive", username), gameObject.name, "ReturnData", "OnRequestFailed");
        yield return wait;
        var daysActive = int.Parse(generalData.Replace("\"", ""));
        generalData = "";
        var dailyAverageBUildingsPlaced = totalBuilding / daysActive;
        PostJSON(string.Format("{0}/DailyAverageBuildingsPlaced", username), dailyAverageBUildingsPlaced.ToString(), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    public void MakeSale()
    {
        ModifyNumberWithTransaction(string.Format("{0}/SalesDaily/D{1}", username, elapsedDays + 1), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    public void CompleteDailyChallenge()
    {
        ModifyNumberWithTransaction(string.Format("{0}/DailyChallengesComplete", username), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }
    public void AccumulateScore(int amount)
    {
        ModifyNumberWithTransaction(string.Format("{0}/ScoreAccumulated", username), amount, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }
    public void RewardMoon(int amount)
    {
        ModifyNumberWithTransaction(string.Format("{0}/MoonRewarded", username), amount, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }
    public void SpendMoon(int amount)
    {
        ModifyNumberWithTransaction(string.Format("{0}/MoonSpent", username), amount, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    public void EnterP2ETournament()
    {
        StartCoroutine(IEEnterP2ETour());
    }

    private IEnumerator IEEnterP2ETour()
    {
        GetJSON(string.Format("{0}/HasEnteredP2ETour/D{1}", username, elapsedDays + 1), gameObject.name, "ReturnData", "OnRequestFailed");
        yield return new WaitForSeconds(1f);
        if (generalData == "null")
        {
            PostJSON(string.Format("{0}/HasEnteredP2ETour/D{1}", username, elapsedDays + 1), "1", gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            ModifyNumberWithTransaction(string.Format("Main/P2EPlayers/D{0}", elapsedDays + 1), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        }
    }

    public void PlaceNFT()
    {
        ModifyNumberWithTransaction(string.Format("{0}/NFTsPlaced", username),1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    public void UpdateNFTsOwned(int amount)
    {
        PostJSON(string.Format("{0}/NFTsOwned", username), amount.ToString(), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }
    private void OnApplicationQuit()
    {
        PerformApplicationQuitFunctions();
    }

    private void PerformApplicationQuitFunctions()
    {
        UpdateAverageSessionDuration();
        UpdateGemsAndCoins();
    }
    public void Quit()
    {
        //Application.Quit();
        PerformApplicationQuitFunctions();

    }
    private void UpdateAverageSessionDuration()
    {
        StartCoroutine(IEUpdateAverageSessionDuration());
    }

    private void UpdateGemsAndCoins()
    {
        path = string.Format("{0}/Balance/D{1}/Coins", username, elapsedDays + 1);
        PostJSON(path, coins.ToString(), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        path = string.Format("{0}/Balance/D{1}/Gems", username, elapsedDays + 1);
        PostJSON(path, gems.ToString(), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    public void ReturnData(string data)
    {
        text.text = "Data received: " + data;
        generalData = data;
    }

    public void OnRequestSuccess(string data)
    {
        text.color = Color.green;
        text.text = data;
    }

    public void OnRequestFailed(string error)
    {
        text.color = Color.red;
        text.text = error;
    }


    private void Awake()
    {
        Instance = this;
    }

}
