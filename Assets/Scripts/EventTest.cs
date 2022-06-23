using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTest : MonoBehaviour
{
    AnalysisManager analysisManager;

    private void Awake()
    {
        analysisManager = FindObjectOfType<AnalysisManager>();
    }
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {

    }

    public void RewardMOON()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.RewardMoon, 4);
        analysisManager.ProccessResult(e);
    }
    public void SpendMOON()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.SpendMoon, 4);
        analysisManager.ProccessResult(e);
    }
    public void GainNFT()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.NFTOwned, "test_nft");
        analysisManager.ProccessResult(e);
    }
    public void PlaceNFT()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.NFTPlaced, "Test");
        analysisManager.ProccessResult(e);
    }
}
