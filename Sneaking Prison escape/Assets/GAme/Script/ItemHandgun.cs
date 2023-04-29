using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHandgun : TriggerEvent
{
    public AudioClip sound;
    public GameObject collectedFX;
    bool isCollected = false;

    public override void OnContactPlayer()
    {
        if (isCollected)
            return;

        isCollected = true;

        GameManager.Instance.Player.rangeAttack.SetWeaponAvailable(true);

        SoundManager.PlaySfx(sound);
        if (collectedFX)
            Instantiate(collectedFX, transform.position, Quaternion.identity);

        if (GameManager.Instance.Player.meleeAttack.isWeaponShowing())
            GameManager.Instance.Player.meleeAttack.ShowWeapon(false);

        Destroy(gameObject);
    }
}
