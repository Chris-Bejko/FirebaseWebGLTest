using System.Collections;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class FirebaseInit : MonoBehaviour
{
    public Text text;
    private string username = "Example";
    public string currentDate;
    string path;
    string generalData;
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
        //GetJSON(string.Format("{0}/retention/D{1}", username, currentDate), gameObject.name, "UpdateDAU", "OnRequestFailed"); 
        yield return wait;
        GetJSON(string.Format("{0}/retention/D1", username), gameObject.name, "CompareDates", "OnRequestFailed"); ///Player retention by days, and daily active users
    }

    /// <summary>
    /// Date Comparing to determine which day of activity it is for this Player
    /// </summary>
    /// <param name="date"></param>
    public async void CompareDates(string date)
    {      
        StartCoroutine(IECompareDates(date));
    }

    IEnumerator IECompareDates(string date)
    {
        if (date.Equals("null"))
        {
            path = string.Format("{0}/retention/D1", username);
            text.text = "date is null";
            ///Adds the current date as D1 , since there is no D1 key
            PostJSON(path, currentDate, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            yield return new WaitForSeconds(0.5f);
            GetJSON(path, gameObject.name, "ReturnData", "OnRequestFailed");
            yield return new WaitUntil(() => generalData != "");
            ModifyNumberWithTransaction(string.Format("Main/DAU/{0}", currentDate.Replace("/", "-")), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            generalData = "";
        }
        yield return new WaitUntil(() => generalData == "");
        ///First , we need to remove the {"} symbols from the retrieved date
        var formattedDate = date.Substring(1, date.Length - 2);
        ///Calculates the days between the first day and this day
        DateTime lastDateTime = DateTime.Parse(formattedDate, new CultureInfo("fr-FR"));
        DateTime currentDateTime = DateTime.Parse(currentDate, new CultureInfo("fr-FR"));
        int elapsedDays = (int)(currentDateTime - lastDateTime).TotalDays;
        ///Posts the path along with the calculated value
        path = string.Format("{0}/retention/D{1}", username, elapsedDays + 1);
        GetJSON(path, gameObject.name, "ReturnData", "OnRequestFailed");
        yield return new WaitUntil(() => generalData != "");
        if (generalData == "null")
        {
            text.text = "Date did not exist, adding it now: ";
            PostJSON(path, currentDate, gameObject.name, "OnRequestSuccess", "OnRequestFailed");
            ModifyNumberWithTransaction(string.Format("Main/DAU/{0}", date.Replace("/", "-")), 1, gameObject.name, "OnRequestSuccess", "OnRequestFailed"); //IF the elapsed days were not 0 (meaning that this is a different day), we increment DAU by 1
            generalData = "";
        }
        else
        {
            OnRequestSuccess("PATH ALREADY EXISTS, DAU&PATH NOT UPDATED");
        }
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
