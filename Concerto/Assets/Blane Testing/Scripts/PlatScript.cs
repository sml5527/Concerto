﻿using UnityEngine;
using System.Collections;

public class PlatScript : MonoBehaviour {

    //Public Variables
    public float bottomRaycastLength, enemyRaycastLength;
    public float jumpForce;
    public float movementForce;
    public float frictionCoff;
    public float decelMultiVal;
    public Vector2 maxClampSpeed;
    public Vector2 minClampSpeed;
    public Vector2 raycastOffset;
    public bool secondJump;
    public bool tryingToFall = false;
    public int score;
    public EnemyComboScript1 currentEnemy;

    //Private Variables
    private LayerMask platformLayerMask, enemyLayerMask;
    private Rigidbody2D rig;
    private BoxCollider2D colliderBox;
    private Rect box;
    private Vector2 bottomRaycastOrigin;
    

    //Structs
    public struct Box
    {
        public float minX, maxX, minY, maxY;

        public void setBounds(float smallX, float bigX, float smallY, float bigY)
        {
            minX = smallX;
            maxX = bigX;
            minY = smallY;
            maxY = bigY;
        }
    }


    // Use this for initialization
    void Start () {
        //Layermask for platforms, used for raycasting
        platformLayerMask = LayerMask.GetMask("Platform");
        enemyLayerMask = LayerMask.GetMask("Enemy");
        rig = GetComponent<Rigidbody2D>();
        colliderBox = GetComponent<BoxCollider2D>();
    }
	
    void FixedUpdate()
    {
        //Raycast to use platforms
        bottomRaycastOrigin = new Vector2(transform.position.x, transform.position.y) - raycastOffset;

        //Using raycasts and layermasks we can do basic collisions for platforms relatively quickly
        RaycastHit2D rayHit = Physics2D.Raycast(bottomRaycastOrigin, Vector2.down, bottomRaycastLength, platformLayerMask.value);
        if (rayHit.collider != null)
        {
            //Physics Forces on platform

            //Normal Force
            Vector2 normalForce = new Vector2(0f, (9.8f * (rig.gravityScale * rig.mass)));
            if (!tryingToFall)
                rig.AddForce(normalForce);

            //Friction Force
            if (Mathf.Abs(rig.velocity.x) > minClampSpeed.x)
            {
                Vector2 frictionForce = new Vector2(normalForce.y * frictionCoff, 0f);
                if (rig.velocity.x < 0f)
                    rig.AddForce(frictionForce);
                else
                    rig.AddForce(-frictionForce);
            }
        }

        //Natural Decel
        if (Input.GetAxis("Horizontal") == 0)
        {
            rig.velocity = new Vector2(rig.velocity.x * decelMultiVal, rig.velocity.y);
        }

        //Keep forces from being obscene
        ClampSpeeds();

    }

	// Update is called once per frame
	void Update () {

        box = new Rect(colliderBox.bounds.min.x, colliderBox.bounds.min.y, colliderBox.bounds.size.x, colliderBox.bounds.size.y);

        //Raycast to use platforms //Working
        bottomRaycastOrigin = new Vector2(transform.position.x, transform.position.y) - raycastOffset;

        //Using raycasts and layermasks we can do basic collisions for platforms relatively quickly //Working
        RaycastHit2D rayHit = Physics2D.Raycast(bottomRaycastOrigin, Vector2.down, bottomRaycastLength, platformLayerMask.value);
        if (rayHit.collider != null)
        {
            //Show the object raycast is returning
            Debug.DrawLine(bottomRaycastOrigin, rayHit.collider.transform.position, Color.red);
            if (Input.GetButtonDown("Jump"))
            {
                //Chaos CONTROOOLS!!!
                rig.AddForce(new Vector2(0f, jumpForce));
                secondJump = true;
            }

            //Stop the object from falling through the platform //Working so long as collision is detected
            if (!tryingToFall && rig.velocity.y <= 0f)
            {
                //Compensate for force built up in air
                rig.velocity = new Vector2(rig.velocity.x, 0f);
            }
        }
        else
        {
            //Jump in midair
            if (Input.GetButtonDown("Jump") && secondJump)
            {
                rig.velocity = new Vector2(rig.velocity.x, 0f);
                rig.AddForce(new Vector2(0f, jumpForce));
                secondJump = false;
            }
        }

        //Chaos CONTROOOLS!!!
        if (Input.GetAxis("Horizontal") > 0)
        {
            //Instant stop
           // if(rig.velocity.x < 0f)
           //     rig.velocity = new Vector2(0f, rig.velocity.y);
            rig.AddForce(new Vector2(movementForce, 0f));
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            //Instant stop
           // if (rig.velocity.x > 0f)
           //     rig.velocity = new Vector2(0f, rig.velocity.y);
            rig.AddForce(new Vector2(-movementForce, 0f));
        }

        if (Input.GetAxis("Vertical") < 0)
            tryingToFall = true;
        else
            tryingToFall = false;

        //Attack

        //Raycast to enemy
        rayHit = Physics2D.Raycast(new Vector2(transform.position.x + box.size.x/2, transform.position.y), Vector2.right, enemyRaycastLength, enemyLayerMask.value);
        //Debug.DrawRay(new Vector2(transform.position.x + box.size.x / 2, transform.position.y), Vector2.right * enemyRaycastLength, Color.blue);

        if (BeatManager.instance.onTime && rayHit.collider != null && (Input.GetButtonDown("XButton") || Input.GetButtonDown("YButton") || Input.GetButtonDown("BButton")) && BeatManager.instance.onTime)
        {
            currentEnemy = rayHit.transform.gameObject.GetComponent<EnemyComboScript1>();
            Debug.DrawRay(new Vector2(transform.position.x + box.size.x / 2, transform.position.y), Vector2.right * enemyRaycastLength, Color.yellow);
            enemyLogic();
        }
    }

    //#JelloPunchesBack
    //Keep forces in check with equal and opposite
    void ClampSpeeds()
    {
        //check for speeds
        Vector2 currentSpeed = rig.velocity;
        Vector2 addForceVector = new Vector2(0, 0);

        if (Mathf.Abs(currentSpeed.x) > maxClampSpeed.x)
        {
            if (currentSpeed.x > 0f)
                addForceVector.x -= (currentSpeed.x - maxClampSpeed.x);
            else
                addForceVector.x -= (currentSpeed.x + maxClampSpeed.x);
        }

        if (Mathf.Abs(currentSpeed.y) > maxClampSpeed.y)
        {
            if (currentSpeed.y > 0f)
                addForceVector.y -= (currentSpeed.y - maxClampSpeed.y);
            else
                addForceVector.y -= (currentSpeed.y + maxClampSpeed.y);
        }

        //stop is small enough
        if (Mathf.Abs(currentSpeed.x) < minClampSpeed.x)
            rig.velocity = new Vector2(0f, rig.velocity.y);

        //find the opposite force
        Vector2 accelForce = new Vector2(rig.mass * (addForceVector.x / Time.fixedDeltaTime), rig.mass * (addForceVector.y / Time.fixedDeltaTime));

        rig.AddForce(accelForce);
    }

    void enemyLogic()
    {
        if (currentEnemy != null)
        {
            //See if there is combat input
            //If multiple inputs, send garbage Input to reset queue due to messup
            //If Battle input is there, Check current Battle Input against enemy combo
            char input = '\0';

            if(Input.GetButtonDown("XButton"))
            {
                input = 'X';
            }
            if (Input.GetButtonDown("YButton"))
            {
                if (input == '\0')
                    input = 'Y';
                else
                    input = 'F';
            }
            if (Input.GetButtonDown("BButton"))
            {
                if (input == '\0')
                    input = 'B';
                else
                    input = 'F';
            }

            if(currentEnemy.checkInput(input))
            {
                score += 1;
                currentEnemy = null;
            } 
        }
    }
}