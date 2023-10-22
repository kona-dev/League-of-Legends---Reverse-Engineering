using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;

using System.Runtime.CompilerServices;
using System.Transactions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    public enum Ability
    {
        NONE,
        Q,
        W,
        E,
        R
    }

    [SerializeField] public string side;

    private Ability currentAbility;

    [SerializeField] GameObject pointerPrefab;
    [SerializeField] GameObject targetPrefab;

    [SerializeField] GameObject autoPrefab;
    [SerializeField] GameObject qPrefab;
    [SerializeField] GameObject wPrefab;
    [SerializeField] GameObject ePrefab;

    // RANGE IS A UNITY UNIT MEASUREMENT IN VECTOR MAGNITUDE
    [SerializeField] public float attackRange;
    
    // ATTACK SPEED FUNCTION IS f(x) = 1sec / attackSpeed;
    [SerializeField] public float attackSpeed;

    // MOVEMENT SPEED IS FLAT SPEED TRANSLATED TO NAV AGENT
    [SerializeField] public float baseMovementSpeed;
    private float movementSpeed;

    LineRenderer lineRenderer;


    public float attackTimer;
    private bool canAttack;
    private bool isTargetingEnemy;
    private GameObject targetEnemy;

    public GameObject rTarget;

    private bool canMove;

    private bool isSelecting;

    private bool isChanneling;
    private float channelTimer;
    private float channelTime;

    [SerializeField] GameObject indicator;
    private Vector3 mousePos;
    private Quaternion dir;
    private Vector3 tempPlacement;

    
    [SerializeField] private float qCooldown;
    [SerializeField] private float wCooldown; 
    [SerializeField] private float eCooldown;
    [SerializeField] private float rCooldown;


    private float wCharges;

    AudioManager audioManager;

    // FOR CAIT

    private int headshotStacks;
    private bool headshot;

    // TEST UI
    [SerializeField] TMP_Text qCDText;
    [SerializeField] TMP_Text wCDText;
    [SerializeField] TMP_Text eCDText;
    [SerializeField] TMP_Text rCDText;
    [SerializeField] TMP_Text passiveText;

    [SerializeField] Slider channelSlider;


    private Vector3 testPosition;

    void Start()
    {
        attackTimer = 0;
        canAttack = true;
        canMove = true;
        movementSpeed = baseMovementSpeed;
        headshotStacks = 0;
        indicator.SetActive(false);
        this.gameObject.tag = side;
        headshot = false;

        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    void Update()
    {


        // UPDATE TEST UI
        qCDText.text = "Q - " +  Mathf.Round(qCooldown * 10.0f) / 10.0f; 
        wCDText.text = "W - " +  Mathf.Round(wCooldown * 10.0f) / 10.0f;
        eCDText.text = "E - " +  Mathf.Round(eCooldown * 10.0f) / 10.0f; 
        rCDText.text = "R - " +  Mathf.Round(rCooldown * 10.0f) / 10.0f;

        passiveText.text = "Passive " + headshotStacks + "/5";


        if (!canMove) { movementSpeed = 0; }
        else movementSpeed = baseMovementSpeed;


        if (movementSpeed != GetComponent<NavMeshAgent>().speed) GetComponent<NavMeshAgent>().speed = movementSpeed;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        if (Physics.Raycast(ray, out hitData, 1000, 1 << 6));
        {
            indicator.transform.LookAt(hitData.point);
            mousePos = hitData.point;

            Debug.DrawLine(gameObject.transform.position, mousePos);
            Debug.DrawRay(gameObject.transform.position, -(mousePos - gameObject.transform.position));
        }

        TestDummy test;
        float activeAttackRange = attackRange;
        if (targetEnemy) if (targetEnemy.gameObject.TryGetComponent<TestDummy>(out test))
        {
            if (test.netted || test.trapped) activeAttackRange = activeAttackRange * 1.5f;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {

            Debug.Log(mousePos - gameObject.transform.position);
            if (Physics.Raycast(ray, out hitData, 1000))
            {
                if (hitData.transform.gameObject.tag == "Ground")
                {
                    GameObject.Instantiate(pointerPrefab, hitData.point, pointerPrefab.transform.rotation);
                    
                    if (!isChanneling) GetComponent<NavMeshAgent>().destination = hitData.point;
                    if (!isChanneling) GetComponent<NavMeshAgent>().stoppingDistance = 0; 
                    isTargetingEnemy = false;
                    if (!isChanneling) gameObject.transform.LookAt(hitData.point + new Vector3(0, 0.5f, 0));
                }
                else if (hitData.transform.gameObject.tag != side)
                {
                    if (!isChanneling) GetComponent<NavMeshAgent>().destination = hitData.transform.gameObject.transform.Find("Floor").transform.position;
                    if (!isChanneling) GetComponent<NavMeshAgent>().stoppingDistance = activeAttackRange;
                    GameObject.Instantiate(targetPrefab, hitData.transform.gameObject.transform.Find("Top").transform.position, targetPrefab.transform.rotation);
                    isTargetingEnemy = true;
                    targetEnemy = hitData.transform.gameObject;
                    if (!isChanneling) gameObject.transform.LookAt(hitData.transform.gameObject.transform.Find("Floor").transform.position + new Vector3(0, 0.5f, 0));
                } 

                
            }

            
        }
        
       
        
        

            if (isTargetingEnemy && (attackTimer <= 0) && canAttack && (Vector3.Distance(this.gameObject.transform.position, targetEnemy.transform.position) <= activeAttackRange))
        {
           // Debug.Log("FIRE");
            attackTimer = 1f / attackSpeed;
            // TODO Crit, Passive stuff, onhit?

            GameObject tempAuto = GameObject.Instantiate(autoPrefab, this.transform.position, this.transform.rotation);
            tempAuto.GetComponent<Projectile>().target = targetEnemy;

            // PASSIVE CALC HERE



            if (targetEnemy.gameObject.TryGetComponent<TestDummy>(out test))
            {
                if (test.trapped)
                {
                    headshot = true;
                    test.trappedTimer = 0;
                }
                else if (test.netted)
                {
                    headshot = true;
                    test.nettedTimer = 0;
                }

            }

            if (!headshot)
            {
                 if (headshotStacks < 5)
                 {
               
                    headshotStacks++;
                 }
                 else if (headshotStacks == 5)
                 {
               
                    headshotStacks = 0;
                    // apply headshot;
                    headshot = true;
                
                
                    }

            }

            if (headshot)
            {
                
                tempAuto.gameObject.transform.localScale = Vector3.one * 2;
                headshot = false;
                audioManager.PlayUnique("Headshot");
            }
            else
            {
                audioManager.PlayUnique("Shoot");
            }

        }
        else if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Q) && qCooldown <= 0 && !isChanneling)
        {
            isSelecting = true;
            currentAbility = Ability.Q;
            indicator.SetActive(true);
        }

         
        if (Input.GetKeyDown(KeyCode.W) && wCooldown <= 0 && !isChanneling)
        {
           
            isSelecting = true;
            currentAbility = Ability.W;
        }

        if (Input.GetKeyDown(KeyCode.E) && eCooldown <= 0 && !isChanneling)
        {
            isSelecting = true;
            currentAbility = Ability.E;
            indicator.SetActive(true);
            

        }

        if (Input.GetKeyDown(KeyCode.R) && rCooldown <= 0 && !isChanneling)
        {
            isSelecting = true;
            currentAbility = Ability.R;
            
        }

        if (isSelecting)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                dir = indicator.transform.rotation;
                indicator.SetActive(false);
                switch (currentAbility)
                {
                    case Ability.Q:
                        audioManager.PlayUnique("QChannel");
                        isChanneling = true;
                        channelTimer = 0.5f;
                        canAttack = false;
                        channelTime = 0.5f;
                        isSelecting = false;
                        break;
                    case Ability.W:
                        isChanneling = true;
                        channelTimer = 0.25f;
                        canAttack = false;
                        channelTime = 0.25f;
                        isSelecting = false;
                        tempPlacement = mousePos;
                        break;

                    case Ability.E:
                        isChanneling = true;
                        channelTimer = 0.35f;
                        canAttack = false;
                        channelTime = 0.35f;
                        isSelecting = false;
                        tempPlacement = mousePos;
                        GameObject temp = GameObject.Instantiate(ePrefab, gameObject.transform.position, dir);
                        audioManager.PlayUnique("EShot");
                        temp.GetComponent<E>().side = side;
                        temp.GetComponent<E>().host = this;

                        Debug.Log(tempPlacement);
                        Debug.Log(-tempPlacement);
                        testPosition = gameObject.transform.localPosition;
                        break;
                    case Ability.R:
                       
                       
                        Ray rRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit rHitData;
                        if (Physics.Raycast(rRay, out rHitData, 1000))
                        {
                            Debug.Log(rHitData.transform.gameObject.tag);
                            if (rHitData.transform.gameObject.tag != side && rHitData.transform.gameObject.tag != "Ground")
                            {
                                rTarget = rHitData.transform.gameObject;
                                isChanneling = true;
                                channelTimer = 1.3f;
                                canAttack = false;
                                channelTime = 1.3f;
                                isSelecting = false;
                                audioManager.Play("RCharge");
                                lineRenderer.enabled = true;
                            }


                        }
                        break;

                }
                
            }
           
        }
       
        if (isChanneling)
        {
            canMove = false;
            if (channelTimer > 0) 
            { 
                channelSlider.enabled = true; channelTimer -= Time.deltaTime; channelSlider.value = (channelTime - channelTimer) / channelTime; 
                if (currentAbility == Ability.E) { transform.position += -(tempPlacement - gameObject.transform.position).normalized * Time.deltaTime * 15; }
                //Debug.DrawRay(testPosition, -(tempPlacement - gameObject.transform.position), Color.red);
                Debug.DrawLine(testPosition, tempPlacement, Color.blue);
                if (currentAbility == Ability.R) 
                {
                   
                    

                    //Set width
                    lineRenderer.startWidth = 0.1f;
                    lineRenderer.endWidth = 0.1f;

                    //Set line count which is 2
                    lineRenderer.positionCount = 2;

                    //Set the postion of both two lines
                    lineRenderer.SetPosition(0, gameObject.transform.position);
                    lineRenderer.SetPosition(1, rTarget.transform.position);
                }
            }
            else
            {
                channelSlider.enabled = false;
                channelSlider.value = 0;
                isChanneling = false;
                canMove = true;

                switch (currentAbility)
                {
                    case Ability.Q:
                        GameObject.Instantiate(qPrefab, gameObject.transform.position, dir);
                        audioManager.PlayUnique("QShot");
                        currentAbility = Ability.NONE;
                        qCooldown = 3;
                        attackTimer = 0.2f;
                        canAttack = true;
                        
                        break;
                    case Ability.W:
                        GameObject temp = GameObject.Instantiate(wPrefab, tempPlacement, wPrefab.transform.rotation);
                        temp.GetComponent<Trap>().side = side;
                        temp.GetComponent<Trap>().host = this;
                        audioManager.PlayUnique("TrapPrime");
                        canAttack = true;
                        wCooldown = 3;
                        break;  

                    case Ability.E:
                        canAttack = true;
                        eCooldown = 3;
                        break;

                    case Ability.R:
                        {
                            GameObject tempR = GameObject.Instantiate(autoPrefab, gameObject.transform.position, gameObject.transform.rotation);
                            tempR.GetComponent<Projectile>().target = rTarget;
                            tempR.GetComponent<Projectile>().speed = 30;
                            audioManager.PlayUnique("RShot");
                            tempR.gameObject.transform.localScale = new Vector3(2, 2, 4);
                            lineRenderer.enabled = false;
                            canAttack = true;
                            rCooldown = 3;
                        }
                        break;
                        
                }

            }


        }

        if (qCooldown > 0) qCooldown -= Time.deltaTime;
        if (wCooldown > 0) wCooldown -= Time.deltaTime;
        if (eCooldown > 0) eCooldown -= Time.deltaTime;
        if (rCooldown > 0) rCooldown -= Time.deltaTime;

        if (qCooldown < 0) qCooldown = 0;
        if (wCooldown < 0) wCooldown = 0;
        if (eCooldown < 0) eCooldown = 0;
        if (rCooldown < 0) rCooldown = 0;
    }


    
}
