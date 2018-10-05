using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Logics : MonoBehaviour
{
    public Sprite[] DiceNum;
    public Sprite PlayerBattleBackGround;
    public Sprite[] PlayerSpriteTurn;
    public Button Dice;
    public Button BasicTile;
    Vector2 Target;
    Vector2 Player;
    public int DiceValue, Turn;
    int TotalPower1, TotalPower2;
    public int PlayerCount, Myturn;
    int Opponent;
    private IEnumerator coroutine;
    static SC_Logics inst;
    Button go1, go2;
    ColorBlock TileColorer, Original_TileColor;
    bool Player_Rolled, HasReRolled, HasOpponentReRolled, OpponentRoll, NoWinner;
    public bool IsFightingDragon;
    public bool isMyTurn,PlayingMulti;
    public Dictionary<string, GameObject> Tilles;
    public Dictionary<string, GameObject> UnityObjects;
    public Dictionary<string, GameObject> Players;
    public GameObject Ui;
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
        Listener.OnMoveCompleted += OnMoveCompleted;
        Listener.OnChatReceived += OnChatReceived;
        Listener.OnGameStopped += OnGameStopped;

    }

    void OnDisable()
    {
        Listener.OnMoveCompleted -= OnMoveCompleted;
        Listener.OnChatReceived -= OnChatReceived;
    }
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
    public Player_Info Stats(int id)
    {
        return Players["Player_" + id].GetComponent<Player_Info>();
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
    public void RemoveWhoDontPlay()
    {
        for (int i = 1; i <= 4; i++)
        {
            Players["Player_" + (i)].SetActive(true);
        }
        for (int i = PlayerCount; i < 4; i++)
        {
            Players["Player_" + (i + 1)].SetActive(false);
        }
    }
    void SetupGame()
    {
        Turn = 0;
        for (int i = 1; i <= PlayerCount; i++)
        {
            Stats(i).RestartStats();
            RestartPlayerPos(i);
        }
        Stats(GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.Moving;
        TotalPower1 = 0;
        TotalPower2 = 0;
        UnityObjects["Winning_Msg"].SetActive(false);
        UnityObjects["You_Player"].GetComponent<Image>().sprite = PlayerSpriteTurn[Myturn - 1];
        NoWinner = true;
        EnableDice(true);
        RemoveWhoDontPlay();
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
        for (int i = 1; i <= 4; i++)
        {
            UnityObjects["Fight_Player_" + i].SetActive(false);
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
        Players["Player_" + p].GetComponent<RectTransform>().position = Vector2.MoveTowards(Player, Target, 1000);
        Stats(p).pos = 0;
    }
    public int GetCurrentPlayer()
    {
        return Turn % PlayerCount + 1;
    }
    void ChangeCurrentTurnSprite(int p)
    {
        UnityObjects["Current_Player"].GetComponent<Image>().sprite = PlayerSpriteTurn[p - 1];
    }
    void BotPlay()
    {
        SC_Bot.Instance.AutoPlay();
    }

    public void PassTurn()
    {
        if (isMyTurn)
        {
            if (PlayingMulti)
            {
                Dictionary<string, object> _toSend = new Dictionary<string, object>();
                _toSend.Add("Move", "End");
                _toSend.Add("HP", Stats(GetCurrentPlayer()).hp.ToString());
                _toSend.Add("faith", Stats(GetCurrentPlayer()).faith.ToString());
                _toSend.Add("xp", Stats(GetCurrentPlayer()).xp.ToString());
                _toSend.Add("gold", Stats(GetCurrentPlayer()).gold.ToString());
                _toSend.Add("pwr", Stats(GetCurrentPlayer()).pwr.ToString());
                string _jsonToSend = MiniJSON.Json.Serialize(_toSend);
                WarpClient.GetInstance().SendChat(_jsonToSend);
            }
            SubmitTurnPass();
        }
    }

    void SubmitTurnPass()
    {
        if (Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.Battle)
        {
            ApplyBattleResult();
        }
        if (Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.BattlePlayer)
        {
            ApplyPlayerBattleResult();
        }
        if (NoWinner)
        {
            SetUpBoard();
            Stats(GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.Idle;
            Turn++;
            ChangeCurrentTurnSprite(GetCurrentPlayer());
            Stats(GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.Moving;
            if (Stats(GetCurrentPlayer()).IsABot)
                Invoke("BotPlay", 0.5f);
            else
                EnableDice(true);
        }
    }
    public void Roll()
    {
        UnityObjects["Continue"].SetActive(false);
        if (isMyTurn)
        {
            coroutine = RollTheDice();
            DiceValue = Random.Range(1, 7);
            if (PlayingMulti)
            {
                Dictionary<string, object> _toSend = new Dictionary<string, object>();
                _toSend.Add("Move", "Roll");
                _toSend.Add("Dval", DiceValue.ToString());
                
                string _jsonToSend = MiniJSON.Json.Serialize(_toSend);
                WarpClient.GetInstance().SendChat(_jsonToSend);
            }
            StartCoroutine(coroutine);
            EnableDice(false);
        }
    }

    public IEnumerator RollTheDice()
    {
        UnityObjects["Continue"].SetActive(false);
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
            if (Stats(Opponent).IsABot == false && HasOpponentReRolled == false)
            {
                UnityObjects["ReRoll2"].SetActive(true);
            }
            OpponentRoll = false;
            BattlePlayerPhaseThree();
        }
        else
        {
            if (Stats(GetCurrentPlayer()).IsABot == false && Stats(GetCurrentPlayer()).faith > 0 && HasReRolled == false)
                UnityObjects["ReRoll"].SetActive(true);
            if (Stats(GetCurrentPlayer()).IsABot == false && Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.Moving)
                SetTitlesForSelect(Stats(GetCurrentPlayer()).pos);
            else if (Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.Battle)
            {
                Player_Rolled = !Player_Rolled;
                Battle(Player_Rolled);
            }
            else if (Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.BattlePlayer)
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
        UnityObjects["Continue"].SetActive(false);
        if (isMyTurn)
        {
            UnityObjects["ReRoll"].SetActive(false);
            DiceValue = Random.Range(1, 7);
            Stats(GetCurrentPlayer()).faith--;
            HasReRolled = true;
            coroutine = RollTheDice();
            StartCoroutine(coroutine);
            if (Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.Moving)
            {
                UpdateTiles(false, Original_TileColor);
            }

            if (PlayingMulti)
            {
                Dictionary<string, object> _toSend = new Dictionary<string, object>();
                _toSend.Add("Move", "ReRoll");
                _toSend.Add("Dval", DiceValue.ToString());
                if (Stats(GetCurrentPlayer()).Turn_Status == Global_Variables.turn_Status.Moving)
                {
                    _toSend.Add("Moving", "Y");
                }
                else
                {
                    _toSend.Add("Moving", "N");
                }
                string _jsonToSend = MiniJSON.Json.Serialize(_toSend);
                WarpClient.GetInstance().SendChat(_jsonToSend);
            }
        }
    }
    public void ReRollOpponent()
    {
        if (isMyTurn)
        {
            Stats(Opponent).faith--;
            UnityObjects["ReRoll2"].SetActive(false);
            HasOpponentReRolled = true;
            coroutine = RollTheDice();
            StartCoroutine(coroutine);
        }
    }

    public void Move(int Tile_Num)
    {
        if (isMyTurn)
        {
            if (PlayingMulti)
            {
                Dictionary<string, object> _toSend = new Dictionary<string, object>();
                _toSend.Add("Move", "Move");
                _toSend.Add("Mval", Tile_Num.ToString());
                string _jsonToSend = MiniJSON.Json.Serialize(_toSend);
                WarpClient.GetInstance().SendChat(_jsonToSend);
            }
            SubmitMove(Tile_Num);
        }
    }
    void SubmitMove(int Tile_Num)
    {
        UnityObjects["ReRoll"].SetActive(false);
        Target = Tilles["Tile_" + Tile_Num].GetComponent<RectTransform>().position;
        Player = Players["Player_" + GetCurrentPlayer()].GetComponent<RectTransform>().position;
        StartCoroutine(AnimateMove(Tile_Num));
        UpdateTiles(false, Original_TileColor);
        HasReRolled = false;

    }

    public IEnumerator AnimateMove(int Tile_Num)
    {
        if (Tile_Num < 100)
        {
            if ((Stats(GetCurrentPlayer()).pos + DiceValue) % 22 == Tile_Num)
            {

                for (int i = 1; i <= DiceValue; i++)
                {
                    Target = Tilles["Tile_" + ((Stats(GetCurrentPlayer()).pos + i) % 22)].GetComponent<RectTransform>().position;
                    Target.y += Random.Range(-20, 20);
                    Target.x += Random.Range(-20, 20);
                    Players["Player_" + GetCurrentPlayer()].GetComponent<RectTransform>().position = Vector2.MoveTowards(Player, Target, 1000);
                    yield return new WaitForSeconds(0.11f);
                }
            }
            else
            {
                for (int i = 1; i <= DiceValue; i++)
                {
                    Target = Tilles["Tile_" + ((22 + Stats(GetCurrentPlayer()).pos - i) % 22)].GetComponent<RectTransform>().position;
                    Target.y += Random.Range(-20, 20);
                    Target.x += Random.Range(-20, 20);
                    Players["Player_" + GetCurrentPlayer()].GetComponent<RectTransform>().position = Vector2.MoveTowards(Player, Target, 1000);
                    yield return new WaitForSeconds(0.11f);
                }
            }
        }
        else
        {
            if ( (((Stats(GetCurrentPlayer()).pos - 100 + DiceValue) % 14) +100) == Tile_Num)
            {

                for (int i = 1; i <= DiceValue; i++)
                {
                    Target = Tilles["Tile_" + (((Stats(GetCurrentPlayer()).pos - 100 + i) % 14) + 100)].GetComponent<RectTransform>().position;
                    Target.y += Random.Range(-20, 20);
                    Target.x += Random.Range(-20, 20);
                    Players["Player_" + GetCurrentPlayer()].GetComponent<RectTransform>().position = Vector2.MoveTowards(Player, Target, 1000);
                    yield return new WaitForSeconds(0.11f);
                }
            }
            else
            {
                for (int i = 1; i <= DiceValue; i++)
                {
                    Target = Tilles["Tile_" + (((14 + Stats(GetCurrentPlayer()).pos - 100 - i) % 14) + 100)].GetComponent<RectTransform>().position;
                    Target.y += Random.Range(-20, 20);
                    Target.x += Random.Range(-20, 20);
                    Players["Player_" + GetCurrentPlayer()].GetComponent<RectTransform>().position = Vector2.MoveTowards(Player, Target, 1000);
                    yield return new WaitForSeconds(0.11f);
                }
            }
        }
        Stats(GetCurrentPlayer()).pos = Tile_Num;
        if (isMyTurn)
        {
            UnityObjects["Interact"].SetActive(true);
            for (int i = 1; i <= PlayerCount; i++)
            {
                if (Stats(i).pos == Stats(GetCurrentPlayer()).pos && i != GetCurrentPlayer())
                {
                    UnityObjects["Fight_Player"].SetActive(true);
                }
            }
        }
    }
    public void Transport(int t)
    {
        Player = Players["Player_" + GetCurrentPlayer()].GetComponent<RectTransform>().position;
        Target = Tilles["Tile_" + t].GetComponent<RectTransform>().position;
        Players["Player_" + GetCurrentPlayer()].GetComponent<RectTransform>().position = Vector2.MoveTowards(Player, Target, 1000);
        Stats(GetCurrentPlayer()).pos = t;
    }
    public void MakeUnanimousDeck()
    {
        Stack<Card> TempBdeck = new Stack<Card>(Deck_Manager.Instance.BlueDeck);//this reverse the original deck
        Stack<Card> TempRdeck = new Stack<Card>(Deck_Manager.Instance.RedDeck);
        string BlueDeckToSend = string.Empty;
        string RedDeckToSend = string.Empty;
        int Size = TempBdeck.Count;
        Dictionary<string, object> _toSend = new Dictionary<string, object>();
        _toSend.Add("Move", "Deck");
        _toSend.Add("Bsize", TempBdeck.Count.ToString());
        for(int i =0; i < Size; i++)
        {
            BlueDeckToSend += TempBdeck.Peek().Picture.ToString();
            TempBdeck.Pop();
        }
        Size = TempRdeck.Count;
        _toSend.Add("Bdeck", BlueDeckToSend);
        _toSend.Add("Rsize", TempRdeck.Count.ToString());
        for (int i = 0; i < Size; i++)
        {
            RedDeckToSend+=TempRdeck.Peek().Picture.ToString();
            TempRdeck.Pop();
        }
        _toSend.Add("Rdeck", RedDeckToSend);
        string _jsonToSend = MiniJSON.Json.Serialize(_toSend);
        WarpClient.GetInstance().SendChat(_jsonToSend);
    }
    /*
     *      L1.Add(new MonsterCard(2, 0));
            L1.Add(new MonsterCard(3, 1));
            L1.Add(new MonsterCard(4, 2));
            L1.Add(new GoldCard(2, 3));
            L1.Add(new BlessCard(5));
            L1.Add(new CurseCard(4));


            L2.Add(new MonsterCard(7, 12));
            L2.Add(new MonsterCard(9, 11));
            L2.Add(new MonsterCard(8, 15));
            L2.Add(new BlessCard(14));
            L2.Add(new CurseCard(13));
     */
    void ChangeToGamerOwnerDeck(string Bdeck,string Rdeck,int Bsize,int Rsize)
    {
        Stack<Card> TempBdeck = new Stack<Card>();
        Stack<Card> TempRdeck = new Stack<Card>();
        int id;
        for (int i =0; i<Bsize; i++)
        {
            id = (int)System.Char.GetNumericValue(Bdeck[i]);
            switch (id)
            {
                case 0:
                    TempBdeck.Push(new MonsterCard(2, 0));
                    break;
                case 1:
                    TempBdeck.Push(new MonsterCard(3, 1));
                    break;
                case 2:
                    TempBdeck.Push(new MonsterCard(4, 2));
                    break;
                case 3:
                    TempBdeck.Push(new GoldCard(2, 3));
                    break;
                case 4:
                    TempBdeck.Push(new CurseCard(4));
                    break;
                case 5:
                    TempBdeck.Push(new BlessCard(5));
                    break;
            }
        }

        for(int i=0; i< Rsize*2; i+=2)
        {
            id = (int)System.Char.GetNumericValue(Rdeck[i])*10;
            id += (int)System.Char.GetNumericValue(Rdeck[i + 1]);
            switch (id)
            {
                case 11:
                    TempRdeck.Push(new MonsterCard(9, 11));
                    break;
                case 12:
                    TempRdeck.Push(new MonsterCard(7,12));
                    break;
                case 13:
                    TempRdeck.Push(new CurseCard(13));
                    break;
                case 14:
                    TempRdeck.Push(new BlessCard(14));
                    break;
                case 15:
                    TempRdeck.Push(new MonsterCard(8, 15));
                    break;
            }
        }

        Deck_Manager.Instance.ReplaceDecks(TempBdeck, TempRdeck);
    }


    void SetTitlesForSelect(int pos)
    {
        if (pos < 100)
        {
            go1 = Tilles["Tile_" + ((pos + DiceValue) % 22)].GetComponent<Button>();
            go2 = Tilles["Tile_" + (22 + pos - DiceValue) % 22].GetComponent<Button>();
        }
        else
        {
            go1 = Tilles["Tile_" + ( ((pos - 100 + DiceValue) % 14) + 100)].GetComponent<Button>();
            go2 = Tilles["Tile_" + (((14 + pos - 100 - DiceValue) % 14) + 100)].GetComponent<Button>();
        }
        UpdateTiles(true, TileColorer);

    }
    void UpdateTiles(bool IsActive, ColorBlock TileColor)
    {
        go1.interactable = IsActive;
        go2.interactable = IsActive;
        go1.colors = TileColor;
        go2.colors = TileColor;

    }
    void Battle(bool Player_Has_Rolled)
    {
        UnityObjects["ReRoll"].SetActive(false);
        UnityObjects["Battle"].SetActive(true);
        if (Player_Has_Rolled)
        {
            TotalPower1 = Stats(GetCurrentPlayer()).pwr + DiceValue;
            UnityObjects["Battle_Roll"].GetComponent<Text>().text = "" + TotalPower1;
            if (HasReRolled == false)
            {
                Invoke("Roll", 1);
            }
        }
        else
        {
            UnityObjects["ReRoll"].SetActive(true);
            TotalPower2 = Deck_Manager.Instance.GetPower() + DiceValue;
            UnityObjects["Battle_Roll"].GetComponent<Text>().text = TotalPower1 + " VS " + TotalPower2;
            BattleResult();
        }
        if (HasReRolled)
        {
            UnityObjects["Battle_Roll"].GetComponent<Text>().text = TotalPower1 + " VS " + TotalPower2;
            BattleResult();
        }

    }

    private void BattleResult()
    {
        UnityObjects["Continue"].SetActive(false);
        if (TotalPower1 > TotalPower2)
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
                if(PlayingMulti)
                     WarpClient.GetInstance().stopGame();

                NoWinner = false;
                UnityObjects["Winning_Msg"].SetActive(true);
                UnityObjects["Winning_Text"].GetComponent<Text>().text = "Player " + GetCurrentPlayer() + "\n Victory!";
            }
            else
            {
                Stats(GetCurrentPlayer()).AddXp(Deck_Manager.Instance.GetPower());
            }
        }
        else if (TotalPower1 < TotalPower2)
        {
            if (Stats(GetCurrentPlayer()).TakeAhit())
            {
                RestartPlayerPos(GetCurrentPlayer());
            }

        }
    }
    void ShowContinueFixBug()
    {
        UnityObjects["Continue"].SetActive(true);
    }

    public void MakeCardShow()
    {
        if (Stats(GetCurrentPlayer()).IsABot == false && isMyTurn)
        {
           Invoke("ShowContinueFixBug", 1.5f);
        }
        else
        {
            Invoke("PassTurn", 1f);
        }
    }
    void ApplyPlayerBattleResult()
    {
        if (TotalPower1 > TotalPower2)
        {
            if (Stats(Opponent).TakeAhit())
            {
                RestartPlayerPos(Opponent);
            }
        }
        else if (TotalPower1 < TotalPower2)
        {
            if (Stats(GetCurrentPlayer()).TakeAhit())
            {
                RestartPlayerPos(GetCurrentPlayer());
            }
        }
    }
    public void SetUpFightPlayer()
    {
        for (int i = 1; i <= PlayerCount; i++)
        {
            if (i != GetCurrentPlayer())
            {
                if (Stats(i).pos == Stats(GetCurrentPlayer()).pos && i <= PlayerCount)
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
        Opponent = x;
        Stats(Opponent).Turn_Status = Global_Variables.turn_Status.BattlePlayer;
        Stats(GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.BattlePlayer;
        for (int i = 1; i <= 4; i++)
        {
            UnityObjects["Fight_Player_" + i].SetActive(false);
        }
        UnityObjects["Drawen_Card"].SetActive(true);
        UnityObjects["Drawen_Card"].GetComponent<Image>().sprite = PlayerBattleBackGround;
        UnityObjects["Card_Text"].GetComponent<Text>().text = "Player " +GetCurrentPlayer() + " VS Player " + Opponent;
        if (PlayingMulti)
        {
            Dictionary<string, object> _toSend = new Dictionary<string, object>();
            _toSend.Add("Move", "FP");
            _toSend.Add("Opponent", Opponent.ToString());
            string _jsonToSend = MiniJSON.Json.Serialize(_toSend);
            WarpClient.GetInstance().SendChat(_jsonToSend);
        }
        EnableDice(true);
    }
    void BattlePlayerPhaseOne()
    {
        UnityObjects["Battle"].SetActive(true);
        TotalPower1 = Stats(GetCurrentPlayer()).pwr + DiceValue;
        UnityObjects["Battle_Roll"].GetComponent<Text>().text = "" + TotalPower1;
        if(isMyTurn)
            UnityObjects["Continue2"].SetActive(true);
    }
    public void BattlePlayerPhaseTwo()
    {
        ChangeCurrentTurnSprite(Opponent);
        UnityObjects["ReRoll"].SetActive(false);
        UnityObjects["Continue2"].SetActive(false);
        OpponentRoll = true;
        if (Stats(Opponent).IsABot)
        {
            Invoke("Roll", 1);
        }
        else
        {
            EnableDice(true);
        }
        if (PlayingMulti)
        {
            EnableDice(false);
            isMyTurn = false;
            Dictionary<string, object> _toSend = new Dictionary<string, object>();
            _toSend.Add("Move", "FP2");
            string _jsonToSend = MiniJSON.Json.Serialize(_toSend);
            WarpClient.GetInstance().SendChat(_jsonToSend);
        }
    }
    void BattlePlayerPhaseThree()
    {
        TotalPower2 = Stats(Opponent).pwr + DiceValue;
        UnityObjects["Battle_Roll"].GetComponent<Text>().text = TotalPower1 + " VS " + TotalPower2;
        if (TotalPower1 > TotalPower2)
        {
            UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = "Player " + GetCurrentPlayer() + " Win!";
        }
        else if (TotalPower1 < TotalPower2)
        {
            UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = "Player " + Opponent + " Win!";
        }
        else
        {
            UnityObjects["Battle_Result_Text"].GetComponent<Text>().text = "Draw!";
        }
        if (PlayingMulti && isMyTurn)
        {
            EnableDice(false);
            isMyTurn = false;
            Dictionary<string, object> _toSend = new Dictionary<string, object>();
            _toSend.Add("Move", "FP3");
            string _jsonToSend = MiniJSON.Json.Serialize(_toSend);
            WarpClient.GetInstance().SendChat(_jsonToSend);
        }
        if (PlayingMulti)
        {
            Invoke("MakeCardShow", 1f);
        }
        else
        {
        MakeCardShow();
        }
    }
    public void SendCardText(string s)
    {
        Dictionary<string, object> _toSend = new Dictionary<string, object>();
        _toSend.Add("Move", "CT");
        _toSend.Add("T", s);
        string _jsonToSend = MiniJSON.Json.Serialize(_toSend);
        WarpClient.GetInstance().SendChat(_jsonToSend);
    } 

    public void ShowStats()
    {
        UnityObjects["Player_Stats"].SetActive(true);
        for (int i = 1; i <= 4; i++)
        {
            UnityObjects["Text_P" + i].GetComponent<Text>().text ="Player "+i +"\nPower: " + Stats(i).pwr + "\nFaith: " + Stats(i).faith + "\nGold: " + Stats(i).gold + "\nHP: " + Stats(i).hp +"\nXp: " + Stats(i).xp;
        }
    }

    public void EnableDice(bool Enabler)
    {
        Dice.GetComponent<Button>().interactable = Enabler;
    }

    public void OnMoveCompleted(MoveEvent _Move)
    {
        Debug.Log("OnMoveCompleted " + _Move.getMoveData() + "" + _Move.getNextTurn());
        if (_Move.getSender() != Global_Variables.User)
        {
            if (_Move.getMoveData() != null)
            {

            }
            else WarpClient.GetInstance().stopGame();
        }
    }

    public void OnChatReceived(ChatEvent eventObj)
    {
        Debug.Log(eventObj.getSender() + " sent " + eventObj.getMessage());
        Dictionary<string, object> _receivedData = MiniJSON.Json.Deserialize(eventObj.getMessage()) as Dictionary<string, object>;
        if (eventObj.getSender() != Global_Variables.User)
        {
            if (eventObj.getMessage() != null)
            {
                if (_receivedData != null)
                {
                    if (_receivedData["Move"].ToString() == "Deck")
                    {
                        ChangeToGamerOwnerDeck(_receivedData["Bdeck"].ToString(), _receivedData["Rdeck"].ToString(),int.Parse(_receivedData["Bsize"].ToString()), int.Parse(_receivedData["Rsize"].ToString()) );
                    }
                    if (_receivedData["Move"].ToString() == "Roll")
                    {
                        DiceValue = int.Parse(_receivedData["Dval"].ToString());
                        StartCoroutine(RollTheDice());
                    }

                    if (_receivedData["Move"].ToString() == "ReRoll")
                    {
                        HasReRolled = true;
                        UnityObjects["ReRoll"].SetActive(false);
                        if (Stats(GetCurrentPlayer()).faith > 0)
                        {
                            Stats(GetCurrentPlayer()).faith--;
                        }
                        if (_receivedData["Moving"].ToString() == "Y")
                        {
                            UpdateTiles(false, Original_TileColor);
                        }
                        DiceValue = int.Parse(_receivedData["Dval"].ToString());
                        StartCoroutine(RollTheDice());
                    }

                    if (_receivedData["Move"].ToString() == "Move")
                    {
                        SubmitMove(int.Parse(_receivedData["Mval"].ToString()));
                    }

                    if(_receivedData["Move"].ToString() == "Draw")
                    {
                        Deck_Manager.Instance.ArrangeDrawenCard(int.Parse(_receivedData["Tile"].ToString()));
                    }

                    if (_receivedData["Move"].ToString() == "CT")
                    {
                        UnityObjects["Card_Text"].GetComponent<Text>().text = _receivedData["T"].ToString();
                    }

                    if (_receivedData["Move"].ToString() == "FP"){
                        Opponent = int.Parse(_receivedData["Opponent"].ToString());
                        EnableDice(false);
                        Stats(GetCurrentPlayer()).Turn_Status = Global_Variables.turn_Status.BattlePlayer;
                        Stats(Opponent).Turn_Status = Global_Variables.turn_Status.BattlePlayer;
                        UnityObjects["Drawen_Card"].SetActive(true);
                        UnityObjects["Drawen_Card"].GetComponent<Image>().sprite = PlayerBattleBackGround;
                        UnityObjects["Card_Text"].GetComponent<Text>().text ="Player " + GetCurrentPlayer() + " VS Player " + Opponent;
                    }

                    if (_receivedData["Move"].ToString() == "FP2")
                    {
                        ChangeCurrentTurnSprite(Opponent);
                        OpponentRoll = true;
                        UnityObjects["ReRoll"].SetActive(false);
                        if (Opponent == Myturn)
                        {
                            EnableDice(true);
                            isMyTurn = true;
                        }
                    }
                    if (_receivedData["Move"].ToString() == "FP3")
                    {
                        isMyTurn = false;
                        BattlePlayerPhaseThree();
                    }

                    if (_receivedData["Move"].ToString() == "End")
                    {
                        Stats(GetCurrentPlayer()).hp = int.Parse(_receivedData["HP"].ToString());
                        Stats(GetCurrentPlayer()).faith = int.Parse(_receivedData["faith"].ToString());
                        Stats(GetCurrentPlayer()).gold = int.Parse(_receivedData["gold"].ToString());
                        Stats(GetCurrentPlayer()).xp = int.Parse(_receivedData["xp"].ToString());
                        Stats(GetCurrentPlayer()).pwr = int.Parse(_receivedData["pwr"].ToString());
                        SubmitTurnPass();
                        if(GetCurrentPlayer() == Myturn)
                        {
                            isMyTurn = true;
                        }
                    }

                }
                else WarpClient.GetInstance().stopGame();
            }
        }


        if (eventObj.getSender() == Global_Variables.User)
        {
            if (_receivedData["Move"].ToString() == "End")
            {
                WarpClient.GetInstance().sendMove("done");
                isMyTurn = false;
                if (GetCurrentPlayer() == Myturn){

                    isMyTurn = true;
                }
            }
            if (_receivedData["Move"].ToString() == "FP2")
            {
                    isMyTurn = false;
            }
            if (_receivedData["Move"].ToString() == "FP3")
            {
                isMyTurn = true;
            }
        }
    }

    void OnGameStopped(string _Sender, string _RoomId)
    {
        NoWinner = false;
        UnityObjects["Winning_Msg"].SetActive(true);
        UnityObjects["Winning_Text"].GetComponent<Text>().text = "Player " + _Sender + "\n Victory!";
        WarpClient.GetInstance().UnsubscribeRoom(_RoomId);
        WarpClient.GetInstance().LeaveRoom(_RoomId);
    }


}
    