using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpotlight : MonoBehaviour
{
    public LayerMask targetAsLayer;
    public float radius = 4;
    public Transform lightSpotObj;
    public AudioClip shootSound;
    [ReadOnly] public GameObject targetInZone;
    [ReadOnly] public GameObject objectBlockedTarget;

    public LineRenderer gunLineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        gunLineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance && GameManager.Instance.gameState != GameManager.GameState.Playing)
            return;

        RaycastHit hitInZone;
        if (Physics.SphereCast(lightSpotObj.transform.position, radius, lightSpotObj.forward, out hitInZone, 30, targetAsLayer))
        {
            targetInZone = hitInZone.collider.gameObject;
            RaycastHit hit; //check any hit from light to target

            objectBlockedTarget = null;
            if (Physics.Linecast(lightSpotObj.transform.position, hitInZone.point, out hit) == false)
            {
                StartCoroutine(ShootPlayerCo());
            }
            else
                objectBlockedTarget = hit.collider.gameObject;
        }
        else
        {
            targetInZone = null;
            objectBlockedTarget = null;
        }
    }

    IEnumerator ShootPlayerCo()
    {
        SoundManager.PlaySfx(shootSound);
        gunLineRenderer.SetPosition(0, gunLineRenderer.transform.position);
        gunLineRenderer.SetPosition(1, targetInZone.transform.position + Vector3.up);
        gunLineRenderer.enabled = true;

        GameManager.Instance.Player.GetShoot();
        yield return new WaitForSeconds(0.1f);
        gunLineRenderer.enabled = false;
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying)
        {
            RaycastHit hit;
            if(Physics.Raycast(lightSpotObj.transform.position, lightSpotObj.transform.forward, out hit, 30))
            {
                Gizmos.DrawWireSphere(hit.point, radius);
            }
        }
    }
}