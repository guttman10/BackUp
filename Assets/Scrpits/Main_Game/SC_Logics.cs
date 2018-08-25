using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Logics : MonoBehaviour {
    public Sprite[] DiceNum;
    public Button Dice;
    public int DiceValue, Turn;
    int TotalPower1, TotalPower2;
    private IEnumerator coroutine;
    static SC_Logics inst;
    public Button go1, go2;
    ColorBlock TileColorer,Original_TileColor;
    bool Player_Roll;
    public Dictionary<string, GameObject> Tilles;
    public Dictionary<string, GameObject> UnityObjects;
    public Dictionary<string, GameObject> Players;
    public static SC_Logics Instance
    {
        get
        {
            if (inst == null)
                inst = GameObject.Find("SC_Logics").GetComponent<SC_Logics>();

            return inst;
        }
    }
    void Awake()
    {
        Global_Variables.PlayerCount = 2;
        Player_Info.Stats(2).IsABot = true;
        InitDiconatries();
        InitColorers();
        RestartGame();
        SetUpBoard();
    }
    void InitDiconatries()
    {
        Tilles = new Dictionary<string, GameObject>();
        UnityObjects = new Dictionary<string, GameObject>();
        Players = new Dictionary<string, GameObject>();
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Blue_Tile");
        foreach (GameObject g in temp)
        {
            Tilles.Add(g.name, g);
            g.GetComponent<Button>().interactable = false;
        }
        temp = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in temp)
            UnityObjects.Add(g.name, g);
        temp = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject g in temp)
            Players.Add(g.name, g);
    }

    void InitColorers()
    {
        TileColorer.normalColor = Color.yellow;
        TileColorer.highlightedColor = Color.green;
        TileColorer.colorMultiplier = 1;
        Original_TileColor = Tilles["Blue_Tile_0"].GetComponent<Button>().colors;
    }

    void RestartGame()
    {
        Turn = 0;
        for(int i =0; i < Global_Variables.PlayerCount; i++)
        {
            Player_Info.Stats(i+1).RestartStats();
        }
        Player_Info.Stats(GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.Moving;
        Player_Roll = false;
        TotalPower1 = 0;
        TotalPower2 = 0;
    }
    void SetUpBoard()
    {
        UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = string.Empty;
        UnityObjects["Drawen_Card"].SetActive(false);
        UnityObjects["Interact"].SetActive(false);
        UnityObjects["Fight_Player"].SetActive(false);
        UnityObjects["Battle"].SetActive(false);
    }
    public int GetCurrentPlayer()
    {
        return Turn % Global_Variables.PlayerCount + 1;
    }
    void BotPlay()
    {
        SC_Bot.Instance.AutoPlay();
    }

    public void PassTurn()
    {
        SetUpBoard();
        Player_Info.Stats(GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.Idle;
        Turn++;
        Player_Info.Stats(GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.Moving;
        if (Player_Info.Stats(GetCurrentPlayer()).IsABot)
            Invoke("BotPlay",0.5f);
        else
            EnableDice(true);

    }
    public void Roll()
    {
        coroutine = RollTheDice();
        StartCoroutine(coroutine);
        DiceValue = Random.Range(1, 7);
        EnableDice(false);
    }

    public IEnumerator RollTheDice()
    {
        for (int i = 0; i < 20; i++)
        {
            Dice.GetComponent<Image>().sprite = DiceNum[Random.Range(1, 7) - 1];
            yield return new WaitForSeconds(0.02f);
        }
        Dice.GetComponent<Image>().sprite = DiceNum[DiceValue - 1];
        if (Player_Info.Stats(GetCurrentPlayer()).IsABot == false && Player_Info.Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.Moving)
            SetTitlesForSelect();
        else if (Player_Info.Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.Battle)
        {
            Player_Roll = !Player_Roll;
            Battle(Player_Roll);
        }
    }

    public void Move(int Tile_Num)
    {
        Vector2 Target = Tilles["Blue_Tile_" + Tile_Num].GetComponent<RectTransform>().position;
        Vector2 Player = Players["Player_"+ GetCurrentPlayer()].GetComponent<RectTransform>().position;
        Target.y += Random.Range(-20, 20);
        Target.x += Random.Range(-20, 20);
        Players["Player_" + GetCurrentPlayer()].GetComponent<RectTransform>().position = Vector2.MoveTowards(Player, Target, 1000);
        Player_Info.Stats(GetCurrentPlayer()).pos = Tile_Num;
        UpdateTiles(false, Original_TileColor);
        UnityObjects["Interact"].SetActive(true);
        for(int i =1; i<Global_Variables.PlayerCount;i++)
        {
            if(Player_Info.Stats(GetCurrentPlayer()).pos == Player_Info.Stats(GetCurrentPlayer() + i).pos)
            {
                UnityObjects["Fight_Player"].SetActive(true);
            }
        }
    }
    void SetTitlesForSelect()
    {
        go1 = Tilles["Blue_Tile_" + (( Player_Info.Stats(GetCurrentPlayer()).pos+DiceValue) % 22)].GetComponent<Button>();
        go2 = Tilles["Blue_Tile_" + (22 + Player_Info.Stats(GetCurrentPlayer()).pos - DiceValue) % 22].GetComponent<Button>();
        UpdateTiles(true, TileColorer);
       
    }
    void UpdateTiles(bool IsActive,ColorBlock TileColor)
    {
        go1.interactable = IsActive;
        go2.interactable = IsActive;
        go1.colors = TileColor;
        go2.colors = TileColor;

    }
    void Battle(bool Player_Has_Rolled)
    {
        UnityObjects["Battle"].SetActive(true);
        if (Player_Has_Rolled)
        {
            TotalPower1 = Player_Info.Stats(GetCurrentPlayer()).pwr + DiceValue;
            UnityObjects["Battle_Roll"].GetComponent<Text>().text = ""+Player_Info.Stats(GetCurrentPlayer()).pwr+"+"+DiceValue;
            Invoke("Roll",1);
        }
        else
        {
            TotalPower2 = Deck_Manager.Instance.GetPower() + DiceValue;
            UnityObjects["Battle_Roll"].GetComponent<Text>().text += " VS " + Deck_Manager.Instance.GetPower() + "+" + DiceValue;
            BattleResult();
        }
    }

    private void BattleResult()
    {
        if(TotalPower1 > TotalPower2)
        {
            Player_Info.Stats(GetCurrentPlayer()).AddXp(Deck_Manager.Instance.GetPower());
            UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = "You Win!";
        }
        else if (TotalPower1 < TotalPower2)
        {
            Player_Info.Stats(GetCurrentPlayer()).TakeAhit();
            UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = "You Lose!";
        }
        else
        {
            UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = "Draw!";
        }
        Invoke("PassTurn", 1.5f );
    }
    public void MakeCardShow()
    {
        Invoke("PassTurn", 1.5f);
    }
    public void EnableDice(bool Enabler)
    {
        Dice.GetComponent<Button>().interactable = Enabler;
    }
}