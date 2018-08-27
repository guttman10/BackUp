using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Logics : MonoBehaviour {
    public Sprite[] DiceNum;
    public Sprite PlayerBattleBackGround;
    public Button Dice;
    public Button BasicTile;
    public int DiceValue, Turn;
    int TotalPower1, TotalPower2;
    int PlayerCount = 4;
    int Oppnent;
    private IEnumerator coroutine;
    static SC_Logics inst;
    Button go1, go2;
    ColorBlock TileColorer,Original_TileColor;
    bool Player_Rolled, HasReRolled, HasOpponentReRolled, OpponentRoll,NoWinner;
    public bool IsFightingDragon;
    public Dictionary<string, GameObject> Tilles;
    public Dictionary<string, GameObject> UnityObjects;
    public Dictionary<string, GameObject> Players;
    public static SC_Logics Instance
    {
        get
        {
            if (inst == null)
                inst = GameObject.Find("Main_Game").GetComponent<SC_Logics>();

            return inst;
        }
    }
    void Awake()
    {
        InitDiconatries();
        InitColorers();
    }
    void OnEnable()
    {
        SetupGame();
        SetUpBoard();
    }
    /*public void RemoveWhoDontPlay(int PlayerSetter)
    {
        PlayerCount = PlayerSetter;
        for (int i = PlayerCount; i < 4 ;  i++)
        {
            Players["Player_"+(i+1)].SetActive(false);
        }
    }**/
    void InitDiconatries()
    {
        Tilles = new Dictionary<string, GameObject>();
        UnityObjects = new Dictionary<string, GameObject>();
        Players = new Dictionary<string, GameObject>();
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Tile");
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
        Original_TileColor = BasicTile.GetComponent<Button>().colors;
    }
    public void RestartGame()
    {
        SetupGame();
        SetUpBoard();
    }

    void SetupGame()
    {
        Turn = 0;
        for(int i =1; i <= PlayerCount; i++)
        {
           Player_Info.Stats(i).RestartStats();
            RestartPlayerPos(i);
        }
        Player_Info.Stats(GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.Moving;
        TotalPower1 = 0;
        TotalPower2 = 0;
        UnityObjects["Winning_Msg"].SetActive(false);
        NoWinner = true;
        EnableDice(true);
    }

    void SetUpBoard()
    {
        Player_Rolled = false;
        HasReRolled = false;
        HasOpponentReRolled = false;
        OpponentRoll = false;
        IsFightingDragon = false;
        UnityObjects["Drawen_Card"].SetActive(false);
        UnityObjects["ReRoll2"].SetActive(false);
        UnityObjects["Fight_Player"].SetActive(false);
        UnityObjects["Continue2"].SetActive(false);
        for (int i =1; i <= 4; i++)
        {
            UnityObjects["Fight_Player_"+i].SetActive(false);
        }
        UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = string.Empty;
        UnityObjects["Interact"].SetActive(false);
        UnityObjects["Battle"].SetActive(false);
        UnityObjects["Continue"].SetActive(false);
        UnityObjects["ReRoll"].SetActive(false);
        UnityObjects["Player_Stats"].SetActive(false);
    }
    public void RestartPlayerPos(int p)
    {
        Vector2 Target = Tilles["Tile_" + 0].GetComponent<RectTransform>().position;
        Vector2 Player = Players["Player_" + p].GetComponent<RectTransform>().position;
        Target.y += Random.Range(-20, 20);
        Target.x += Random.Range(-20, 20);
        Players["Player_" +p].GetComponent<RectTransform>().position = Vector2.MoveTowards(Player, Target, 1000);
        Player_Info.Stats(p).pos = 0;
    }
    public int GetCurrentPlayer()
    {
        return Turn % PlayerCount + 1;
    }
    void BotPlay()
    {
        SC_Bot.Instance.AutoPlay();
    }

    public void PassTurn()
    {
        if (Player_Info.Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.Battle)
        {
            ApplyBattleResult();
        }
        if (Player_Info.Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.BattlePlayer)
        {
            ApplyPlayerBattleResult();
        }
        if(NoWinner){
            SetUpBoard();
            Player_Info.Stats(GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.Idle;
            Turn++;
            Player_Info.Stats(GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.Moving;
            if (Player_Info.Stats(GetCurrentPlayer()).IsABot)
                Invoke("BotPlay", 0.5f);
            else
                EnableDice(true);
        }

    }
    public void Roll()
    {
        coroutine = RollTheDice();
        StartCoroutine(coroutine);
        EnableDice(false);
    }

    public IEnumerator RollTheDice()
    {
        DiceValue = Random.Range(1, 7);
        for (int i = 0; i < 20; i++)
        {
            Dice.GetComponent<Image>().sprite = DiceNum[Random.Range(1, 7) - 1];
            yield return new WaitForSeconds(0.02f);
        }
        Dice.GetComponent<Image>().sprite = DiceNum[DiceValue - 1];
        if (HasOpponentReRolled)
        {
            OpponentRoll = true;
        }
        if (OpponentRoll)
        {
            if(Player_Info.Stats(Oppnent).IsABot == false && HasOpponentReRolled == false)
            {
                UnityObjects["ReRoll2"].SetActive(true);
            }
            OpponentRoll = false;
            BattlePlayerPhaseThree();
        }
        else
        {
            if (Player_Info.Stats(GetCurrentPlayer()).IsABot == false && Player_Info.Stats(GetCurrentPlayer()).faith > 0 && HasReRolled == false)
                UnityObjects["ReRoll"].SetActive(true);
            if (Player_Info.Stats(GetCurrentPlayer()).IsABot == false && Player_Info.Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.Moving)
                SetTitlesForSelect(Player_Info.Stats(GetCurrentPlayer()).pos);
            else if (Player_Info.Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.Battle)
            {
                Player_Rolled = !Player_Rolled;
                Battle(Player_Rolled);
            }
            else if (Player_Info.Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.BattlePlayer)
            {
                if (HasReRolled)
                {
                    Player_Rolled = !Player_Rolled;
                    HasReRolled = false;
                }
                if (Player_Rolled == false)
                {
                    Player_Rolled = !Player_Rolled;
                    BattlePlayerPhaseOne();
                }
            }
        }
    }
    public void ReRoll()
    {
        Player_Info.Stats(GetCurrentPlayer()).faith--;
        UnityObjects["ReRoll"].SetActive(false);
        HasReRolled = true;
        coroutine = RollTheDice();
        StartCoroutine(coroutine);
        if (Player_Info.Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.Moving)
        {
            UpdateTiles(false, Original_TileColor);
        }
    }
    public void ReRollOpponent()
    {
        Player_Info.Stats(Oppnent).faith--;
        UnityObjects["ReRoll2"].SetActive(false);
        HasOpponentReRolled = true;
        coroutine = RollTheDice();
        StartCoroutine(coroutine);
    }

    public void Move(int Tile_Num)
    {
        UnityObjects["ReRoll"].SetActive(false);
        Vector2 Target = Tilles["Tile_" + Tile_Num].GetComponent<RectTransform>().position;
        Vector2 Player = Players["Player_"+ GetCurrentPlayer()].GetComponent<RectTransform>().position;
        Target.y += Random.Range(-20, 20);
        Target.x += Random.Range(-20, 20);
        Players["Player_" + GetCurrentPlayer()].GetComponent<RectTransform>().position = Vector2.MoveTowards(Player, Target, 1000);
        Player_Info.Stats(GetCurrentPlayer()).pos = Tile_Num;
        UpdateTiles(false, Original_TileColor);
        UnityObjects["Interact"].SetActive(true);
        for(int i =1; i<=PlayerCount;i++)
        {
            if(Player_Info.Stats(i).pos == Player_Info.Stats(GetCurrentPlayer()).pos && i != GetCurrentPlayer())
            {
                UnityObjects["Fight_Player"].SetActive(true);
            }
        }
    }
    void SetTitlesForSelect(int pos)
    {
        if (pos < 100)
        {
            go1 = Tilles["Tile_" + ((pos+DiceValue) % 22)].GetComponent<Button>();
            go2 = Tilles["Tile_" + (22 + pos - DiceValue) % 22].GetComponent<Button>();
        }
        else
        {
            go1 = Tilles["Tile_" + (((pos-100 + DiceValue) % 14)+100)].GetComponent<Button>();
            go2 = Tilles["Tile_" + (((14 + pos-100 - DiceValue) % 14)+100)].GetComponent<Button>();
        }
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
            UnityObjects["Battle_Roll"].GetComponent<Text>().text = ""+TotalPower1;
            if (HasReRolled == false)
            {
                Invoke("Roll",1);
            }
        }
        else
        {
            TotalPower2 = Deck_Manager.Instance.GetPower() + DiceValue;
            UnityObjects["Battle_Roll"].GetComponent<Text>().text = TotalPower1 + " VS " + TotalPower2;
            BattleResult();
        }
        if (HasReRolled)
        {
            UnityObjects["Battle_Roll"].GetComponent<Text>().text = TotalPower1  + " VS " + TotalPower2;
            BattleResult();
        }
            
    }

    private void BattleResult()
    {
        if(TotalPower1 > TotalPower2)
        {
            UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = "You Win!";
        }
        else if (TotalPower1 < TotalPower2)
        {
            UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = "You Lose!";
        }
        else
        {
            UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = "Draw!";
        }
        MakeCardShow();
    }
    
    void ApplyBattleResult()
    {
        
        if (TotalPower1 > TotalPower2)
        {
            if (IsFightingDragon)
            {
                NoWinner = false;
                UnityObjects["Winning_Msg"].SetActive(true);
                UnityObjects["Winning_Text"].GetComponent<Text>().text = "Player " + GetCurrentPlayer() + "\n Victory!";
            }
            else
            {
                Player_Info.Stats(GetCurrentPlayer()).AddXp(Deck_Manager.Instance.GetPower());
            }
        }
        else if (TotalPower1 < TotalPower2)
        {
            if (Player_Info.Stats(GetCurrentPlayer()).TakeAhit())
            {
                RestartPlayerPos(GetCurrentPlayer());
            }

        }
    }
    public void MakeCardShow()
    {
        if (Player_Info.Stats(GetCurrentPlayer()).IsABot == false)
        {
            UnityObjects["Continue"].SetActive(true);
        }
        else
        {
            Invoke("PassTurn", 1.5f);
        }
    }
    void ApplyPlayerBattleResult()
    {
        if (TotalPower1 > TotalPower2)
        {
            if (Player_Info.Stats(Oppnent).TakeAhit())
            {
                RestartPlayerPos(Oppnent);
            }
        }
        else if (TotalPower1 < TotalPower2)
        {
            if (Player_Info.Stats(GetCurrentPlayer()).TakeAhit())
            {
                RestartPlayerPos(GetCurrentPlayer());
            }
        }
    }
    public void SetUpFightPlayer()
    {
        for(int i = 1; i <=PlayerCount; i++)
        {
           if(i != GetCurrentPlayer())
            {
                if (Player_Info.Stats(i).pos == Player_Info.Stats(GetCurrentPlayer()).pos && i <= PlayerCount)
                {
                    UnityObjects["Fight_Player_" + i].SetActive(true);
                }
            }
        }
        UnityObjects["Interact"].SetActive(false);
        UnityObjects["Fight_Player"].SetActive(false);
    }
    public void FightPlayer(int x)
    {
        Oppnent = x;
        Player_Info.Stats(Oppnent).Turn_Status = Global_Variables.turn_Status.BattlePlayer;
        Player_Info.Stats(GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.BattlePlayer;
        for (int i = 1; i <= 4; i++)
        {
            UnityObjects["Fight_Player_" + i].SetActive(false);
        }
        UnityObjects["Drawen_Card"].SetActive(true);
        UnityObjects["Drawen_Card"].GetComponent<Image>().sprite = PlayerBattleBackGround;
        UnityObjects["Card_Text"].GetComponent<Text>().text = "Fighting Player " +Oppnent;
        EnableDice(true);
    }
    void BattlePlayerPhaseOne()
    {
        UnityObjects["Battle"].SetActive(true);
        TotalPower1 = Player_Info.Stats(GetCurrentPlayer()).pwr + DiceValue;
        UnityObjects["Battle_Roll"].GetComponent<Text>().text = "" + TotalPower1;
        UnityObjects["Continue2"].SetActive(true);
    }
    public void BattlePlayerPhaseTwo()
    {
        UnityObjects["ReRoll"].SetActive(false);
        UnityObjects["Continue2"].SetActive(false);
        OpponentRoll = true;
        if (Player_Info.Stats(Oppnent).IsABot)
        {
            Invoke("Roll", 1);
        }
        else
        {
            EnableDice(true);
        }
    }
    void BattlePlayerPhaseThree()
    {
        TotalPower2 = Player_Info.Stats(Oppnent).pwr + DiceValue;
        UnityObjects["Battle_Roll"].GetComponent<Text>().text = TotalPower1 + " VS " + TotalPower2;
        if (TotalPower1 > TotalPower2)
        {
            UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = "Player " +GetCurrentPlayer() + " Win!";
        }
        else if (TotalPower1 < TotalPower2)
        {
            UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = "Player " + Oppnent+ " Win!";
        }
        else
        {
            UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = "Draw!";
        }
        MakeCardShow();
    }
    public void ShowStats()
    {
        UnityObjects["Player_Stats"].SetActive(true);
        for (int i = 1; i <= 4; i++)
        {
            UnityObjects["Text_P"+i].GetComponent<Text>().text = "Power: "+Player_Info.Stats(i).pwr + "\nFaith: "+Player_Info.Stats(i).faith+"\nGold: "+ Player_Info.Stats(i).gold + "\nHP: "+ Player_Info.Stats(i).hp;
        }
    }
    public void EnableDice(bool Enabler)
    {
        Dice.GetComponent<Button>().interactable = Enabler;
    }
}