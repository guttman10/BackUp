using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UI_Controller : MonoBehaviour {
    public void SinglePlayer()
    {
        UI_Manager.Instance.BtnClick();
        UI_Manager.Instance.SinglePlayer();
    }
    public void Back()
    {
        UI_Manager.Instance.BtnClick();
        UI_Manager.Instance.Back();
    }
    public void MultiPlayer()
    {
        UI_Manager.Instance.BtnClick();
        UI_Manager.Instance.MultiPlayer();
    }
    public void Options()
    {
        UI_Manager.Instance.BtnClick();
        UI_Manager.Instance.Options();
    }
    public void StudMenu()
    {
        UI_Manager.Instance.BtnClick();
        UI_Manager.Instance.StudMenu();
    }
    public void Slider_Players()
    {
        UI_Manager.Instance.UpdateSliderPlayers_Single();
    }
    public void Slider_Music()
    {
        UI_Manager.Instance.UpdateSliderText_Music();
    }
    public void Slider_Sfx()
    {
        UI_Manager.Instance.UpdateSliderText_SFX();
    }
    public void Git()
    {
        UI_Manager.Instance.BtnClick();
        UI_Manager.Instance.Git();
    }
    public void StartGameSingle()
    {
        UI_Manager.Instance.StartGameSingle();
    }
}
