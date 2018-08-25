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
     
}
