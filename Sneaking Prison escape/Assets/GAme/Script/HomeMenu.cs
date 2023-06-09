using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeMenu : MonoBehaviour
{
    public static HomeMenu Instance;
    public GameObject UI;
    public GameObject LevelUI;
    public GameObject CharectorUI;
    public GameObject LoadingUI;

    [Header("Sound and Music")]
    public Image soundImage;
    public Image musicImage;
    public Sprite soundImageOn, soundImageOff, musicImageOn, musicImageOff;

    public void Awake()
    {
        Instance = this;
        UI.SetActive(true);
        LevelUI.SetActive(false);
        LoadingUI.SetActive(false);

        Time.timeScale = 1;

        if (soundImage)
            soundImage.sprite = GlobalValue.isSound ? soundImageOn : soundImageOff;
        if (musicImage)
            musicImage.sprite = GlobalValue.isMusic ? musicImageOn : musicImageOff;
        if (!GlobalValue.isSound)
            SoundManager.SoundVolume = 0;
        if (!GlobalValue.isMusic)
            SoundManager.MusicVolume = 0;
    }

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        SoundManager.PlayGameMusic();
    }

    public void ShowLevelUI(bool open)
    {
        SoundManager.Click();
        LevelUI.SetActive(open);
        UI.SetActive(!open);
       
    }

    public void ShowCharectUI(bool open)
    {
        SoundManager.Click();

        CharectorUI.SetActive(open);
        UI.SetActive(!open);
    }

    public void LoadLevel()
    {
        LoadingUI.SetActive(true);
        SceneManager.LoadSceneAsync("Level " + GlobalValue.levelPlaying);
    }

    public void LoadTestFeatureScene()
    {
        GlobalValue.levelPlaying = -1; 
        LoadingUI.SetActive(true);
        SceneManager.LoadSceneAsync("Demo");
    }

    #region Music and Sound
    public void TurnSound()
    {
        GlobalValue.isSound = !GlobalValue.isSound;
        soundImage.sprite = GlobalValue.isSound ? soundImageOn : soundImageOff;

        SoundManager.SoundVolume = GlobalValue.isSound ? 1 : 0;
        SoundManager.Click();
    }

    public void TurnMusic()
    {
        GlobalValue.isMusic = !GlobalValue.isMusic;
        musicImage.sprite = GlobalValue.isMusic ? musicImageOn : musicImageOff;

        SoundManager.MusicVolume = GlobalValue.isMusic ? SoundManager.Instance.musicsGameVolume : 0;
        SoundManager.Click();
    }
    #endregion
}