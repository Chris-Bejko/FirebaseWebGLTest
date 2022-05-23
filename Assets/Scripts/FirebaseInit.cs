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

    /// <summary>
    /// Test variables (coins, gems, etc)
    /// </summary>
    private int coins = 5;
    private int gems = 1;
    public static FirebaseInit Instance { get; private set; }
    [DllImport("__Internal")]
    private static extern void GetJSON(string path, string objectName, string callback, string fallback);
    [DllImport("__Internal")]
    private static extern void PushJSON(string path, string key, string value, string objectName, string callback, string fallback);
    [DllImport("__Internal")]
    private static extern void PostJSON(string path, string value, string objectName, string callback, string fallback);
    [DllImport("__Internal")]
    private static extern void ModifyNumberWithTransaction(string path, float amount, string objectName, string callback, string fallback);
    IEnumerator Start()
    {
        generalData = "";
        WaitForSeconds wait = new WaitForSeconds(2f);
        currentDate = DateTime.Now.ToString("dd/MM/yy");
        text.text = "Waiting 2 seconds";
        yield return wait;
        GetJSON(string.Format("Retention/{0}/D1", username), gameObject.name, "CompareDates", "OnRequestFailed"); ///Player retention by days, and daily active users
    }

    /// <summary>
    /// Date Comparing to determine which day of activity it is for this Player
    /// </summary>
    /// <param name="date"></param>
    public void CompareDates(string date)
    {
        StartCoroutine(IECompareDates(date));
    }

    /// <summary>
    /// Compares the last date the user logged in, determines Player retention by days (D1, D4 etc), and Daily Active Users (by date:amount)
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    IEnumerator IECompareDates(string date)
    {
        if (date.Equals("null")) ///D1 does not exist, so this is day 1 for the username
        {
            path = string.Format("Retention/{0}/D1", username);
            text.text = "date is null";
            ///Adds the current date as D1 , since there is no D1 key
            PostJSON(path, currentDate, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            yield return new WaitForSeconds(0.5f);
            GetJSON(path, gameObject.name, "ReturnData", "OnRequestFailed");
            yield return new WaitUntil(() => !generalData.Equals(""));
            ///Increases DAU by 1
            ModifyNumberWithTransaction(string.Format("Main/DAU/{0}", currentDate.Replace("/", "-")), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            generalData = "";
            //UpdateGemsAndCoins();
            yield return new WaitUntil(() => generalData == "");
        }

        ///First , we need to remove the {"} symbols from the retrieved date
        formattedDate = date.Substring(1, date.Length - 2);
        ///Calculates the days between the first day and this day
        DateTime firstDateTime = DateTime.Parse(formattedDate, new CultureInfo("fr-FR"));
        DateTime currentDateTime = DateTime.Parse(currentDate, new CultureInfo("fr-FR"));
        elapsedDays = (int)(currentDateTime - firstDateTime).TotalDays;
        ///Determines the path for this day (e.x D5), and tries to get it
        path = string.Format("Retention/{0}/D{1}", username, elapsedDays + 1);
        GetJSON(path, gameObject.name, "ReturnData", "OnRequestFailed");
        yield return new WaitUntil(() => !generalData.Equals(""));
        OnRequestSuccess(generalData);
        if (generalData.Equals("null")) ///If the path is null, this is the first log in for today
        {
            ///We add the path, and increment DAU by 1
            text.text = "Path did not exist, adding it now: ";
            PostJSON(path, currentDate, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            ModifyNumberWithTransaction(string.Format("Main/DAU/{0}", date.Replace("/", "-")), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed"); //IF the elapsed days were not 0 (meaning that this is a different day), we increment DAU by 1
            generalData = "";
        }
    }
    private void OnApplicationQuit()
    {
        PerformApplicationQuitFunctions();
    }

    private void PerformApplicationQuitFunctions()
    {
        UpdateGemsAndCoins();

    }
    public void Quit()
    {
        Application.Quit();
    }
    private void UpdateGemsAndCoins()
    {
        path = string.Format("Balance/{0}/D{1}/coins", username, elapsedDays + 1);
        PostJSON(path, coins.ToString(), gameObject.name, "OnRequestSuccess", "OnRequestFailed");
        path = string.Format("Balance/{0}/D{1}/gems", username, elapsedDays + 1);
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
