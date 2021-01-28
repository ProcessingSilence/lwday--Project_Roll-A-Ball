using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    // *** MOVEMENT ***

    public float maxMovement = 25.0f;
    public float moveHorizontal;
    public float moveVertical;
    public GameObject rotatingObject;
    
    [Header("Jumping")]
    // *** JUMPING ***
    public float jumpHeight = 25.0f;    
    // Get the south direction of where to put grounded raycast
    private Vector3 South;
    private float tempY;
    public float raycastDist = 3f;
    private float originalRaycastDist;
    // Has the player wait 0.1 seconds before being able to jmup again (prevent jump spamming exploit). 
    // 0 = inactive, 1
    public int jumpWait;
    private float originalJumpHeight;
    private float getJumpHeight;  
    private bool isHoldingJump;
    private RaycastHit hit;
    public LayerMask ignoreMe;


    [Header("Wall Jumping")]
    // *** WALL JUMPING ***
    public GameObject wallJumpDetector;
    private float originalMaxSpeed;
    public Vector3 wallJumpLocation;
    public float wallJumpForce = 25;
    public int allowWallJump;
    public float verticalAngle;
    public float horizontalAngle;
    // Determines if the player is allowed to walljump based on WallJumpRaycast detections.
    // Gets activated from WallJumpRaycast script;
    //public bool AllowedToWalljump; 
    
    [Header("Running")]
    public float iteratingAdd;
    private bool isRunning;
    
    [Header("Health")]
    // *** HEALTH ***      
        public int maxHealth = 3;
        public int currentHealth;     
        // Amount of damage taken on damage touch
        public int damageAmt = 1;

    [Header("Win/Lose Conditions")]
    // *** WIN/LOSE CONDITIONS ***
        private int wonGame;
        public bool isDead;
        private GameObject sceneManagerObject;
        private MainGameManager myMainGameManagerScript;
        public KillWhenFallen my_KillWhenFallen_script; 
        // Determines if player gets facked over by a tornado.
        // 0 = inactive, 1 = activate death IEnumerator, 2 = Force chaotic tornado movement upon player
        private int tornadoFacked;
        
    [Header("Cosmetic Effects")]
    // *** COSMETIC EFFECTS ***
        public Material DarkShadow;
        public Material LightShadow;
        public Projector shadowProjector;     
        private AudioSource my_Audio;     
        [SerializeField] public AudioClip[] soundEffects;
        private bool playLandingSound = true;   
        private GameObject canvas;
        // Public bool that determines whether to show health bar or not.
        public bool spawnHealthHUD = true;   
        // [1]
        // Instantiate heatlh bar and set it to healthHUD,
        // set its position to (133,0),
        // get its script and set to my_HealthBar_script,
        // set PlayerController script to my_PlayerController_script in Healthbar so values can be passed back and forth between scripts,
        // set it as child to the Canvas (find Canvas via "Canvas" tag),
        // send connect confirmation bool to HeathBar script
        // recieve connect confirmation bool from HealthBar script;
        public GameObject healthHUD;
        private HealthBar my_HealthBar_script;  
        public GameObject deathExplosion;
        
    
    [Header("Debugging")]
    // *** DEBUGGING ***    
        public GameObject detectedGroundedObject;
        public bool connect; // See [1] above.
        // Leaves wall jump detector on, even if the player is grounded.
        //public bool alwaysTurnOnWallJumpDetector;
    
    
    // *** NECESSITIES ***
        private Rigidbody rb;
    
    void Awake()
    {
        if (spawnHealthHUD)
        {
            // See [1] above for info about below:
            healthHUD = Instantiate(healthHUD);
            my_HealthBar_script = healthHUD.GetComponent<HealthBar>();
            my_HealthBar_script.playerObj = gameObject;
            canvas = GameObject.FindGameObjectWithTag("Canvas");
            healthHUD.transform.parent = canvas.transform;
            my_HealthBar_script.connect = true;
            healthHUD.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,74, 0);
            healthHUD.GetComponent<RectTransform>().localScale = new Vector3(0.25f, 0.16f, 0.16f);
            if (connect)
            {
                Debug.Log("Healthbar connected to PlayerController");
            }

            originalRaycastDist = raycastDist;
        }
        
        rb = GetComponent<Rigidbody>();
        South = new Vector3(0, -1, 0);
        sceneManagerObject = GameObject.FindGameObjectWithTag("SceneManager");
        myMainGameManagerScript = sceneManagerObject.GetComponent<MainGameManager>();
        currentHealth = maxHealth;
        my_Audio = gameObject.GetComponent<AudioSource>();
        originalJumpHeight = jumpHeight;
        originalRaycastDist = raycastDist;
        //if (alwaysTurnOnWallJumpDetector == false) wallJumpDetector.SetActive(false);
        originalMaxSpeed = maxMovement;
        iteratingAdd = (2 / (originalMaxSpeed *3)) * (originalMaxSpeed *3);
    }


    void FixedUpdate()
    {
        if (isRunning && maxMovement < originalMaxSpeed * 3f) 
        {
            maxMovement+= iteratingAdd;
        }
        if (!isRunning && maxMovement > originalMaxSpeed) 
        {
            maxMovement-= iteratingAdd;
        }

        if (maxMovement > originalMaxSpeed * 3f)
        {
            maxMovement = originalMaxSpeed * 3f;
        }

        if (maxMovement < originalMaxSpeed)
            maxMovement = originalMaxSpeed;

        // Set pauseGame's sceneNum to 2 (resets the scene), kill player with isDead bool.
        if (currentHealth <= 0)
        {
            myMainGameManagerScript.sceneNum = 2;           
            Instantiate(deathExplosion, new Vector3 (transform.position.x,transform.position.y,transform.position.z), transform.rotation);
            if (spawnHealthHUD)
            {
                my_HealthBar_script.slider.value = 0;
            }
            isDead = true;
        }  

    
        //  Start the coroutine and set tornadoFacked int to 2 which sends the player flying upwards while violently shaking.
        if (tornadoFacked == 1)
        {
            tornadoFacked = 2;
            StartCoroutine(TornadoDeath());
        }

        // Send player flying upwards while violently shaking in random directions.
        if (tornadoFacked == 2)
        {
            rb.AddForce(Random.Range(-200f, 200f)* Time.deltaTime,200* Time.deltaTime,Random.Range(-200f, 200f)* Time.deltaTime ) ;
        }
/*
        if (rb.velocity.y >= originalJumpHeight - originalJumpHeight/1.5f && Input.GetKey(KeyCode.Space) && isHoldingJump == false)
        {
            isHoldingJump = true;
            StartCoroutine(HoldingJump());
        }
*/


        // Get control input from player, it determines the movement of the player's xz velocity.
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal*50, 0.0f, moveVertical*50);


        if (allowWallJump == 0)
        {
            // Limit the velocity when the player is not being controlled in any direction.
            // If the player is touching object tagged "Ramp", the player will not slow down and remain slippery.
            if (moveHorizontal == 0 && moveVertical == 0 && IsGrounded() && !RampCheck())
            {
                var tempY = rb.velocity.y;
                rb.velocity = rbVelocity(rb.velocity, 2)* Time.deltaTime;
                rb.velocity = new Vector3(rb.velocity.x, tempY, rb.velocity.z);
            }
        }


        if (allowWallJump == 0) 
        {       
            // The position of the camera determines where the force will move the ball in the x and z axis
            Vector3 relativeMovement = rotatingObject.transform.TransformVector(movement);
            relativeMovement.y = 0;

            var tempYagain = rb.velocity.y;
            rb.velocity = (relativeMovement * (maxMovement * 5)* Time.deltaTime);
            rb.velocity = new Vector3(rb.velocity.x, tempYagain, rb.velocity.z);
            // Prevent ball from constantly sliding
            if (GetComponent<Rigidbody>().velocity.magnitude > maxMovement)
            {
                rb.velocity = rbVelocity(rb.velocity, maxMovement);
            }
        }


        

        
        // Set wonGame to 2 so if statement doesn't repeat
        if (wonGame == 1)
        {
            wonGame = 2;
            myMainGameManagerScript.enablePausing = 3;
            Destroy(gameObject);
        }
        
        // Destroy player after receiving "isDead" bool
        if (isDead)
        {
            my_KillWhenFallen_script.my_Audio.Play();
            Destroy(gameObject);
        }        
    }


    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        // Check grounded raycast, and jump if ground is detected.
        // Also start coroutine to check if player is holding down space.
        if (Input.GetKeyDown("space") && IsGrounded() && jumpWait == 0)
        {        
            jumpWait = 1;
            rb.velocity += new Vector3(0,jumpHeight, 0);
            my_Audio.clip = soundEffects[3];
            my_Audio.Play();
        }
        
        if (jumpWait == 1)
        {
            jumpWait = 2;
            StartCoroutine(waitBeforeJump());
        }
        
        // Shadow is light when grounded, becomes dark when ungrounded (is that even a word?).
        if (IsGrounded())
        {
            shadowProjector.material = LightShadow;
            jumpHeight = originalJumpHeight;
            getJumpHeight = 0;
            wallJumpDetector.SetActive(false);
            //if (wallJumpDetector.active && alwaysTurnOnWallJumpDetector == false) wallJumpDetector.SetActive(false);
        }
        else
        {
            shadowProjector.material = DarkShadow;
            wallJumpDetector.SetActive(true);
            // Make the grounded raycast shorter in-air so that jumping can't be spammed to float.
            //raycastDist = 2f;
            //if (wallJumpDetector.active == false) wallJumpDetector.SetActive(true);
        }

        if (IsGrounded() && playLandingSound)
        {
            playLandingSound = false;
            my_Audio.clip = soundEffects[4];
            my_Audio.Play();
            raycastDist = originalRaycastDist;
        }

        // Get rotation based on given input to determine rotation of wall-jumping raycasts.
        //raycastRotation = new Vector3( 0, Mathf.Atan2( Input.GetAxis("Vertical"), Input.GetAxis("Horizontal")) * 180 / Mathf.PI, 0 );
        
        if (IsGrounded() == false && playLandingSound == false)
        {
            playLandingSound = true;
        }
    }

    private void LateUpdate()
    {
        if (allowWallJump == 1)
        {
            allowWallJump = 2;
            rb.velocity = Vector3.zero;
            StartCoroutine(wallJumpResult());
        }

        if (allowWallJump == 4)
        {
            transform.LookAt(wallJumpLocation);
            rb.velocity =(-transform.forward* 50);
            rb.velocity = new Vector3(rb.velocity.x, 30, rb.velocity.z);
            //rb.AddForce(transform.up* 70);
        }
    }

    // Get raycast position based on South position (-1 y)
   public bool IsGrounded() 
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, South, out hit, raycastDist, ignoreMe))
        {
            detectedGroundedObject = hit.transform.gameObject;
        }

        return Physics.Raycast(transform.position, South, raycastDist, ignoreMe);
    }


    private void OnDrawGizmosSelected()
    {
        Debug.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y , transform.position.z), Color.blue, raycastDist);
    }

    // Get grounded raycast, and then send an endless raycast downward detecting tag of object below.
    // If object is tagged as "Ramp", return true.
    bool RampCheck() 
    {
        if (IsGrounded())
        {
            if (Physics.Raycast(transform.position, new Vector3(transform.position.x, transform.position.y +1, transform.position.z), out hit, ignoreMe) && hit.transform.tag == "Ramp")
            {
                //Debug.Log("RAMP DETECTED.");
                return true;
            }
            return false;
        }
        return false;
    }



    void OnCollisionEnter(Collision _collision)
    {
        //player takes damage here
        if (_collision.gameObject.tag == "Enemy")
        {
            Debug.Log("Touched damager");
            // Choose and play random hurt sound effect from 1 to 3.
            var damageSound = Random.Range(1,4);
            switch (damageSound)
            {
                case 1:
                {
                    my_Audio.clip = soundEffects[0];
                    break;
                }
                case 2:
                {
                    my_Audio.clip = soundEffects[1];
                    break;
                }
                case 3:
                {
                    my_Audio.clip = soundEffects[2];
                    break;
                }
            }
            my_Audio.Play();
            
            currentHealth -= damageAmt;
            // Send bool signal to myHealthBar script to subtract health.
            my_HealthBar_script.takeDamage = true;
        }
        
        // Start tornadoFacked coroutine procedure after player touches tornado hitbox.
        if (_collision.gameObject.tag == "Tornado")
        {
            tornadoFacked = 1;
        }
        
        // Kill the player instantly when colliding with object tagged "InstaKill"
        if (_collision.gameObject.tag == "InstaKill")
        {
            //my_KillWhenFallen_script.my_Audio.Play();
            currentHealth = 0;
        }
    }

    // Wait 4 seconds before killing off the player.
    IEnumerator TornadoDeath()
    {
        yield return new WaitForSeconds(4);
        currentHealth = 0;
    }

    // Limit speed on x and z positions        
    // Get the y velocity in a temp value, limit all the velocities, and then place the temp y velocity back into the rb y velocity slot.
    Vector3 rbVelocity(Vector3 rb, float speedLimit)
    {
        var tempVel = rb.y;            
        rb.y = 0.0f;
        rb = Vector3.ClampMagnitude(GetComponent<Rigidbody>().velocity, speedLimit);
        rb.y = tempVel;
        return rb;
        //GetComponent<Rigidbody>().velocity = rb;
    }

    IEnumerator waitBeforeJump()
    {
        yield return new WaitForSeconds(0.1f);
        jumpWait = 0;
    }

    IEnumerator wallJumpResult()
    {      

        //rb.AddForceAtPosition();
        maxMovement = originalMaxSpeed;
        allowWallJump = 4;
        yield return new WaitForSeconds(0.1f);
        allowWallJump = 0;
    }
}