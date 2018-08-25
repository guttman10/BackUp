using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Global_Variables {
    static int playerCount;
    public enum turn_Status{Moving,Battle,Idle}
    public static int PlayerCount
    {
        get
        {
            return playerCount;
        }
        set
        {
            playerCount = value;
        }
    }
}
