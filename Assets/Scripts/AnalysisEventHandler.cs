// Copyright (C) 2022 Myria - All rights reserved
using System.Collections.Generic;
using UnityEngine;

public partial class AnalysisManager : MonoBehaviour
{
    public delegate void AnalyticsEventHandler(AnalyticsEvent id);
    public delegate void AnalyticsEventHandlerReturn(AnalyticsEvent id); 
    private readonly Dictionary<AnalyticsEventID, AnalyticsEventHandler> Handlers = new();
    private readonly Dictionary<AnalyticsEventReturnType, AnalyticsEventHandlerReturn> ReturnHandlers = new();

    public void RegisterHandler(AnalyticsEventID id, AnalyticsEventHandler handler)
    {
        Handlers.Add(id, handler);
    }

    public void RegisterReturnTypeHandler(AnalyticsEventReturnType id, AnalyticsEventHandlerReturn handler)
    {
        ReturnHandlers.Add(id, handler);
    }

    public void RegisterHandlers()
    {
        Debug.Log("HandlersGettingRegisterd");
        RegisterHandler(AnalyticsEventID.StartSession, HandlerStartSession);
        RegisterHandler(AnalyticsEventID.UpdateSessionProgress, HandlerUpdateSession);
        RegisterHandler(AnalyticsEventID.EndSession, HandlerEndSession);

        RegisterHandler(AnalyticsEventID.SpendCoins, HandlerSpendCoins);
        RegisterHandler(AnalyticsEventID.EarnCoins, HandlerEarnCoins);
        RegisterHandler(AnalyticsEventID.EarnGems, HandlerEarnGems);
        RegisterHandler(AnalyticsEventID.AccumulateGems, HandlerAccumulateGems);

        RegisterHandler(AnalyticsEventID.PlaceRegularBuilding, HandlerPlaceRegularBuilding);
        RegisterHandler(AnalyticsEventID.ChooseTerrainType, HandlerChooseTerrainType);
        RegisterHandler(AnalyticsEventID.ResetProgress, HandlerResetProgress);
        RegisterHandler(AnalyticsEventID.MakeSale, HandlerMakeSale);
        RegisterHandler(AnalyticsEventID.GainScore, HandlerGainScore);
        RegisterHandler(AnalyticsEventID.CompleteChallenge, HandlerCompleteChallenge);
        RegisterHandler(AnalyticsEventID.EnterP2ETournament, HandlerEnterP2ETournament);
        RegisterHandler(AnalyticsEventID.AccumulateScore, HandlerAccumulateScore);

        RegisterHandler(AnalyticsEventID.RewardMOON, HandlerRewardMOON);
        RegisterHandler(AnalyticsEventID.SpendMOON, HandlerSpendMOON);
        RegisterHandler(AnalyticsEventID.GainNFT, HandlerGainNFT);
        RegisterHandler(AnalyticsEventID.PlaceNFT, HandlerPlaceNFT);

        // return types from firebase
        RegisterReturnTypeHandler(AnalyticsEventReturnType.OperationSuccesful, UpdateSuccessful);
        RegisterReturnTypeHandler(AnalyticsEventReturnType.OperationFailed, UpdateFailure);
    }

    public void ProccessResult(AnalyticsEvent e)
    {
        Debug.Log(e._id);
        if(Handlers.ContainsKey(e._id))
            Handlers[e._id](e);
    }

    public void ProccessReturnResult(AnalyticsEvent e)
    {
        if (ReturnHandlers.ContainsKey(e._return))
            ReturnHandlers[e._return](e);
    }

    /// <summary>
    /// When process is successful:
    /// </summary>
    /// <param name="e"></param>
    public void UpdateSuccessful(AnalyticsEvent e)
    {
        //Debug.Log(e._id);
    }

    /// <summary>
    /// When a process is failed: 
    /// </summary>
    /// <param name="e"></param>
    public void UpdateFailure(AnalyticsEvent e)
    {
        ProccessResult(e);
    }
}
