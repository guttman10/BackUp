using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UI_Manager : MonoBehaviour {
    private  Dictionary<string, GameObject> UiObjects;
    enum screen {Main_Menu,Loading,Options,Menu_Multi,Stud_Menu,Menu_Single }
    screen curr;
    Stack<screen> s;
    static UI_Manager inst;
    public GameObject MainGame;
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
    }
    void OnEnable()
    {
        UiObjects["Loading"].SetActive(false);
        UiObjects["Menu_Multi"].SetActive(false);
        UiObjects["Menu_Single"].SetActive(false);
        UiObjects["Options"].SetActive(false);
        UiObjects["Stud_Menu"].SetActive(false);
        curr = screen.Main_Menu;
    }
    void InitDic()
    {
        UiObjects = new Dictionary<string, GameObject>();
        GameObject[] _UiObjects = GameObject.FindGameObjectsWithTag("Ui");
        foreach (GameObject i in _UiObjects)
            UiObjects.Add(i.name, i);
    }
     void changeScreens(screen New)
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



}
