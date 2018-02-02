using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

abstract public class LivingEntity : Photon.PunBehaviour
{
    //
    private new Rigidbody rigidbody;

    //
    public Canvas canvas;
    public Text nickCanvas;
    public Image hpBar;

    //nick
    protected string nick;
    
    //hp    
    protected int currentHp;
    protected int maxHp;

    //collider
    protected BoxCollider colliderSelf;
    private Vector3 colliderWorldPosition;

    //movement
    protected PathFind pathFind;
    protected Coroutine makingStep;
    protected float walkingSpeed;

    //target
    protected TargetAttack targetAttack;

    //auto attack
    protected Coroutine attackCooldown;
    protected float autoAttackCooldown;
    protected int weaponDamage;

    virtual protected void Awake()
    {
        //
        rigidbody = GetComponent<Rigidbody>();

        //nick  
        nickCanvas.text = nick;

        //hp
        fillHpBar();

        //collider
        colliderSelf = GetComponent<BoxCollider>();
        colliderWorldPosition = colliderSelf.transform.position;

        //movement
        pathFind = GetComponent<PathFind>();
        walkingSpeed = 1f;

        //auto attack
        autoAttackCooldown = 2f;
        weaponDamage = 0;
    }

    virtual protected void Update()
    {        
        AutoAtack();

        if (canvas.transform.rotation != Quaternion.Euler(90f, 0f, 0f))
        {
            canvas.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(currentHp);           
        }
        else
        {
            currentHp = (int)stream.ReceiveNext();
        }
    }

    private void AutoAtack()
    {
        if (targetAttack != null && attackCooldown == null && !targetAttack.isNull())
        {
            targetAttack.dealDamage(weaponDamage);
            attackCooldown = StartCoroutine(WaitForAutoAttackCooldown(autoAttackCooldown));
        }
    }

    private IEnumerator WaitForAutoAttackCooldown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        attackCooldown = null;
    }

    [PunRPC]
    public void takeDamage(int damage)
    {
        currentHp -= damage;

        if (currentHp < 1)
        {
            PhotonNetwork.Destroy(gameObject);
        }

        fillHpBar();
    }

    [PunRPC]
    public void heal(int heal)
    {
        if(currentHp + heal > maxHp)
        {
            currentHp = maxHp;
        }
        else
        {
            currentHp += heal;
        }        
        fillHpBar();
    }

    private void fillHpBar()
    {
        hpBar.fillAmount = ((float)currentHp / (float)maxHp);
    }

    protected IEnumerator MakeStep(Vector3 direction)
    {
        Vector3 startPosition = transform.position;
        Vector3 destinationPosition = transform.position + direction;
        float stepDuration = walkingSpeed;
        float timeForStep = 0, timeAnimation = 0;
        Vector3 coliderCeneter = colliderSelf.center;
        Vector3 coliderDestination = Vector3.forward;

        if (!(direction == Vector3.left || direction == Vector3.right || direction == Vector3.forward || direction == Vector3.back))
        {
            stepDuration *= 2.1f;
            coliderDestination = new Vector3(-1f, 0f, 1f);
            if (direction == new Vector3(1f, 0f, -1f) || direction == new Vector3(-1f, 0f, 1f))
            {
                coliderDestination = new Vector3(1f, 0f, 1f);
            }
        }

        colliderWorldPosition = colliderSelf.transform.position + direction;

        if (!Physics.CheckSphere(destinationPosition, 0.1f))
        {
            rotateCharacter(direction);

            while (timeForStep < stepDuration)
            {
                Vector3 position = Vector3.Lerp(startPosition, destinationPosition, timeAnimation);
                colliderSelf.center = Vector3.Lerp(coliderDestination, coliderCeneter, timeAnimation);
                transform.position = position;
                rigidbody.MovePosition(position);
                timeForStep += Time.deltaTime;
                timeAnimation += Time.deltaTime / stepDuration;
                yield return new WaitForEndOfFrame();
            }
            colliderSelf.center = Vector3.zero;
            transform.position = destinationPosition;
            rigidbody.MovePosition(destinationPosition);
        }
        makingStep = null;
    }

    private void rotateCharacter(Vector3 direction)
    {
        Quaternion way = new Quaternion();

        if (direction.x == -1)
        {
            way = Quaternion.Euler(0f, 270f, 0f);            
        }
        else if (direction.x == 1)
        {
            way = Quaternion.Euler(0f, 90f, 0f);
        }
        else if (direction.z == -1)
        {
            way = Quaternion.Euler(0f, 180f, 0f);
        }
        else if (direction.z == 1)
        {
            way = Quaternion.Euler(0f, 0f, 0f);
        }

        transform.rotation = way;
        rigidbody.MoveRotation(way);
    }

    public Vector3 getColliderWorldTransform()
    {
        return colliderWorldPosition;
    }
}