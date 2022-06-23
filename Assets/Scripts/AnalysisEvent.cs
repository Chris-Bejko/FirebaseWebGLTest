// Copyright (C) 2022 Myria - All rights reserved
using UnityEngine.Assertions;


public enum AnalyticsEventID
{
    RewardMoon = 0,
    SpendMoon,
    NFTOwned,
    NFTPlaced,
    TotalNFTValue,
    LoadWorldMap,
    SelectTile,
    SelectTileConfirmed,
    ConfirmName,
    CreateTown,
    LoadIntroductionPopup,
    CloseIntroductionPopup,
    CompleteSale,
    ReachFuelProduction,
    ClearXObstacles,
    EnterFreeDateTour,
    EnterPaidDateTour,
    RewardsCollected,
    RewardsExpired,
}

public enum DatabaseEventID
{
    ///3-6  Currency
    SpendCoins = 3,
    EarnCoins = 4,
    EarnGems = 5,
    AccumulateGems = 6,

    ///7-14 Progress
    PlaceRegularBuilding = 7,
    ChooseTerrainType = 8,
    ResetProgress = 9,
    MakeSale = 10,
    GainScore = 11,
    CompleteChallenge = 12,
    AccumulateScore = 13,
    EnterP2ETournament = 14,

    ///15-18 Moon&NFT
    GainNFT = 17,
    PlaceNFT = 18
}
public enum AnalyticsEventReturnType
{
    OperationSuccesful = 0,
    OperationFailed = 1,
}

public struct AnalyticsEvent
{
    public AnalyticsEventID _id { get; private set; }
    public AnalyticsEventReturnType _return { get; private set; }
    private readonly object _data; ///float string and int , will contain all the data we need to pass as parameteres to the analytics

    public int IntData
    {
        get
        {
            Assert.IsTrue(_data is int, "AnalyticsEvent._data is not an int");
            return (int)_data;
        }
    }

    public float FloatData
    {
        get
        {
            Assert.IsTrue(_data is float, "AnalyticsEvent._data is not a float");
            return (float)_data;
        }
    }

    public string StringData
    {
        get
        {
            Assert.IsTrue(_data is string, "AnalyticsEvent._data is not a string");
            return (string)_data;
        }
    }

    public AnalyticsEvent(AnalyticsEventID id) : this()
    {
        _id = id;
    }

    public AnalyticsEvent(AnalyticsEventID id, int data) : this()
    {
        _id = id;
        _data = data;
    }

    public AnalyticsEvent(AnalyticsEventID id, float data) : this()
    {
        _id = id;
        _data = data;
    }

    public AnalyticsEvent(AnalyticsEventID id, string data) : this()
    {
        _id = id;
        _data = data;
    }

    public AnalyticsEvent(AnalyticsEventID id, AnalyticsEventReturnType returnType, object data) : this()
    {
        _data = data;
        _id = id;
        _return = returnType;
    }
}


