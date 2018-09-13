using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using AssemblyCSharp;

public class UI_Manager : MonoBehaviour {
    private string apiKey = "7eb59baffee6ead1768c8c3a3a4140e061c63617942f827afa239ad9ecf77409";
    private string secretKey = "f9c4ddf4472c0b9fcc48db07648d2b0b9380922dfaedb33cb9381203b92c454d";
    private Dictionary<string, object> matchRoomData;
    private Dictionary<string, GameObject> UiObjects;
    private List<string> rooms;
    private int index = 0;
    private string roomId = string.Empty;
    public enum screen {Main_Menu,Loading,Options,Menu_Multi,Stud_Menu,Menu_Single,Login }
    screen curr;
    Stack<screen> s;
    string Pass;
    static UI_Manager inst;
    public GameObject MainGame;
    private Listener listen;
    public static UI_Manager Instance
    {
        get
        {
            if (inst == null)
                inst = GameObject.Find("UI_Manager").GetComponent<UI_Manager>();

            return inst;
        }
    }
    void Awake()
    {
        InitDic();
        s = new Stack<screen>();
        MainGame.SetActive(false);
        curr = screen.Login;
        if (listen == null)
            listen = new Listener();
        UiObjects["Main_Menu"].SetActive(false);
        UiObjects["Loading"].SetActive(false);
        UiObjects["Menu_Multi"].SetActive(false);
        UiObjects["Menu_Single"].SetActive(false);
        UiObjects["Options"].SetActive(false);
        UiObjects["Stud_Menu"].SetActive(false);
    }
    private void OnEnable()
    {
        Listener.OnConnect += OnConnect;
        Listener.OnRoomsInRange += OnRoomsInRange;
        Listener.OnCreateRoom += OnCreateRoom;
        Listener.OnGetLiveRoomInfo += OnGetLiveRoomInfo;
        Listener.OnJoinRoom += OnJoinRoom;
        Listener.OnUserJoinRoom += OnUserJoinRoom;
        Listener.OnGameStarted += OnGameStarted;
    }

    private void OnDisable()
    {
        Listener.OnConnect -= OnConnect;
        Listener.OnRoomsInRange -= OnRoomsInRange;
        Listener.OnCreateRoom -= OnCreateRoom;
        Listener.OnGetLiveRoomInfo -= OnGetLiveRoomInfo;
        Listener.OnJoinRoom -= OnJoinRoom;
        Listener.OnUserJoinRoom -= OnUserJoinRoom;
        Listener.OnGameStarted -= OnGameStarted;
    }

    void InitDic()
    {
        UiObjects = new Dictionary<string, GameObject>();
        GameObject[] _UiObjects = GameObject.FindGameObjectsWithTag("Ui");
        foreach (GameObject i in _UiObjects)
            UiObjects.Add(i.name, i);
    }
    public void changeScreens(screen New)
    {
        UiObjects[""+curr].SetActive(false);
        UiObjects[""+New].SetActive(true);
        curr = New;
    }
    public void BtnClick() {
        UiObjects["Click"].GetComponent<AudioSource>().Play(0);
    }
    public void Git()
    {
        Application.OpenURL("https://github.com/guttman10/");
    }
    public void SinglePlayer() {
        s.Push(curr);
        changeScreens(screen.Menu_Single);
    }
    public void StartGameSingle()
    {
        MainGame.SetActive(true);
        SC_Logics.Instance.Stats(2).IsABot = true;
        SC_Logics.Instance.Stats(3).IsABot = true;
        SC_Logics.Instance.Stats(4).IsABot = true;
        UiObjects["Ui"].SetActive(false);

    }
    public void MultiPlayer()
    {
        s.Push(curr);
        changeScreens(screen.Menu_Multi);
    }
    public void Options()
    {
        s.Push(curr);
        changeScreens(screen.Options);
    }
    public void StudMenu()
    {
        s.Push(curr);
        changeScreens(screen.Stud_Menu);
    }
    public void Back()
    {
        changeScreens(s.Pop());
    }
    public void UpdateSliderPlayers_Single()
    {
        UiObjects["Player_Counter_Single"].GetComponent<Text>().text = UiObjects["Slider_Players_Single"].GetComponent<Slider>().value.ToString();
        MainGame.GetComponent<SC_Logics>().PlayerCount = (int)UiObjects["Slider_Players_Single"].GetComponent<Slider>().value;
    }
    public void UpdateSliderPlayers_Multi()
    {
        UiObjects["Player_Counter_Multi"].GetComponent<Text>().text = UiObjects["Slider_Players_Multi"].GetComponent<Slider>().value.ToString();
        MainGame.GetComponent<SC_Logics>().PlayerCount = (int)UiObjects["Slider_Players_Multi"].GetComponent<Slider>().value;
    }
    public void UpdateSliderText_Music()
    {
        UiObjects["Matrix_Theme"].GetComponent<AudioSource>().volume = UiObjects["Slider_Music"].GetComponent<Slider>().value;
        UiObjects["Music_Val"].GetComponent<Text>().text = ((int)(UiObjects["Slider_Music"].GetComponent<Slider>().value * 100)).ToString();
    }
    public void UpdateSliderText_SFX()
    {
        UiObjects["Click"].GetComponent<AudioSource>().volume = UiObjects["Slider_SFX"].GetComponent<Slider>().value;
        UiObjects["SFX_Val"].GetComponent<Text>().text = ((int)(UiObjects["Slider_SFX"].GetComponent<Slider>().value*100)).ToString();
    }
    public void UpdateUser()
    {
        Global_Variables.User = UiObjects["Input_User"].GetComponent<InputField>().text;
    }

    public void UpdatePass()
    {
        Pass = UiObjects["Input_Pass"].GetComponent<InputField>().text;
    }
    public void VerifyLogin()
    {
        if (Pass == Global_Variables.Pass)
            changeScreens(screen.Main_Menu);
        else
        {
            UiObjects["ERR_MSG"].GetComponent<Text>().text = "Invalid Password";
        }
    }
    private void UpdateStatus(string _NewStatus)
    {
        UiObjects["Status"].GetComponent<Text>().text = _NewStatus;
    }
    public void Play_MultiPlayer()
    {
        changeScreens(screen.Loading);
        MainGame.GetComponent<SC_Logics>().PlayingMulti = true;
        WarpClient.initialize(apiKey, secretKey);
        WarpClient.GetInstance().AddConnectionRequestListener(listen);
        WarpClient.GetInstance().AddChatRequestListener(listen);
        WarpClient.GetInstance().AddUpdateRequestListener(listen);
        WarpClient.GetInstance().AddLobbyRequestListener(listen);
        WarpClient.GetInstance().AddNotificationListener(listen);
        WarpClient.GetInstance().AddRoomRequestListener(listen);
        WarpClient.GetInstance().AddZoneRequestListener(listen);
        WarpClient.GetInstance().AddTurnBasedRoomRequestListener(listen);

        matchRoomData = new Dictionary<string, object>
        {
            { "Password", "Shenkar11" },
            { "PlayerCount", MainGame.GetComponent<SC_Logics>().PlayerCount }
        };
        WarpClient.GetInstance().Connect(Global_Variables.User);
        UpdateStatus("Connecting...");
    }

    private void OnConnect(bool _IsSuccess)
    {
        Debug.Log(_IsSuccess);
        if (_IsSuccess)
        {
            UpdateStatus("Connected!");
            WarpClient.GetInstance().GetRoomsInRange(1, MainGame.GetComponent<SC_Logics>().PlayerCount);
        }
        else
        {
            UpdateStatus("Error connecting");
            changeScreens(screen.Menu_Multi);
        }
    }
    public void OnRoomsInRange(bool _IsSuccess, MatchedRoomsEvent eventObj)
    {
        if (_IsSuccess)
        {
        UpdateStatus("Parsing rooms..");
            rooms = new List<string>();
            foreach (var roomData in eventObj.getRoomsData())
            {
                Debug.Log("Room Id: " + roomData.getId());
                Debug.Log("Room Owner: " + roomData.getRoomOwner());
                rooms.Add(roomData.getId());
            }

            index = 0;
            DoRoomSearchLogic();
        }
        else
        {
            UpdateStatus("Error finding room..");
            changeScreens(screen.Menu_Multi);
        }
    }
    public void DoRoomSearchLogic()
    {
        if (index < rooms.Count)
        {
            UpdateStatus("Getting room Details (" + rooms[index] + ")");
            WarpClient.GetInstance().GetLiveRoomInfo(rooms[index]);
        }
        else
        {
            UpdateStatus("Creating Room...");
            WarpClient.GetInstance().CreateTurnRoom( Global_Variables.User+"'s Room", Global_Variables.User, MainGame.GetComponent<SC_Logics>().PlayerCount, matchRoomData, 120);
        }
    }
        private void OnCreateRoom(bool _IsSuccess, string _RoomId)
    {
        if (_IsSuccess)
        {
            Debug.Log("Room Created! " + _RoomId);
            UpdateStatus("Room Created! " + _RoomId);
            roomId = _RoomId;
            WarpClient.GetInstance().JoinRoom(_RoomId);
            WarpClient.GetInstance().SubscribeRoom(_RoomId);
            MainGame.GetComponent<SC_Logics>().Myturn = 1;
        }
        else
        {
            UpdateStatus("Error creating room");
            changeScreens(screen.Menu_Multi);
        }
    }
    public void OnGetLiveRoomInfo(LiveRoomInfoEvent eventObj)
    {
        Dictionary<string, object> _parms = eventObj.getProperties();
        List<string> UserList =new List <string> (eventObj.getJoinedUsers());
        if (_parms["Password"].ToString() == matchRoomData["Password"].ToString() && int.Parse(_parms["PlayerCount"].ToString()) == MainGame.GetComponent<SC_Logics>().PlayerCount)
        {
            roomId = eventObj.getData().getId();
            WarpClient.GetInstance().JoinRoom(roomId);
            WarpClient.GetInstance().SubscribeRoom(roomId);
            MainGame.GetComponent<SC_Logics>().Myturn = UserList.Count + 1;
        }
        else
        {
            index++;
            DoRoomSearchLogic();
        }
    }

    public void OnJoinRoom(bool _IsSuccess, string _RoomId)
    {
        if (_IsSuccess)
        {
            UpdateStatus("Joined Room: " + _RoomId);
            if (MainGame.GetComponent<SC_Logics>().Myturn == MainGame.GetComponent<SC_Logics>().PlayerCount)
            {
                WarpClient.GetInstance().startGame();
            }
        }
    }

    public void OnUserJoinRoom(RoomData eventObj, string _UserName)
    {
        UpdateStatus("User: " + _UserName + " Joined Room");
    }

    public void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        Debug.Log("OnGameStarted");
        MainGame.SetActive(true);
        UiObjects["Ui"].SetActive(false);
        if (MainGame.GetComponent<SC_Logics>().Myturn == MainGame.GetComponent<SC_Logics>().GetCurrentPlayer())
        {
            MainGame.GetComponent<SC_Logics>().MakeUnanimousDeck();
            MainGame.GetComponent<SC_Logics>().isMyTurn = true;
        }
        else
        {
            MainGame.GetComponent<SC_Logics>().isMyTurn = false;
        }
    }
}
