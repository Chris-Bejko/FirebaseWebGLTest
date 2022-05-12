using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JavascriptHook : MonoBehaviour
{
    public Image squareImage;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            TintRed();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            TintGreen();
        }
    }

    public void TintRed()
    {
        squareImage.color = Color.red;
    }
    public void TintGreen()
    {
        squareImage.color = Color.green;
    }
}
