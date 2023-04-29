using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
 /*   public static AdsManager Instance;
    //delegate   ()
    public delegate void RewardedAdResult(bool isSuccess);

    //event  
    public static event RewardedAdResult AdResult;

    public enum AD_NETWORK { Unity, Admob }

    //[Header("REWARDED VIDEO AD")]
    [HideInInspector] public AD_NETWORK rewardedUnit;
    [HideInInspector] public float timePerWatch = 90;
    float lastTimeWatch = -999;

    [Header("SHOW AD VICTORY/GAMEOVER")]
    public AD_NETWORK adGameOverUnit;
    public int showAdGameOverCounter = 2;
    int counter_gameOver = 0;
    public int showAdVictoryCounter = 1;
    int counter_victory = 0;

    private void Awake()
    {
        if (AdsManager.Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void ShowAdmobBanner(bool show)
    {
        AdmobController.Instance.ShowBanner(show);
    }

    #region NORMAL AD

    public void ShowNormalAd(GameManager.GameState state)
    {
        Debug.Log("SHOW NORMAL AD " + state);

        if (state == GameManager.GameState.GameOver)
            StartCoroutine(ShowNormalAdCo(state, 0.8f));
        else
            StartCoroutine(ShowNormalAdCo(state, 0));
    }

    IEnumerator ShowNormalAdCo(GameManager.GameState state, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (state == GameManager.GameState.GameOver)
        {
            counter_gameOver++;
            if (counter_gameOver >= showAdGameOverCounter)
            {
                if (adGameOverUnit == AD_NETWORK.Unity)
                {
                    //try show Unity video
                    if (UnityAds.Instance.ForceShowNormalAd())
                    {
                        counter_gameOver = 0;
                    }
                }
                else if (adGameOverUnit == AD_NETWORK.Admob)
                {
                    if (AdmobController.Instance.ForceShowInterstitialAd())
                    {
                        counter_gameOver = 0;
                    }
                }
            }
        }
        else if (state == GameManager.GameState.Finish)
        {
            counter_victory++;
            if (counter_victory >= showAdVictoryCounter)
            {
                if (adGameOverUnit == AD_NETWORK.Unity)
                {
                    //try show Unity video
                    if (UnityAds.Instance.ForceShowNormalAd())
                    {
                        counter_victory = 0;
                    }
                }
                else if (adGameOverUnit == AD_NETWORK.Admob)
                {
                    if (AdmobController.Instance.ForceShowInterstitialAd())
                    {
                        counter_victory = 0;
                    }
                }
            }
        }
        //}
    }

    public void ResetCounter()
    {
        counter_gameOver = 0;
        //counter_gameFinish = 0;
    }

    #endregion

    #region REWARDED VIDEO AD

    bool _isRewardedAdReady = false;

    public bool isRewardedAdReady()
    {
        if (_isRewardedAdReady)
            return true;

        if ((rewardedUnit == AD_NETWORK.Unity) && UnityAds.Instance.isRewardedAdReady())
        {
            _isRewardedAdReady = true;
            return true;
        }
        else
        {
            if (AdmobController.Instance.isRewardedVideoAdReady())
            {
                _isRewardedAdReady = true;
                return true;
            }
        }

        if ((rewardedUnit == AD_NETWORK.Admob) && AdmobController.Instance.isRewardedVideoAdReady())
        {
            _isRewardedAdReady = true;
            return true;
        }
        else
        {
            if (UnityAds.Instance.isRewardedAdReady())
            {
                _isRewardedAdReady = true;
                return true;
            }
        }
        return false;
    }

    public float TimeWaitingNextWatch()
    {
        return timePerWatch - (Time.realtimeSinceStartup - lastTimeWatch);
    }

    public void ShowRewardedAds()
    {
        _isRewardedAdReady = false;
        lastTimeWatch = Time.realtimeSinceStartup;

        if (rewardedUnit == AD_NETWORK.Unity)
        {
            UnityAds.AdResult += UnityAds_AdResult;
            UnityAds.Instance.ShowRewardVideo();
        }
        else
        {
            AdmobController.AdResult += AdmobController_AdResult;
            AdmobController.Instance.WatchRewardedVideoAd();
        }


    }

    private void AdmobController_AdResult(bool isWatched)
    {
        AdmobController.AdResult -= AdmobController_AdResult;
        AdResult(true);
    }

    private void UnityAds_AdResult(WatchAdResult result)
    {
        UnityAds.AdResult -= UnityAds_AdResult;
        AdResult(result == WatchAdResult.Finished);
    }

    #endregion*/
}
