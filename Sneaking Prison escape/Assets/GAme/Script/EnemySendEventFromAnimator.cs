using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySendEventFromAnimator : MonoBehaviour
{
    public MeleeEnemy meleeEnemy;

    public void AnimMeleeAttackStart()
    {
        meleeEnemy.AnimMeleeAttackStart();
    }

    public void AnimMeleeAttackEnd()
    {
        meleeEnemy.AnimMeleeAttackEnd();
    }
}
