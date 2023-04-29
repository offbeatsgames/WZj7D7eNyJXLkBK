using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPushAction : MonoBehaviour
{
    Animator anim;

    public GameObject targetObject;
    public string callEventMessage = "Action";
    public float delayCall = 1f;
    public AudioClip sound;
    public float delaySoundClick = 1;

    public GameObject tutorialInfor;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (GameManager.Instance.gameState != GameManager.GameState.Playing)
            return;

        if (tutorialInfor && GameManager.Instance.Player && GameManager.Instance.Player.playerCheckPushButtonObject)
            tutorialInfor.SetActive(GameManager.Instance.Player.playerCheckPushButtonObject.currentButtonObject == this);
    }

    public void Action()
    {
        StartCoroutine(ActionCo());
    }

    IEnumerator ActionCo()
    {
        GameManager.Instance.Player.ForcePlayerStanding(true);
        yield return new WaitForSeconds(delaySoundClick);
        SoundManager.PlaySfx(sound);
        anim.SetTrigger("push");
        yield return new WaitForSeconds(delayCall);
        GameManager.Instance.Player.ForcePlayerStanding(false);

        targetObject.SendMessage(callEventMessage, SendMessageOptions.DontRequireReceiver);
    }
}