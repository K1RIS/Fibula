using UnityEngine;

abstract public class AttackSingleTargetRune : Rune
{
    abstract protected int damage { get; }
    abstract protected string element { get; }

    override public void use(Player player)//do poprawy zeby walil w kazdego na tej kratce
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, LayerMask.GetMask("Monster", "Player")))
        {
            GameObject target = hit.collider.gameObject;
            target.GetComponent<LivingEntity>().photonView.RPC("takeDamage", PhotonTargets.All, damage);            
        }
    }
}
