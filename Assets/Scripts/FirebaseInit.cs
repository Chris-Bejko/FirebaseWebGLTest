using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseInit : MonoBehaviour
{
    public Text text;

    [DllImport("_Internal")]
    private static extern void GetJSON(string path, string objectName, string callback, string fallback);


    void Start()
    {
        text.text = "Start worked";
        GetJSON(path: "example", gameObject.name, callback: "OnRequestSuccess", fallback: "OnRequestFailed");
    }

    void OnRequestSuccess(string data)
    {
        text.color = Color.green;
        text.text = data;
    }

    void OnRequestFailed(string error)
    {
        text.color = Color.red;
        text.text = error;
    }
}
