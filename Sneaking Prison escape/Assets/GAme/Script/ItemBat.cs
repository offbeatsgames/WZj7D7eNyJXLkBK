using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBat : TriggerEvent
{
    public AudioClip sound;
    public GameObject collectedFX;
    bool isCollected = false;

    public override void OnContactPlayer()
    {
        if (isCollected)
            return;

        isCollected = true;

        GameManager.Instance.Player.meleeAttack.SetWeaponAvailable();
        SoundManager.PlaySfx(sound);
        if (collectedFX)
            Instantiate(collectedFX, transform.position, Quaternion.identity);

        if (GameManager.Instance.Player.rangeAttack.isWeaponShowing())
            GameManager.Instance.Player.rangeAttack.ShowWeapon(false);

        Destroy(gameObject);
    }
}