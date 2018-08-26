using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_GameManager : MonoBehaviour {
    public void Roll()
    {
        SC_Logics.Instance.Roll();
    }
    public void Move(int x)
    {
        SC_Logics.Instance.Move(x);
    }
    public void Draw()
    {
        Deck_Manager.Instance.Draw(Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).pos);
    }
    public void PassTurn()
    {
        SC_Logics.Instance.PassTurn();
    }
    public void ReRoll()
    {
        SC_Logics.Instance.ReRoll();
    }
    public void SetSetUpFightPlayer()
    {
        SC_Logics.Instance.SetUpFightPlayer();
    }
    public void FightPlayer(int x)
    {
        SC_Logics.Instance.FightPlayer(x);
    }
    public void BattlePlayerPhaseTwo()
    {
        SC_Logics.Instance.BattlePlayerPhaseTwo();
    }
    public void ReRollOpponent()
    {
        SC_Logics.Instance.ReRollOpponent();
    }
}
