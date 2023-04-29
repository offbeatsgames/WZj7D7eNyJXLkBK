using UnityEngine;
using System.Collections;

public class RangeAttack : MonoBehaviour
{
    [ReadOnly] public bool weaponAvailable = false;
    public Transform FirePoint;
    public float fireRate = 1;
    public GameObject muzzleTracerFX, muzzleFX;
    [Header("+++BULLET+++")]
     int normalDamage = 30;
    public int bullets = 3;
    [ReadOnly] public int bulletRemains;
    bool isFacingRight = false;
    float nextFire = 0;
    public AudioClip soundAttack;
    public LayerMask targetLayer;
    public float standingTimeWhenSneaking = 0.3f;
    public GameObject gunHolder;

    [HideInInspector]
    public bool isWeaponShowing()
    {
        return gunHolder.activeInHierarchy;
    }

    public void ShowWeapon(bool show)
    {
        gunHolder.SetActive(weaponAvailable && show);
    }

    public void SetWeaponAvailable(bool fromItem)
    {
        if (fromItem)
            bulletRemains = bullets;

        weaponAvailable = true;
        ShowWeapon(true);
    }

    private void Start()
    {
        ShowWeapon(false);
    }

    public bool Fire(bool _isFacingRight)
    {
        if ((bulletRemains > 0) && Time.time > nextFire)
        {
            if (!isWeaponShowing())
                ShowWeapon(true);

            nextFire = Time.time + fireRate;
            bulletRemains--;
            isFacingRight = _isFacingRight;

            //StartCoroutine(FireCo());
            return true;
        }
        else
            return false;
    }

    public void AnimShoot()
    {
        StartCoroutine(FireCo());
    }

    IEnumerator FireCo()
    {
        yield return null;
        //yield return new WaitForSeconds(0.1f);

        SoundManager.PlaySfx(soundAttack);
        Vector2 firepoint = FirePoint.position;

        var _dir = isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit hit;
        Physics.Raycast(firepoint, _dir, out hit, 100, targetLayer);

        //Physics2D.Raycast(FirePoint.position + (isFacingRight ? Vector3.left : Vector3.right), _dir, 100, targetLayer);

        if (muzzleTracerFX)
        {
            var _tempFX = Instantiate(muzzleTracerFX, firepoint, muzzleTracerFX.transform.rotation);
            _tempFX.transform.right = _dir;
        }

        if (muzzleFX)
        {
            var _muzzle = Instantiate(muzzleFX, firepoint, muzzleFX.transform.rotation);
            _muzzle.transform.right = _dir;
        }

        if (hit.collider)
        {
            var takeDamage = (ICanTakeDamage)hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage));
            if (takeDamage != null)
            {
                var finalDamage = normalDamage;

                takeDamage.TakeDamage(finalDamage, Vector2.zero, gameObject, hit.point);
            }
        }
    }
}
