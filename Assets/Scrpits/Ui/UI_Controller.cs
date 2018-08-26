using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Controller : MonoBehaviour {
    public GameObject MainGame;
    void Awake()
    {
        MainGame.SetActive(false);
    }
    public void SinglePlayer()
    {
        UI_Manager.BtnClick();
        MainGame.SetActive(true);
        UI_Manager.SinglePlayer();
    }
    public void Back()
    {
        UI_Manager.BtnClick();
        UI_Manager.Back();
    }
    public void MultiPlayer()
    {
        UI_Manager.BtnClick();
        UI_Manager.MultiPlayer();
    }
    public void Options()
    {
        UI_Manager.BtnClick();
        UI_Manager.Options();
    }
    public void StudMenu()
    {
        UI_Manager.BtnClick();
        UI_Manager.StudMenu();
    }
    public void Slider_Players()
    {
        UI_Manager.UpdateSliderText();
    }
    public void Slider_Music()
    {
        UI_Manager.UpdateSliderText_Music();
    }
    public void Slider_Sfx()
    {
        UI_Manager.UpdateSliderText_SFX();
    }
    public void Git()
    {
        UI_Manager.BtnClick();
        UI_Manager.Git();
    }
}
