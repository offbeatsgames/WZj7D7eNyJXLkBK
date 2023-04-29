using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public enum GameState { Waiting, Playing, GameOver, Finish }
    [ReadOnly] public GameState gameState;
    [ReadOnly] public PlayerController playerController;

    //define player reborn event, it will called all registered objects
    public delegate void OnPlayerReborn();
    public static OnPlayerReborn playerRebornEvent;

    GameObject clonePlayer;

    public PlayerController Player
    {
        get
        {
            if (playerController != null)
                return playerController;
            else
            {
                playerController = FindObjectOfType<PlayerController>();
                clonePlayer = Instantiate(playerController.gameObject);
                clonePlayer.SetActive(false);
                if (playerController)
                    return playerController;
                else
                    return null;
            }
        }
    }

    public void SetGameState(GameState state)
    {
        gameState = state;
    }

    [ReadOnly] public Vector3 checkPoint;
    [ReadOnly] public Vector3 HelicheckPoint;

    
    public Helicopter Heli;
    

    [SerializeField] private bool isChaseLevel;

    public void SetCheckPoint(Vector3 pos)
    {
        checkPoint = pos;

        if(isChaseLevel)
        {
            HelicheckPoint = Heli.transform.position;
        }
       
    }

    public void Awake()
    {
        Application.targetFrameRate = 60;

        Instance = this;
        var _startPoint = GameObject.Find("Startpoint");
        if (_startPoint)
        {
            Player.gameObject.transform.position = _startPoint.transform.position;
            SetCheckPoint(_startPoint.transform.position);
        }
        else
            Debug.Log("Can't  find the Startpoint on the scene! Please check and make sure it visible on the Scene level!");
    }

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        gameState = GameState.Playing;
        SoundManager.PlayGameMusic();

        if (checkPoint == Vector3.zero)
            checkPoint = Player.transform.position;
        
        if(isChaseLevel)
        {
            Heli = FindObjectOfType<Helicopter>();
            HelicheckPoint = Heli.transform.position;
        }
    }

    public void GameOver()
    {
        if (gameState == GameState.GameOver)
            return;
        SoundManager.Instance.PauseMusic(true);
        Time.timeScale = 1;
        gameState = GameState.GameOver;
      /*  if (AdsManager.Instance)
            AdsManager.Instance.ShowNormalAd(GameState.GameOver);*/

        MenuManager.Instance.ShowUI(false);

        Invoke("Continue", 3);

    }

    public void FinishGame()
    {
        if (gameState == GameState.Finish)
            return;
        
        gameState = GameState.Finish;

        if(GlobalValue.levelPlaying >= GlobalValue.LevelHighest)
        {
            GlobalValue.LevelHighest++;
        }

        MenuManager.Instance.Finish();
       /* if (AdsManager.Instance)
            AdsManager.Instance.ShowNormalAd(GameState.Finish);*/

        SoundManager.PlaySfx(SoundManager.Instance.soundGamefinish);
    }

    public void Continue()
    {
        if (playerRebornEvent != null)
            playerRebornEvent();

        SoundManager.Instance.PauseMusic(false);

        Invoke("SpawnPlayer", 0.1f);
    }

    void SpawnPlayer()
    {
        bool isHasMeleeWeapon = playerController.meleeAttack.weaponAvailable;
        bool isHasGunWeapon = playerController.rangeAttack.weaponAvailable;
        int bulletRemains = playerController.rangeAttack.bulletRemains;
        Destroy(playerController.gameObject);

        playerController = Instantiate(clonePlayer, checkPoint, Quaternion.identity).GetComponent<PlayerController>();
        playerController.gameObject.SetActive(true);
        if (isHasMeleeWeapon)
            playerController.GetComponent<MeleeAttack>().SetWeaponAvailable();
        if (isHasGunWeapon)
        {
            playerController.GetComponent<RangeAttack>().SetWeaponAvailable(false);
            playerController.GetComponent<RangeAttack>().bulletRemains = bulletRemains;
        }

        if(isChaseLevel)
            Heli.transform.position = HelicheckPoint;


        gameState = GameState.Playing;
        MenuManager.Instance.ShowUI(true);
    }
}