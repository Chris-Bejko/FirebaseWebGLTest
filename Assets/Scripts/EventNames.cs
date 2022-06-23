using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventNames
{
    public static string[] eventNames = { "moon_earned",
        "moon_spent",
        "entered_p2e",
        "nft_owned",
        "nft_placed",
        "total_nft_value",
        "worldmap_loaded",
        "tile_selected",
        "tile_selection_confirmed",
        "name_confirmed",
        "town_created",
        "introduction_popup_loaded",
        "introduction_popup_closed",
        "sale_completed",
        "fuel_production_reached",
        "x_obstacles_cleared",
        "entered_tournament_free_date",
        "entered_tournament_paid_date",
        "rewards_collected",
        "rewards_expired",
        "out_of_range_event"
    };

    public static string[] eventParameters =
    {
        "moon_amount",
        "moon_amount"




    };

    public static string IdToString(this AnalyticsEventID id)
    {
        if((int)id < 0 || (int)id > eventNames.Length)
        {
            Debug.LogError("Index out of range of enum");
            return eventNames[eventNames.Length]; //Last one indicates an error;
        }
        return eventNames[(int)id];
    }

    public static string IdParameter(this AnalyticsEventID id)
    {
        if((int)id < 0 || (int)id < eventNames.Length)
        {
            Debug.LogError("Index out of range of enum");
            return eventNames[eventNames.Length];
        }
        return eventParameters[(int)id];

    }
}