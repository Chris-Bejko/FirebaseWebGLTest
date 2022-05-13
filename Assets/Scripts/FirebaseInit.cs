using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseInit : MonoBehaviour
{
    public Text text;
    public static FirebaseInit Instance { get; private set; }
    [DllImport("__Internal")]
    private static extern void GetJSON(string path, string objectName, string callback, string fallback);
    [DllImport("__Internal")]
    private static extern void PushJSON(string path, string value, string objectName, string callback, string fallback);
    [DllImport("__Internal")]
    private static extern void PostJSON(string path, string value, string objectName, string callback, string fallback);
    IEnumerator Start()
    {
        text.text = "Start worked";
        GetJSON(path: "example", gameObject.name, callback: "OnRequestSuccess", fallback: "OnRequestFailed");
        yield return new WaitForSeconds(2f);
        PushJSON(path: "example1", value: "value1", objectName: gameObject.name, callback: "OnRequestSuccess", fallback: "OnRequestFailed");
        yield return new WaitForSeconds(2f);
        PostJSON(path: "example2", value: "value2", objectName: gameObject.name, callback: "OnRequestSuccess", fallback: "OnRequestFailed");
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

    public void MyFunction()
    {
        text.text = "My function success";
    }


    private void Awake()
    {
        Instance = this;
    }
    public void MessageFromJavascript(string message)
    {
        Debug.Log(message);
        text.text = "Text Changed Successfully to " + message;
    }
}
