using System.Collections;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using UnityEngine.Networking;

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

    bool isDoingOperation;



    /// <summary>
    /// Test variables (coins, gems, etc)
    /// </summary>
    private int coins = 5;
    private int gems = 1;
    private string url = "http://ip-api.com/json";
    public static FirebaseInit Instance { get; private set; }

    /// <summary>
    /// Extern methods from jslib plugin
    /// </summary>
    [DllImport("__Internal")]
    private static extern void GetJSON(string path, string objectName, string callback, string fallback);
    [DllImport("__Internal")]
    private static extern void PostJSON(string path, string value, string objectName, string callback, string fallback);
    [DllImport("__Internal")]
    private static extern void ModifyNumberWithTransaction(string path, float amount, string objectName, string callback, string fallback);
    IEnumerator Start()
    {
        isDoingOperation = false;
        startTime = Time.time;
        generalData = "";
        WaitForSeconds wait = new WaitForSeconds(2f);
        currentDate = DateTime.Now.ToString("dd/MM/yy");
        text.text = "Waiting 2 seconds";
        yield return wait;
        GetJSON(string.Format("{0}/Retention/D1", username), gameObject.name, "PerformStartActions", "OnRequestFailed"); ///Player retention by days, and daily active users\
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
        ModifyNumberWithTransaction(string.Format("{0}/TotalSessions", username), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed"); ///Update total sessions
        var wait = new WaitForSeconds(1f);
        var day1 = date;
        if (date.Equals("null")) ///First login ever
        {
            date = currentDate;
            day1 = currentDate;
            PostJSON(string.Format("{0}/Retention/D1", username), day1.ToString().Replace("/", "-"), gameObject.name, "ReturnData", "OnRequestFailed"); ///Update Retention
            ModifyNumberWithTransaction(username + "/Retention/TotalDaysActive", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed"); ///Update Total Days Active
            ModifyNumberWithTransaction(string.Format("Main/DAU/{0}", date.Replace("/", "-").Replace("\"", "")), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed"); ///Update Daily Active users
        }
        formattedDate = day1.Replace("\"", "");
        DateTime firstDateTime = DateTime.Parse(formattedDate, new CultureInfo("fr-FR"));
        DateTime currentDateTime = DateTime.Parse(currentDate, new CultureInfo("fr-FR"));
        elapsedDays = (int)(currentDateTime - firstDateTime).TotalDays;
        GetJSON(string.Format("{0}/Retention/D{1}", username, elapsedDays + 1), gameObject.name, "ReturnData", "OnRequestFailed");
        yield return wait;
        if (generalData.Equals("null")) //First login for today
        {
            PostJSON(string.Format("{0}/Retention/D{1}", username, elapsedDays + 1), currentDate.Replace("/", "-"), gameObject.name, "ReturnData", "OnRequestFailed"); ///Update Retention
            ModifyNumberWithTransaction(username + "/Retention/TotalDaysActive", 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed"); ///Update Total Days Active;
            ModifyNumberWithTransaction(string.Format("Main/DAU/{0}", currentDate.Replace("/", "-").Replace("\"", "")), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed"); ///Update Daily Active Users
        }
    }
    /// <summary>
    /// Updates the average session duration on quit();
    /// </summary>
    /// <returns></returns>
    private IEnumerator IEUpdateAverageSessionDuration()
    {
        yield return new WaitUntil(() => !isDoingOperation);
        isDoingOperation = true;
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
        isDoingOperation = false;
    }

    /// <summary>
    /// Coins spent by day
    /// </summary>
    /// <param name="amount"></param>
    public void SpendCoins(int amount)
    {
        ModifyNumberWithTransaction(string.Format("{0}/CoinsSpent/D{1}", username, elapsedDays + 1), amount, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Coins earned by day
    /// </summary>
    /// <param name="amount"></param>
    public void EarnCoins(int amount)
    {
        ModifyNumberWithTransaction(string.Format("{0}/CoinsEarned/D{1}", username, elapsedDays + 1), amount, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Gems earned by day
    /// </summary>
    /// <param name="amount"></param>
    public void AccumulateGems(int amount)
    {
        ModifyNumberWithTransaction(string.Format("{0}/GemsAccumulated/D{1}", username, elapsedDays + 1), amount, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }
    /// <summary>
    /// Gets called when a building is placed
    /// </summary>
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
        yield return new WaitUntil(() => !isDoingOperation);
        isDoingOperation = true;
        var wait = new WaitForSeconds(1f);
        ModifyNumberWithTransaction(string.Format("{0}/TotalRegularBuildingsPlaced", username), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        yield return wait;
        GetJSON(string.Format("{0}/TotalRegularBuildingsPlaced", username), gameObject.name, "ReturnData", "OnRequestFailed");
        yield return wait;
        int totalBuilding;
        try
        {
            totalBuilding = int.Parse(generalData.Replace("\"", ""));
        }
        catch (SystemException exception)
        {
            totalBuilding = 0;
            OnRequestFailed("There was an error parsing: " + exception);
        }
        yield return wait;
        generalData = "";
        GetJSON(string.Format("{0}/Retention/TotalDaysActive", username), gameObject.name, "ReturnData", "OnRequestFailed");
        yield return wait;
        int daysActive = int.TryParse(generalData.Replace("\"", ""), out daysActive) ? daysActive : 1;
        generalData = "";
        var dailyAverageBUildingsPlaced = totalBuilding / daysActive;
        PostJSON(string.Format("{0}/DailyAverageBuildingsPlaced", username), dailyAverageBUildingsPlaced.ToString().Replace("\"", ""), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        isDoingOperation = false;
    }
    /// <summary>
    /// Sales made (by day)
    /// </summary>
    public void MakeSale()
    {
        ModifyNumberWithTransaction(string.Format("{0}/SalesDaily/D{1}", username, elapsedDays + 1), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }
    /// <summary>
    /// Daily Challenges complete  (by day)
    /// </summary>
    public void CompleteDailyChallenge()
    {
        ModifyNumberWithTransaction(string.Format("{0}/DailyChallengesComplete/D{1}", username, elapsedDays + 1), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }
    /// <summary>
    /// Score gained
    /// </summary>
    /// <param name="amount"></param>
    public void AccumulateScore(int amount)
    {
        ModifyNumberWithTransaction(string.Format("{0}/ScoreAccumulated/D{1}", username, elapsedDays + 1), amount, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }
    /// <summary>
    /// $Moon Rewarded
    /// </summary>
    /// <param name="amount"></param>
    public void RewardMoon(int amount)
    {
        ModifyNumberWithTransaction(string.Format("{0}/MoonRewarded/D{1}", username, elapsedDays + 1), amount, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }
    /// <summary>
    /// Spends $Moon
    /// </summary>
    /// <param name="amount"></param>
    public void SpendMoon(int amount)
    {
        ModifyNumberWithTransaction(string.Format("{0}/MoonSpent/D{1}", username, elapsedDays + 1), amount, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }
    /// <summary>
    /// Player enters P2E tournament for today (can only do it once , for testing)
    /// </summary>
    public void EnterP2ETournament()
    {
        StartCoroutine(IEEnterP2ETour());
    }

    private IEnumerator IEEnterP2ETour()
    {
        yield return new WaitUntil(() => !isDoingOperation);
        isDoingOperation = true;
        GetJSON(string.Format("{0}/HasEnteredP2ETour/D{1}", username, elapsedDays + 1), gameObject.name, "ReturnData", "OnRequestFailed");
        yield return new WaitForSeconds(1f);
        if (generalData == "null")
        {
            PostJSON(string.Format("{0}/HasEnteredP2ETour/D{1}", username, elapsedDays + 1), "1", gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            ModifyNumberWithTransaction(string.Format("Main/P2EPlayers/D{0}", elapsedDays + 1), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        }
        isDoingOperation = false;
    }


    /// <summary>
    /// When an NFT is placed
    /// </summary>
    public void PlaceNFT()
    {
        ModifyNumberWithTransaction(string.Format("{0}/NFTsPlaced/D{1}", username, elapsedDays + 1), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// When nft's owned get changed, they should be updated
    /// </summary>
    /// <param name="amount"></param>
    public void UpdateNFTsOwned(int amount)
    {
        PostJSON(string.Format("{0}/NFTsOwned", username), amount.ToString(), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }


    private void OnApplicationQuit()
    {
        //PerformApplicationQuitFunctions();
    }

    private void PerformApplicationQuitFunctions()
    {
        UpdateAverageSessionDuration();
        UpdateGemsAndCoins();
        UpdateScoreDailyMargin();
    }

    public void Quit()
    {
        //Application.Quit();
        PerformApplicationQuitFunctions();

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
        path = string.Format("Main/ScoreDailyMargin/D{0}", elapsedDays + 1);
        GetJSON(string.Format("{0}/ScoreAccumulated/D{1}", username, elapsedDays + 1), gameObject.name, "ReturnData", "OnRequestFailed");
        yield return new WaitForSeconds(1f);
        int score = int.TryParse(generalData.Replace("\"", ""), out score) ? score : 0;
        ModifyNumberWithTransaction(path, score, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
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

    public void GetMarketingData(string url)
    {
        StartCoroutine(GetIP(url));
    }
    /// <summary>
    /// Not working currently
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private IEnumerator GetIP(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get("https://api.ipify.org");
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            OnRequestFailed(www.error);
        }
        else
        {
            OnRequestSuccess(www.result.ToString());
        }
        string ip = www.downloadHandler.text;
        string uri = $"https://ipapi.co/{ip}/json/";
        UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        string uriTotext = webRequest.downloadHandler.text;
        OnRequestSuccess("webRequest Made at ip: " + ip);
        yield return webRequest.SendWebRequest();
        string[] pages = uriTotext.Split('/');
        int page = pages.Length - 1;
        IpApiData ipApiData = IpApiData.CreateFromJSON(webRequest.downloadHandler.text);
        PostJSON(string.Format("{0}/Country", username), ipApiData.country_name, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Updates Sales daily by resource type
    /// </summary>
    /// <param name="resourceName"></param>
    public void MakeResourceSale(string resourceName)
    {
        //resourceName = resourceName.Replace("$", ""); //Firebase does not allow $ signs
        resourceName = "resource_farm";///Test, this will be assigned on call
        path = string.Format("{0}/SalesDaily/D{1}/ByType/{2}", username, elapsedDays + 1, resourceName);
        ModifyNumberWithTransaction(path, 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }
    /// <summary>
    /// Updates progress resets daily by number and by terrain
    /// </summary>
    /// <param name="terrain"></param>
    public void UpdateProgressResets(string terrain)
    {
        terrain = "exampleTerrain";
        path = string.Format("{0}/ProgressResets/D{1}", username, elapsedDays + 1);
        ModifyNumberWithTransaction(path, 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        path = string.Format("{0}/ProgressResets/D{1}/{2}", username, elapsedDays + 1, terrain);
        ModifyNumberWithTransaction(path, 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }
    /// <summary>
    /// Adds the type of terrain chosen
    /// </summary>
    /// <param name="terrain"></param>
    public void TerrainChosen(string terrain)
    {
        terrain = "exampleTerrain";
        path = string.Format("{0}/TerrainChosen/D{1}/{2}", username, elapsedDays + 1, terrain);
        ModifyNumberWithTransaction(path, 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
    }

    /// <summary>
    /// Call this as a callback
    /// </summary>
    /// <param name="data"></param>
    public void OnRequestSuccess(string data)
    {
        text.color = Color.green;
        text.text = data;
    }
    /// <summary>
    /// Call this as a fallback
    /// </summary>
    /// <param name="error"></param>
    public void OnRequestFailed(string error)
    {
        text.color = Color.red;
        text.text = error;
    }


    private void Awake()
    {
        Instance = this;
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
}

public class IpApiData
{
    public string country_name;

    public static IpApiData CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<IpApiData>(jsonString);
    }


}