using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Player : LivingEntity
{
    //camera
    private Camera cam;

    //eq
    public GameObject[] equipment;
    private enum equipmentSlotNumber { Helmet, Armor, Legs, Boots, LeftHand, RightHand, Amulet, Container, Ring1, Ring2, Random, Arrow };

    //movement stuff
    private Vector3 pointToAutoMove;
    private KeyCode lastClickedMovementButton;
    private bool makeQueuedStep = false;

    //interaction with items
    public GameObject attachedItem;
    private GameObject lastClickedEqSlot;

    //look
    private bool looking = false;

    //gowno
    private RaycastHit hit;

    //runes
    private bool usingRune = false;
    private GameObject rune;
    [SerializeField] private GameObject magicWall;

    //test
    public Texture2D cursor1;
    public Texture2D cursor2;
    public Texture2D cursor3;

    private List<Container> containers;



    override protected void Awake()
    {
        DontDestroyOnLoad(gameObject);

        maxHp = 200;
        currentHp = 150;

        equipment = new GameObject[12];

        base.Awake();

        containers = new List<Container>();

        if (!photonView.isMine)
        {            
            enabled = false;
        }
    }

    private void Start()
    {
        cam = Camera.main;
    }

    override protected void Update()
    {
        autoMove();
        movement();

        selectOrUnselectAttackTarget();

        setLooking();
        lookAt();

        manageItems();

        useRune();

        base.Update();

        ///test
        if (looking)
        {
            Cursor.SetCursor(cursor2, Vector2.zero, CursorMode.Auto);
        }
        else if (attachedItem != null)
        {
            Cursor.SetCursor(cursor1, Vector2.zero, CursorMode.Auto);
        }
        else if (usingRune)
        {
            Cursor.SetCursor(cursor3, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        
        openBackPack();
    }

    private void FixedUpdate()
    {
        moveCamera();
    }

    private void moveCamera()
    {
        if (cam.transform.rotation != Quaternion.Euler(90f, 0f, 0f))
        {
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        cam.transform.position = (transform.position + new Vector3(0f, 14f, 0f));
    }

    private void autoMove()
    {
        setPointToAutoMove();
        autoMoveToPoint();
    }

    private void setPointToAutoMove()
    {
        if (Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1))
        {
            if (!looking && !usingRune && attachedItem == null)
            {
                if (!Utility.canvasClicked())
                {
                    Vector3 mousePosition = Utility.convertMousePositionToVector3("Terrain");
                    if (mousePosition != Vector3.zero)
                    {
                        lastClickedMovementButton = KeyCode.None;
                        pointToAutoMove = mousePosition;
                    }
                }
            }
        }
    }

    private void autoMoveToPoint()
    {
        if (!(pointToAutoMove == Vector3.zero))
        {
            if (colliderSelf.transform.position != pointToAutoMove)
            {
                if (!moving())
                {
                    Vector3 direction = pathFind.DetectWhereToStep(pointToAutoMove);
                    if (direction == Vector3.zero)
                    {
                        pointToAutoMove = Vector3.zero;
                    }
                    else
                    {
                        makingStep = StartCoroutine(MakeStep(direction));
                    }
                }
            }
            else
            {
                pointToAutoMove = Vector3.zero;
            }
        }
    }

    private void movement()
    {
        if (movementButtonClicked())
        {
            stopMovingToClickedPoint();
            KeyCode secondLastClickedMovementButton = lastClickedMovementButton;
            setLastClickedMovementButton();

            if (!moving())
            {
                move();
            }
            else
            {
                if (secondLastClickedMovementButton != lastClickedMovementButton)
                {
                    makeQueuedStep = true;
                }
            }
        }

        if (Input.GetKey(lastClickedMovementButton))
        {
            stopMovingToClickedPoint();
            if (!moving())
            {
                move();
                makeQueuedStep = false;
            }
        }

        if (makeQueuedStep && !moving())
        {
            move();
            makeQueuedStep = false;
        }
    }

    private bool movementButtonClicked()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Home) ||
            Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.PageUp) ||
            Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.End) ||
            Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.PageDown))
        {
            return true;
        }
        return false;
    }

    private void stopMovingToClickedPoint()
    {
        if (pointToAutoMove != Vector3.zero)
        {
            pointToAutoMove = Vector3.zero;
        }
    }

    private void setLastClickedMovementButton()
    {
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(kcode))
            {
                if (lastClickedMovementButton != kcode)
                {
                    lastClickedMovementButton = kcode;
                }
                else
                {
                    makeQueuedStep = false;
                }
            }
        }
    }

    private bool moving()
    {
        if (makingStep == null)
        {
            return false;
        }
        return true;
    }

    private void move()
    {
        makingStep = StartCoroutine(MakeStep(detectWhichDirectionWalk(lastClickedMovementButton)));
    }

    private Vector3 detectWhichDirectionWalk(KeyCode clickedButton)
    {
        Vector3 direction = Vector3.zero;

        switch (clickedButton)
        {
            case KeyCode.W:
            case KeyCode.Keypad8:
            case KeyCode.UpArrow:
                direction = Vector3.forward;
                break;

            case KeyCode.S:
            case KeyCode.Keypad2:
            case KeyCode.DownArrow:
                direction = Vector3.back;
                break;

            case KeyCode.A:
            case KeyCode.Keypad4:
            case KeyCode.LeftArrow:
                direction = Vector3.left;
                break;

            case KeyCode.D:
            case KeyCode.Keypad6:
            case KeyCode.RightArrow:
                direction = Vector3.right;
                break;

            case KeyCode.Keypad7:
            case KeyCode.Home:
                direction = new Vector3(-1f, 0f, 1f);
                break;

            case KeyCode.Keypad9:
            case KeyCode.PageUp:
                direction = new Vector3(1f, 0f, 1f);
                break;

            case KeyCode.Keypad1:
            case KeyCode.End:
                direction = new Vector3(-1f, 0f, -1f);
                break;

            case KeyCode.Keypad3:
            case KeyCode.PageDown:
                direction = new Vector3(1f, 0f, -1f);
                break;
        }

        return direction;
    }

    private void selectOrUnselectAttackTarget()
    {
        if (Input.GetMouseButtonUp(1) && !Input.GetMouseButton(0))
        {
            if (!looking && !usingRune && attachedItem == null)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, LayerMask.GetMask("Monster", "Player")))
                {
                    if (targetAttack == null)
                    {
                        targetAttack = new TargetAttack(hit.collider.gameObject);
                    }
                    else if (targetAttack.equals(hit.collider.gameObject))
                    {
                        targetAttack.stopAttacking();
                    }
                    else
                    {
                        targetAttack = new TargetAttack(hit.collider.gameObject);
                    }
                }
            }
        }
    }

    private void setLooking()
    {
        if (!usingRune && (Input.GetMouseButton(0) && Input.GetMouseButtonDown(1)) || (Input.GetMouseButton(1) && Input.GetMouseButtonDown(0)))
        {
            looking = true;
        }
    }

    private void lookAt()
    {
        if (((Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1)) || (Input.GetMouseButtonUp(1) && !Input.GetMouseButton(0))) && looking)
        {
            GameObject thing = Utility.raycastThatDetectsUI();

            if (!Utility.canvasClicked() && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                Debug.Log(hit.collider.name);
            }
            else if (thing.tag == "DetectEQSlot")
            {
                GameObject eqSlot = thing;

                if (equipment[getNumberFromEqSlot(eqSlot)] != null)
                {
                    Debug.Log(equipment[getNumberFromEqSlot(eqSlot)].name);
                }
            }
            else if (thing.tag == "Container")
            {
                int index = containers.IndexOf(thing.GetComponent<Container>());
                GameObject item = containers[index].get(thing.name);
                //asdasdasdasd uwaga
                if (item != null)
                {
                    Debug.Log(item.name);
                }
            }
            looking = false;
        }
    }

    private int getNumberFromEqSlot(GameObject slot)
    {
        return (int)((equipmentSlotNumber)Enum.Parse(typeof(equipmentSlotNumber), slot.name));
    }

    private void manageItems()
    {
        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            attachItem();
        }

        if (attachedItem != null && looking)
        {
            detachItem();
        }

        if (Input.GetMouseButtonUp(0) && attachedItem != null)
        {
            if (!Utility.canvasClicked())
            {
                throwItemOnGround();
            }
            else
            {
                GameObject equipmentSlot = Utility.raycastThatDetectsUI();

                if (equipmentSlot != null && equipmentSlot.tag == "DetectEQSlot")
                {
                    if (equipment[getNumberFromEqSlot(equipmentSlot)] == null)
                    {
                        equipItem(equipmentSlot);
                    }
                    else
                    {
                        if (itemCanBeInThisSlot(equipmentSlot, attachedItem))
                        {
                            if (!attachedItem.activeSelf)
                            {
                                if (itemCanBeInThisSlot(lastClickedEqSlot, equipment[getNumberFromEqSlot(equipmentSlot)]))
                                {
                                    swapItemsInEQ(equipmentSlot);
                                }
                            }
                            else
                            {
                                swapItemFromEqWithItemOnTheGround(equipmentSlot);
                            }
                        }
                    }
                }
                else if (equipmentSlot.tag == "Container")
                {                   
                    //if (!bp.contains(attachedItem))
                    //{
                    //    bp.put(attachedItem);
                    //    attachedItem.SetActive(false);
                    //}
                }
            }
            detachItem();
        }
    }

    private void attachItem()
    {
        if (!looking && !usingRune)
        {
            if (!Utility.canvasClicked())
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, LayerMask.GetMask("Item")))
                {
                    attachedItem = hit.collider.gameObject;
                }
            }
            else
            {
                GameObject equipmentSlot = Utility.raycastThatDetectsUI();

                if (equipmentSlot != null)
                {
                    if (equipmentSlot.tag == "DetectEQSlot")
                    {
                        attachedItem = equipment[getNumberFromEqSlot(equipmentSlot)];
                        lastClickedEqSlot = equipmentSlot;
                    }
                    else if (equipmentSlot.tag == "Container")
                    {
                        //GameObject item = bp.get(equipmentSlot.name);
                        //if (item != null)
                        //{
                        //    attachedItem = item;
                        //    lastClickedEqSlot = equipmentSlot;
                        //}
                    }
                }
            }
        }
    }

    private void detachItem()
    {
        attachedItem = null;
    }

    private void throwItemOnGround()
    {
        Vector3 mousePosition = Utility.convertMousePositionToVector3("Terrain");
        if (mousePosition != Vector3.zero)
        {
            if (attachedItem.activeSelf == false)
            {
                if (lastClickedEqSlot.name.Split(' ')[0] != "cell")
                {
                    equipment[getNumberFromEqSlot(lastClickedEqSlot)] = null;
                    lastClickedEqSlot.GetComponent<Image>().color = Color.red;
                }
                else
                {
                    //bp.remove(int.Parse(lastClickedEqSlot.name.Split(' ')[1]));
                }                
                attachedItem.SetActive(true);
            }
            attachedItem.transform.position = mousePosition;
        }
    }

    private void equipItem(GameObject equipmentSlot)
    {
        if (itemCanBeInThisSlot(equipmentSlot, attachedItem))
        {
            if (attachedItem.activeSelf == false)
            {
                lastClickedEqSlot.GetComponent<Image>().color = Color.red;
                equipment[getNumberFromEqSlot(lastClickedEqSlot)] = null;
            }
            equipment[getNumberFromEqSlot(equipmentSlot)] = attachedItem;
            equipment[getNumberFromEqSlot(equipmentSlot)].SetActive(false);
            equipmentSlot.GetComponent<Image>().color = Color.green;
        }
    }

    private bool itemCanBeInThisSlot(GameObject equipmentSlot, GameObject item)
    {
        if(item.tag == "Container")
        {
            if(equipmentSlot.name == "Container")
            {
                return true;
            }
            return false;
        }
        else if (equipmentSlot.name == item.tag ||
            equipmentSlot.name == "LeftHand" || equipmentSlot.name == "RightHand" || equipmentSlot.name == "Random" || equipmentSlot.name == "Arrow" ||
            ((equipmentSlot.name == "Ring1" || equipmentSlot.name == "Ring2") && item.tag == "Ring"))
        {
            return true;
        }
        return false;
    }

    private void swapItemsInEQ(GameObject equipmentSlot)
    {
        equipment[getNumberFromEqSlot(lastClickedEqSlot)] = equipment[getNumberFromEqSlot(equipmentSlot)];
        equipment[getNumberFromEqSlot(equipmentSlot)] = attachedItem;
    }

    private void swapItemFromEqWithItemOnTheGround(GameObject equipmentSlot)
    {
        equipment[getNumberFromEqSlot(equipmentSlot)].transform.position = attachedItem.transform.position;
        equipment[getNumberFromEqSlot(equipmentSlot)].SetActive(true);
        equipment[getNumberFromEqSlot(equipmentSlot)] = attachedItem;
        equipment[getNumberFromEqSlot(equipmentSlot)].SetActive(false);
    }

    private void useRune()
    {
        if (Input.GetMouseButtonUp(1)) {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, LayerMask.GetMask("Item")))
            {
                if (hit.collider.tag == "Rune")
                {
                    usingRune = true;
                    rune = hit.collider.gameObject;
                }
            }
        }
        if (usingRune && Input.GetMouseButtonUp(0))
        {
            rune.GetComponent<Rune>().use(this);
            usingRune = false;           
        }//to zrobic
        //if (usingRune && Input.GetMouseButtonUp(1))
        //{
        //    usingRune = false;
        //}
    }

    [PunRPC]
    public void createMagicWall(Vector3 pos)
    {        
        Instantiate(magicWall, pos, new Quaternion());
    }
    
    private void openBackPack()
    {
        if (Input.GetMouseButtonUp(1))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, LayerMask.GetMask("Item")))
            {
                if (hit.collider.tag == "Container")
                {
                    Container container = hit.collider.gameObject.GetComponent<Container>();
                    if (!containers.Contains(container))
                    {
                        containers.Add(container);
                        container.open();
                    }
                    else
                    {
                        containers.Remove(container);
                        container.close();
                    }
                }
            }
        }
    }

    [PunRPC]
    public void changeNick(string newName)
    {
        nick = newName;
        nickCanvas.text = newName;
    }
}