using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UI_Manager : MonoBehaviour {
    private static Dictionary<string, GameObject> unityObjects;
    enum screen {Main_Menu,Loading,Options,Menu_Multi,Stud_Menu }
    static screen curr;
    static Stack<screen> s;
    void Awake()
    {
        InitDic();
        s = new Stack<screen>();
        unityObjects["Loading"].SetActive(false);
        unityObjects["Menu_Multi"].SetActive(false);
        unityObjects["Options"].SetActive(false);
        unityObjects["Stud_Menu"].SetActive(false);
        curr = screen.Main_Menu;
    }
    void InitDic()
    {
        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _unityObjects = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject i in _unityObjects)
            unityObjects.Add(i.name, i);
    }
    static void changeScreens(screen New)
    {
        unityObjects[""+curr].SetActive(false);
        unityObjects[""+New].SetActive(true);
        curr = New;
    }
    public static void BtnClick() {
        unityObjects["Click"].GetComponent<AudioSource>().Play(0);
    }
    public static void Git()
    {
        Application.OpenURL("https://github.com/guttman10/");
    }
    public static void SinglePlayer()
    {
        SceneManager.LoadScene("Scene_Game");
    }
    public static void MultiPlayer()
    {
        s.Push(curr);
        changeScreens(screen.Menu_Multi);
    }
    public static void Options()
    {
        s.Push(curr);
        changeScreens(screen.Options);
    }
    public static void StudMenu()
    {
        s.Push(curr);
        changeScreens(screen.Stud_Menu);
    }
    public static void Back()
    {
        changeScreens(s.Pop());
    }
    public static void UpdateSliderText()
    {
        unityObjects["Player_Counter"].GetComponent<Text>().text = unityObjects["Slider_Players"].GetComponent<Slider>().value.ToString();
        
    }
    public static void UpdateSliderText_Music()
    {
        unityObjects["Matrix_Theme"].GetComponent<AudioSource>().volume = unityObjects["Slider_Music"].GetComponent<Slider>().value;
        unityObjects["Music_Val"].GetComponent<Text>().text = ((int)(unityObjects["Slider_Music"].GetComponent<Slider>().value * 100)).ToString();
    }
    public static void UpdateSliderText_SFX()
    {
        unityObjects["Click"].GetComponent<AudioSource>().volume = unityObjects["Slider_SFX"].GetComponent<Slider>().value;
        unityObjects["SFX_Val"].GetComponent<Text>().text = ((int)(unityObjects["Slider_SFX"].GetComponent<Slider>().value*100)).ToString();
    }



}
