// Copyright (C) 2022 Myria - All rights reserved
using UnityEngine;
using UnityEngine.UI;

public partial class AnalysisManager : MonoBehaviour
{
    [SerializeField] private float dataCollectionInterval;
    [SerializeField] Text text;
    private float _dataCollectionTimer;

    private void Awake()
    {
        RegisterHandlers();
        EnableAnalyticsCollection();
    }

    private void Update()
    {
        _dataCollectionTimer += Time.deltaTime;
        text.text = _dataCollectionTimer.ToString();
        if (_dataCollectionTimer > dataCollectionInterval)
        {
            _dataCollectionTimer = 0;
            text.text = "UpdatedSession";
            Debug.Log("UpdatedSession");
            var e = new AnalyticsEvent(AnalyticsEventID.UpdateSessionProgress, "Kitsomo");
            ProccessResult(e);
        }
    }

    public void EnableAnalyticsCollection()
    {
        _dataCollectionTimer = Time.time;
    }

    /// <summary>
    /// On session start, we update some data automatically: 
    /// </summary>
    public static void HandlerStartSession(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.StartSession(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerUpdateSession(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.PerformApplicationQuitFunctions(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerEndSession(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.PerformApplicationQuitFunctions(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerSpendCoins(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.SpendCoins(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerEarnCoins(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.EarnCoins(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerEarnGems(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.EarnGems(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerAccumulateGems(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.AccumulateGems(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerPlaceRegularBuilding(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.PlaceRegularBuilding(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerChooseTerrainType(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.TerrainChosen(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerResetProgress(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.UpdateProgressResets(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerMakeSale(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.MakeSale(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerGainScore(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.GainScore(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerCompleteChallenge(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.CompleteDailyChallenge(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerAccumulateScore(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.AccumulateScore(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerRewardMOON(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.RewardMoon(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerSpendMOON(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.SpendMoon(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerEnterP2ETournament(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.EnterP2ETournament(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerGainNFT(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.UpdateNFTsOwned(e);
    }

    /// <summary>
    /// ...
    /// </summary>
    /// <param name="e"></param>
    public static void HandlerPlaceNFT(AnalyticsEvent e)
    {
        FirebaseSDK.Instance.PlaceNFT(e);
    }
}
