using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public EndCutscreenManager endscreenmanger;
    public static MenuManager Instance;
    public GameObject uI, finish, pauseUI, LoadingUI;
    public GameObject[] butNext;

    public GameObject meleeIcon, gunIcon;
    public Text levelTxt;
    [Header("Sound and Music")]
    public Image soundImage;
    public Image musicImage;
    public Sprite soundImageOn, soundImageOff, musicImageOn, musicImageOff;
    public Text bulletLeftTxt;

    [Header("Jetpack bar")]
    public Slider jetpackSlider;
    public Text txtJetpackRemainPercent;

    [Header("OXYGEN BAR")]
    public Slider oxygenSlider;
    public Text txtOxygenRemainPercent;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        uI.SetActive(true);
        pauseUI.SetActive(false);
        LoadingUI.SetActive(false);
        endscreenmanger = FindObjectOfType<EndCutscreenManager>();
       

        levelTxt.text = SceneManager.GetActiveScene().name;

        if (Time.timeScale == 0)
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

    private void Update()
    {
        if (GameManager.Instance.gameState == GameManager.GameState.Playing && GameManager.Instance.Player && GameManager.Instance.Player.meleeAttack)
            meleeIcon.SetActive(GameManager.Instance.Player.meleeAttack.weaponAvailable);
        if (GameManager.Instance.gameState == GameManager.GameState.Playing && GameManager.Instance.Player && GameManager.Instance.Player.rangeAttack)
            gunIcon.SetActive(GameManager.Instance.Player.rangeAttack.weaponAvailable);

        bulletLeftTxt.text = GameManager.Instance.Player.rangeAttack.bulletRemains + "";
        jetpackSlider.gameObject.SetActive(GameManager.Instance.Player.isJetpackActived);
        jetpackSlider.value = GameManager.Instance.Player.jetpackRemainTime / GameManager.Instance.Player.jetpackDrainTimeOut;
        txtJetpackRemainPercent.text = ((GameManager.Instance.Player.jetpackRemainTime / GameManager.Instance.Player.jetpackDrainTimeOut) * 100).ToString("0") + "%";

        oxygenSlider.gameObject.SetActive(GameManager.Instance.Player.playerCheckWater.isUnderWater);
        oxygenSlider.value = GameManager.Instance.Player.playerCheckWater.oxygenRemainTime / GameManager.Instance.Player.playerCheckWater.oxygenDrainTimeOut;
        txtOxygenRemainPercent.text = ((GameManager.Instance.Player.playerCheckWater.oxygenRemainTime / GameManager.Instance.Player.playerCheckWater.oxygenDrainTimeOut) * 100).ToString("0") + "%";
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

    public void Finish()
    {
        uI.SetActive(false);
        Invoke("FinishCo", 10);
       endscreenmanger.playCutscreen();
        
    }

    void FinishCo()
    {
        foreach (var but in butNext)
        {
            but.SetActive(GlobalValue.levelPlaying != -1 && (GlobalValue.levelPlaying < GlobalValue.LevelHighest));
        }

        finish.SetActive(true);
    }

    public void ShowUI(bool open)
    {
        uI.SetActive(open);
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Pause(bool pause)
    {
        pauseUI.SetActive(pause);
        Time.timeScale = pause ? 0 : 1;

        SoundManager.Instance.PauseMusic(pause);
    }

    public void NextLevel()
    {
        GlobalValue.levelPlaying++;
        LoadingUI.SetActive(true);
        SceneManager.LoadSceneAsync("Level " + GlobalValue.levelPlaying);
    }

    public void Home()
    {
        GlobalValue.levelPlaying = -1;
        LoadingUI.SetActive(true);
        SceneManager.LoadSceneAsync("HomeScene");
    }
}
