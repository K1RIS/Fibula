using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Monster : LivingEntity
{
    override protected void Awake()
    {
        nick = "Potwor";

        maxHp = 200;
        currentHp = 200;

        base.Awake();
    }

    override protected void Update()
    {
        if (!targetAttack.isNull())
        {
            if (targetAttack.distance(colliderSelf.transform.position) > 1.42f)
            {
                if (makingStep == null)
                {
                    Vector3 nextStep = pathFind.DetectWhereToStep(targetAttack.getColliderWorldPosition(), true);
                    makingStep = StartCoroutine(MakeStep(nextStep));
                }
            }
        }

        base.Update();
    }

    private void OnTriggerEnter(Collider player)
    {
        if (player.tag == "Player")
        {
            targetAttack = new TargetAttack(player.gameObject);
        }
    }

    private void OnTriggerExit(Collider player)
    {
        if (targetAttack.equals(player.gameObject))
        {
            targetAttack.stopAttacking();
        }
    }
}
