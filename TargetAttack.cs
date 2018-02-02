using UnityEngine;

public class TargetAttack
{
    private GameObject target;
    private BoxCollider targetCollider;
    private LivingEntity targetClass;

    public TargetAttack(GameObject _target)
    {
        target = _target;
        targetClass = target.GetComponent<LivingEntity>();
        targetCollider = target.GetComponent<BoxCollider>();
    }

    public void dealDamage(int damage)
    {
        targetClass.takeDamage(damage);
    }

    public void stopAttacking()
    {
        target = null;
    }

    public bool isNull()
    {
        if (target == null)
        {
            return true;
        }
        return false;
    }

    public bool equals(GameObject obj)
    {
        if(target == obj)
        {
            return true;
        }
        return false;
    }

    public float distance(Vector3 from)
    {
        return Vector3.Distance(targetCollider.transform.position, from);
    }

    public Vector3 getColliderWorldPosition()
    {
        return targetClass.getColliderWorldTransform();
    }
}
