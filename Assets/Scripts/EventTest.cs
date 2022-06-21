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
    void Start()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.StartSession, "Kitsomo");
        analysisManager.ProccessResult(e);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpendCoins()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.SpendCoins, 5);
        analysisManager.ProccessResult(e);
    }
    public void EarnCoins()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.EarnCoins, 5);
        analysisManager.ProccessResult(e);
    }

    public void EarnGems()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.EarnGems, 5);
        analysisManager.ProccessResult(e);
    }
    public void AccumulateGems()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.AccumulateGems, 5);
        analysisManager.ProccessResult(e);
    }

    public void MakeSale()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.MakeSale, "ExampleName");
        analysisManager.ProccessResult(e);
    }
    public void PlaceRegularBuilding()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.PlaceRegularBuilding, "ExampleName");
        analysisManager.ProccessResult(e);
    }
    public void ChooseTerrainType()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.ChooseTerrainType, "ExampleName");
        analysisManager.ProccessResult(e);
    }
    public void ResetProgress()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.ResetProgress, "ExampleName");
        analysisManager.ProccessResult(e);
    }
    public void GainScore()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.GainScore, 4);
        analysisManager.ProccessResult(e);
    }
    public void AccumulateScore()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.AccumulateScore, 4);
        analysisManager.ProccessResult(e);
    }
    public void EnterP2ETournament()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.EnterP2ETournament, "Test");
        analysisManager.ProccessResult(e);
    }
    public void CompleteChallenge()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.CompleteChallenge, "Test");
        analysisManager.ProccessResult(e);
    }
    public void RewardMOON()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.RewardMOON, 4);
        analysisManager.ProccessResult(e);
    }
    public void SpendMOON()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.SpendMOON, 4);
        analysisManager.ProccessResult(e);
    }
    public void GainNFT()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.GainNFT, "Test");
        analysisManager.ProccessResult(e);
    }
    public void PlaceNFT()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.PlaceNFT, "Test");
        analysisManager.ProccessResult(e);
    }
    public void EndSession()
    {
        var e = new AnalyticsEvent(AnalyticsEventID.EndSession);
        analysisManager.ProccessResult(e);
    }
    
}
