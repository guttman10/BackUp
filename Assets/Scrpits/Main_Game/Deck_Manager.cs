using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Deck_Manager : MonoBehaviour {
    Stack<Card> BlueDeck,RedDeck;
    List<Card> L1, L2;
    Card Curr;
    public Sprite[] Album;
    private static Deck_Manager inst = null;
    public static Deck_Manager Instance
    {
        get
        {
            if (inst == null)
                inst = GameObject.Find("Deck_Manager").GetComponent<Deck_Manager>();

            return inst;
        }
    }
    void Awake()
    {
        InitDecks();
        RestartDecks();
    }

    private void InitDecks()
    {
        L1 = new List<Card>();
        L2 = new List<Card>();
        BlueDeck = new Stack<Card>();
        RedDeck = new Stack<Card>();
        for (int i = 0; i <20; i++)
        {
            L1.Add(new MonsterCard(2, 0));
            L1.Add(new MonsterCard(3, 1));
            L1.Add(new MonsterCard(4, 2));
            L2.Add(new MonsterCard(9, 11));

        }
        for(int i =0; i<10; i++)
        {
            L1.Add(new GoldCard(1, 3));
            L1.Add(new GoldCard(2, 3));
            L1.Add(new BlessCard(5));
            L1.Add(new CurseCard(4));
            L2.Add(new BlessCard(5));
            L2.Add(new CurseCard(4));
        }

    }
    public void RestartDecks()
    {
        RestartDeckBlue();
        RestartDeckRed();
    }

    private void RestartDeckRed()
    {
        var rng = new System.Random();
        int n = L2.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = L2[k];
            L2[k] = L2[n];
            L2[n] = value;
        }
        for (int i = 0; i < L2.Count; i++)
        {
            RedDeck.Push(L2[i]);
        }
    }

    private void RestartDeckBlue()
    {
        var rng = new System.Random();
        int n = L1.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = L1[k];
            L1[k] = L1[n];
            L1[n] = value;
        }
        for (int i = 0; i < L1.Count; i++)
        {
            BlueDeck.Push(L1[i]);
        }
    }

    public void Draw(int Tile)
    {
        SC_Logics.Instance.UnityObjects["Interact"].SetActive(false);
        SC_Logics.Instance.UnityObjects["Fight_Player"].SetActive(false);
        CheckDeckForEmpty();
        if(Tile >= 100)
        {
            Curr = RedDeck.Peek();
            RedDeck.Pop();
        }
        else
        {
            Curr = BlueDeck.Peek();
            BlueDeck.Pop();
        }
        Curr.PlayCard();
    }

    private void CheckDeckForEmpty()
    {
        if (BlueDeck.Count == 0)
            RestartDeckBlue();
        if (RedDeck.Count == 0)
            RestartDeckRed();

    }

    public int GetPower()
    {
        return Curr.GetPower();
    }



}
public class Card
{
    protected int Picture;//this can save somespaace (using 100-200  ints insted of 100-200 sprites)

    public virtual void PlayCard()
    {
    }

    public virtual int GetPower()
    {
        throw new NotImplementedException();
    }
}
public class MonsterCard :Card
{
    int Power;
    public MonsterCard(int P, int S)
    {
        Picture = S;
        Power = P;
    }
    public override void PlayCard()
    {
        SC_Logics.Instance.UnityObjects["Drawen_Card"].SetActive(true); 
        SC_Logics.Instance.UnityObjects["Drawen_Card"].GetComponent<Image>().sprite = Deck_Manager.Instance.Album[Picture];
        SC_Logics.Instance.UnityObjects["Card_Text"].GetComponent<Text>().text = "Monster\nPower level " + Power;
        Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.Battle;
        if(Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).IsABot==false)
            SC_Logics.Instance.EnableDice(true);
    }

    public override int GetPower()
    {
        return Power;
    }
}
public class GoldCard : Card
{
    int Value;
    public GoldCard(int v, int s)
    {
        Picture = s;
        Value = v;
    }
    public override void PlayCard()
    {
        SC_Logics.Instance.UnityObjects["Drawen_Card"].SetActive(true);
        SC_Logics.Instance.UnityObjects["Drawen_Card"].GetComponent<Image>().sprite = Deck_Manager.Instance.Album[Picture];
        SC_Logics.Instance.UnityObjects["Card_Text"].GetComponent<Text>().text = "Gold\n"+ Value + " Coins";
        Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).gold += Value;
        SC_Logics.Instance.MakeCardShow();
    }

}
public class BlessCard : Card
{
    public BlessCard(int s)
    {
        Picture = s;
    }
    public override void PlayCard()
    {
        int Rand = UnityEngine.Random.Range(1, 5);
        SC_Logics.Instance.UnityObjects["Drawen_Card"].SetActive(true);
        SC_Logics.Instance.UnityObjects["Drawen_Card"].GetComponent<Image>().sprite = Deck_Manager.Instance.Album[Picture];
        switch (Rand)
        {
            case 1:
                Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).gold += 2;
                SC_Logics.Instance.UnityObjects["Card_Text"].GetComponent<Text>().text = "Blessing\nYou have been granted 2 Coins";
                break;
            case 2:
                Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).hp += 2;
                SC_Logics.Instance.UnityObjects["Card_Text"].GetComponent<Text>().text = "Blessing\nYou have been granted 2 Hp";
                break;
            case 3:
                Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).faith += 2;
                SC_Logics.Instance.UnityObjects["Card_Text"].GetComponent<Text>().text = "Blessing\nYou have been granted 2 Faith";
                break;
            case 4:
                Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).AddXp(3);
                SC_Logics.Instance.UnityObjects["Card_Text"].GetComponent<Text>().text = "Blessing\nYou have been granted 3 Xp";
                break;
        }
        SC_Logics.Instance.MakeCardShow();
    }

}

public class CurseCard : Card
{
    public CurseCard(int s)
    {
        Picture = s;
    }
    public override void PlayCard()
    {
        int Rand = UnityEngine.Random.Range(1, 5);
        SC_Logics.Instance.UnityObjects["Drawen_Card"].SetActive(true);
        SC_Logics.Instance.UnityObjects["Drawen_Card"].GetComponent<Image>().sprite = Deck_Manager.Instance.Album[Picture];
        switch (Rand)
        {
            case 1:
                Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).gold = 0;
                SC_Logics.Instance.UnityObjects["Card_Text"].GetComponent<Text>().text = "Curse\nYou Lost your Coins";
                break;
            case 2:
                Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).TakeAhit();
                SC_Logics.Instance.UnityObjects["Card_Text"].GetComponent<Text>().text = "Curse\nYou Were Damaged for 1 Hp";
                break;
            case 3:
                Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).faith = 0;
                SC_Logics.Instance.UnityObjects["Card_Text"].GetComponent<Text>().text = "Curse\nYour Gods Have abandoned you\nYour Faith is now 0";
                break;
            case 4:
                Player_Info.Stats(SC_Logics.Instance.GetCurrentPlayer()).xp=0;
                SC_Logics.Instance.UnityObjects["Card_Text"].GetComponent<Text>().text = "Curse\nYou have Amnesia\nYour Xp is now 0";
                break;
        }
        SC_Logics.Instance.MakeCardShow();
    }

}