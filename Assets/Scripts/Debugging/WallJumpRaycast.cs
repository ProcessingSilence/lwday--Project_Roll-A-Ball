using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class WallJumpRaycast : MonoBehaviour
{
    // Explanation through image: https://i.imgur.com/K2wOdsg.png
    
    public Transform target;
    public GameObject PlayerObj;
    private Transform Player;
    private Rigidbody PlayerRb;
    private RaycastHit hit;

    // 0-1 Gets xy axis of wall to determine if it's walljumpable or not (-85 to -105 acceptable)   
        // 0-1 Are both placed .1 apart on the y-axis
    // 2-3 determine the angle of the wall based on xz axis to determines direction the ball will jump off of the wall.
        // This only gets calculated if 0-1 are calculated successfully.
        // 2 and 3 are shot infinitely outward since whether the program runs depends on 0 and 1 touching a wall.
        // 2-3 Are both placed .1 apart on the x-axis
    // If a wall is no longer detected, the program gets cut-off and all variable values are reset to 0.
    private Vector3[] raycastOrigins = new Vector3[4];
    public RaycastHit[] raycastHits = new RaycastHit[4];
    public Vector3[] hitLocations = new Vector3[4];
    
    public float horizontalRay;
    public float verticalRay;

    public float raycastDist;
    public LayerMask LayersToDetect;
    public LayerMask LayersToDetect2;    
    public PlayerController my_PlayerController_script;
    public float calculatedAngle;
    public float wallJumpAngle;

    // Get the current direction the player is facing in. If a new input is made, it gets put into newDirection and gets compared
    // to currentDirection. If they are different, then currentDirection gets replaced by newDirection.
    public Vector2 currentDirection;
    public Vector2 newDirection;
    public bool isGrounded;
    public int groundedAmt;
    public Vector3 finalLocation;
    public MainGameManager PauseGameThing;
    private bool useForce;
    public float distanceBetweenPoints;
    public float radiusAmount;

    // Prevents ball from wall-jumping sideways when one of the two orientations is not pressed.
    private bool dontMoveSideways;

    // Has the ball's gravity reduced when touching a wall that's within the angle range, connects to BetterJump.cs
    public bool slideAgainstWall;

    private bool waitBeforeWallJump;

    private AudioSource AudioS;
    public AudioClip WallJumpSound;

    //Indicates if player is touching wall from its intensity.
    public Light lightIndicator;
    void Awake()
    {
        Player = PlayerObj.GetComponent<Transform>();
        PlayerRb = PlayerObj.GetComponent<Rigidbody>();
        AudioS = Player.GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {


    }

    private void Update()
    {
        // Destroy gameObject if player isn't active to shut error log up.
        if (!PlayerObj)
        {
            Destroy(gameObject);
        }
        
        isGrounded = my_PlayerController_script.IsGrounded();

        if (Player)
        {
            if (isGrounded == false)
            {
                // Get the movement direction of the player, and convert it to the nearest numbers of -1, 0, or 1
                horizontalRay = raycastDirection(my_PlayerController_script.moveHorizontal);
                verticalRay = raycastDirection(my_PlayerController_script.moveVertical);
                

                // Current direction is placed in a Vector2 so that the facing direction of the raycast will remain in place
                // instead of moving back to (0,0) when the player isn't giving any input. It only goes back to (0,0) when
                // the player is grounded.
                newDirection = new Vector2(horizontalRay, verticalRay);
                if (newDirection != currentDirection && newDirection != new Vector2(0,0))
                    currentDirection = newDirection;

                /*
                // Have the y-direction go opposite when the control direction is diagonal so that the player will move forward when diagonally wall-jumping.
                if (currentDirection.x != 0 && currentDirection.y != 0)
                {
                    currentDirection.y = -currentDirection.y;
                }
                */

                // Raycast locations are set to player position plus/minus an offset.
                // See variable instantiation for info.
                raycastOrigins[0] = raycastOrigins[1] = raycastOrigins[2] = raycastOrigins[3] = Player.position;
                raycastOrigins[0].y += 0.05f;
                raycastOrigins[1].y -= 0.05f;
                raycastOrigins[2].x += 0.05f;            
                raycastOrigins[3].x -= 0.05f;
                
                /*
                Debug.DrawRay(raycastOrigins[0], (-transform.right * currentDirection.x) + -transform.forward * currentDirection.y, Color.green);
                Debug.DrawRay(raycastOrigins[1], (-transform.right * currentDirection.x) + -transform.forward * currentDirection.y, Color.red);
                Debug.DrawRay(raycastOrigins[2], (-transform.right * currentDirection.x) + -transform.forward * currentDirection.y, Color.cyan);
                Debug.DrawRay(raycastOrigins[3], (-transform.right * currentDirection.x) + -transform.forward * currentDirection.y, Color.yellow);
                */                   
            }

            // Detect wall to tell BetterJump script that player is sliding down wall.
            if (Physics.Raycast(Player.position,
                (-transform.right * currentDirection.x) + -transform.forward * currentDirection.y, raycastDist,
                LayersToDetect) && !isGrounded)
            {
                slideAgainstWall = true;
                //lightIndicator.color = new Color(250,69,45);
                lightIndicator.intensity = 5;
            }
            else
            {
                slideAgainstWall = false;
                //lightIndicator.color = new Color(255,255,255);
                lightIndicator.intensity = 0;
            }
            
            // Calculations only get made when space is pressed down.
            if (Input.GetKeyDown(KeyCode.Space) && waitBeforeWallJump == false)
            {
                // Both 0 and 1 have to detect an object before the calculations can be made.
                if (Physics.Raycast(raycastOrigins[0], (-transform.right * currentDirection.x) + -transform.forward * currentDirection.y, out raycastHits[0], raycastDist, LayersToDetect) &&
                    Physics.Raycast(raycastOrigins[1], (-transform.right * currentDirection.x) + -transform.forward * currentDirection.y, out raycastHits[1], raycastDist, LayersToDetect))
                {
                    // Get the Vector3 locations of the hit points
                    hitLocations[0] = raycastHits[0].point;
                    hitLocations[1] = raycastHits[1].point;
                    Debug.Log("raycastHits[0]: " + hitLocations[0]);
                    Debug.Log("raycastHits[1]: " + hitLocations[1]);
                    
                    // Get the angle based on the xy locations from point 0 to 1 within a radius. 0 is the origin in the radius, 1 being the on the edge of the radius.
                    var VectorDir = new Vector3(hitLocations[1].x - hitLocations[0].x, hitLocations[1].y - hitLocations[0].y,0);
                    calculatedAngle = Mathf.Atan2(VectorDir.y, VectorDir.x) * Mathf.Rad2Deg;

                    if (calculatedAngle <= -85 && calculatedAngle >= -105)
                    {
                        // Stop sliding against wall when wall-jumpable wall is confirmed.
                        slideAgainstWall = false;
                        lightIndicator.intensity = 1;
                        Debug.Log("Allowed to walljump");
                        // Get the angle of the wall based on the x and z axis to determine the direction the player will fly off of the wall.
                        // Raycast is made 10x longer so the detection of the wall will remain consistent.
                        Physics.Raycast(raycastOrigins[2], (-transform.right * currentDirection.x) +
                            -transform.forward * currentDirection.y, 
                            out raycastHits[2], raycastDist*10, LayersToDetect);
                        Physics.Raycast(raycastOrigins[3],(-transform.right * currentDirection.x) +
                            -transform.forward * currentDirection.y,  
                            out raycastHits[3], raycastDist*10, LayersToDetect);           
                    
                        // Set the hits to Vector3 rayCastHits variables.
                        hitLocations[2] = raycastHits[2].point;
                        hitLocations[3] = raycastHits[3].point;
                        
                        VectorDir = new Vector3(hitLocations[3].x - hitLocations[2].x, 0,hitLocations[3].z - hitLocations[2].z);
                        finalLocation = VectorDir;
                        wallJumpAngle = Mathf.Atan2(VectorDir.x, VectorDir.z) * Mathf.Rad2Deg;
                        //PlayerRb.AddForce(transform.forward * wallJumpAngle);
                        finalLocation = Vector3.Lerp(hitLocations[2], hitLocations[3], 0.5f);
                        
                        // Use a formula that gets the distance between the point and a circle to get the distance from the outside point
                        // to the radius: https://www.varsitytutors.com/hotmath/hotmath_help/topics/shortest-distance-between-a-point-and-a-circle
                        distanceBetweenPoints = Vector3.Distance(Player.position, finalLocation);
                        var radiusDistance = Mathf.Sqrt(Mathf.Pow((-Player.position.x - finalLocation.x), 2) +
                                                        Mathf.Pow((-Player.position.z - finalLocation.z), 2) - radiusAmount);
                        if (radiusDistance < 0)
                            radiusDistance = -radiusDistance;
                        // Get what percent the distance between the point and radius is for the whole distance.
                        var subtraction = (distanceBetweenPoints - (distanceBetweenPoints - radiusAmount)) / distanceBetweenPoints;
                        
                        // Lerp the given percent distance from origin to the vector.
                        // The point should move towards the point on the ramp while remaining in the given radius.
                        var tempPos = Vector3.Lerp(Player.position, finalLocation, subtraction);
                        Debug.Log(subtraction);
                        finalLocation = tempPos;
                        useForce = true;
                        my_PlayerController_script.wallJumpLocation = finalLocation;
                        my_PlayerController_script.allowWallJump = 1;
                        my_PlayerController_script.verticalAngle = calculatedAngle;
                        my_PlayerController_script.horizontalAngle = wallJumpAngle;
                        AudioS.clip = WallJumpSound;
                        AudioS.Play();
                        Debug.Log(my_PlayerController_script.wallJumpLocation);
                        //waitBeforeWallJump = true;
                        //StartCoroutine(WallJumpWaitEnum());

                        //var tempPos = Vector3.Lerp(Player.position, finalLocation, finalPercentage(distanceBetweenPoints, radiusAmount));

                        //Debug.Log(finalPercentage(distanceBetweenPoints, radiusAmount));
                        //finalLocation = tempPos;
                        //Physics.Linecast(finalLocation, Player.position,  out var hitLocation, LayerMask.GetMask("Player"));
                        //finalLocation = hitLocation.point;
                        //PauseGameThing.isPaused = true;
                    }
                }    
            }

            // Reset all values when grounded
            if (isGrounded)
            {
                slideAgainstWall = false;
                groundedAmt++;
                //Debug.Log("GROUNDED");
                currentDirection = new Vector2(0,0);
                newDirection = new Vector2(0,0);
                calculatedAngle = wallJumpAngle = 0;
                hitLocations[0] = Vector3.zero;
                hitLocations[1] = Vector3.zero;
                hitLocations[2] = Vector3.zero;
                hitLocations[3] = Vector3.zero;
                dontMoveSideways = false;
            }
        }

        
       
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Debug.DrawRay(finalLocation, Player.position - finalLocation, Color.yellow);
        if (useForce)
        {
            useForce = false;
            Player.LookAt(finalLocation);
            PlayerRb.AddForce(transform.forward* 50);
            //finalLocation.y = .5f;
            //PlayerRb.velocity -= finalLocation * 2;

        }

        if (Player)
        {
            transform.position = Player.position;
            // Only look at the target's xz axis.
            Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.LookAt(targetPosition);
            //transform.rotation = Quaternion.Euler(0, transform.rotation.y ,0);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(finalLocation, .1f);
        Gizmos.DrawSphere(Player.position, .1f);
        
    }

    // Has the raycast direction only determined by the numbers -1, 0, and 1 instead of using precise decimals from Input.GetAxis.
    float raycastDirection(float givenInput)
    {
        if (givenInput > 0)
        {
            return 1;
        }
        else if (givenInput < 0)
        {
            return -1;
        }
        else
            return 0;
    }

    IEnumerator WallJumpWaitEnum()
    {
        yield return new WaitForSeconds(0.1f);
        waitBeforeWallJump = false;
    }

    /*
    float finalPercentage(float distance, float radius)
    {
        var tempPerc =  1 - ((distance / radius) * 0.1f);
        if (tempPerc < 0)
            tempPerc = -tempPerc;
        return tempPerc;
    }
    */
}
