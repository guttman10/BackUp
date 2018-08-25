using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Info : MonoBehaviour
{

    public int gold, hp, faith, pos,xp,pwr;
    public bool IsABot;
    public Global_Variables.turn_Status Turn_Status;
    static Player_Info inst;
    public static Player_Info Stats(int id)
    {
        inst = GameObject.Find("Player_Info_" + id).GetComponent<Player_Info>();
        return inst;
    }
    public void RestartStats()
    {
        gold = 1;
        hp = 4;
        faith = 2;
        pwr = 2;
        pos = 0;
        xp = 0;
        Turn_Status = Global_Variables.turn_Status.Idle;
    }
    public void AddXp(int ToAdd)
    {
        xp += ToAdd;
        while(xp >= 5)
        {
            pwr++;
            xp -= 5;
        }
    }
    public void TakeAhit()
    {
        hp--;
        if (hp == 0)
        {
            Debug.Log("Player" + SC_Logics.Instance.GetCurrentPlayer() + " is Dead...");
        }
    }

}
