using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Bot : MonoBehaviour {
    private static SC_Bot inst = null;
    public static SC_Bot Instance
    {
        get
        {
            if (inst == null)
                inst = GameObject.Find("SC_Bot").GetComponent<SC_Bot>();

            return inst;
        }
    }
    public void AutoPlay()
    {
        SC_Logics.Instance.Roll();
        Invoke("BotMove", 1);
    }
    void BotMove()
    {
        Vector2 Player = SC_Logics.Instance.Players["Player_" + (SC_Logics.Instance.GetCurrentPlayer())].GetComponent<RectTransform>().position;
        int Randomizer = Random.Range(1, 11);
        int Tile;
        if (Randomizer % 2 == 0)
        {
            Tile = ( Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).pos + SC_Logics.Instance.DiceValue) % 22;
        }
        else
        {
            Tile = (22 + (( Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).pos - SC_Logics.Instance.DiceValue))) % 22;
        }
        Vector2 Target = SC_Logics.Instance.Tilles["Blue_Tile_" + Tile].GetComponent<RectTransform>().position;
        Target.y += Random.Range(-20, 20);
        Target.x += Random.Range(-20, 20);
        SC_Logics.Instance.Players["Player_" + SC_Logics.Instance.GetCurrentPlayer()].GetComponent<RectTransform>().position = Vector2.MoveTowards(Player, Target, 1000);
        Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).pos = Tile;
        BotDraw();
    }
    void BotDraw()
    {
        Deck_Manager.Instance.Draw(Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).pos);
        if (Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.Battle)
            SC_Logics.Instance.Roll();

    }

}
