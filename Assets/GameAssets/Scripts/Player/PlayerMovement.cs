using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement active;
    //Variables
    [SerializeField]
    private bool canNoclip;
    [SerializeField]
    public Transform orientation;
    [Header("Walking")]
    [SerializeField]
    private float walkingSpeed = 1f;
    [SerializeField]
    private float sprintingSpeed = 1f;
    [SerializeField]
    private float crouchingSpeed = 1f;
    [SerializeField]
    private float climbingSpeed = 1f;
    [SerializeField]
    private float climbingAngleThreshold = 45f;
    [SerializeField]
    private float noclipSpeed = 1f;
    [SerializeField]
    private float acceleration = 1f;
    [SerializeField]
    private float deceleration = 1f;
    [SerializeField]
    private float maxAngle = 45f;
    [SerializeField]
    private CapsuleCollider playerCollider;
    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private float colliderCrouchHeight;
    [SerializeField]
    private float cameraCrouchHeight;
    [Header("Max Distance From Objects")]
    [SerializeField]
    private float maxDistancefromTrain;
    [SerializeField]
    private bool maxDistanceLimitEnabled;
    [Header("Stamina")] 
    [SerializeField] 
    private float maxStamina;
    [SerializeField] 
    private float staminaGainDelay;
    [SerializeField] 
    private float staminaGain;
    [SerializeField] 
    private float staminaLoss;
    [Header("Jumping")]
    [SerializeField]
    private float jumpForce = 1f;
    [SerializeField]
    private float jumpCoolDown = 0.25f;
    [SerializeField] 
    private LayerMask groundLayer;
    [SerializeField]
    private float gravityForce = 1f;
    [Header("Step Detection")]
    [SerializeField]
    private float stepCheckLow;
    [SerializeField]
    private float stepCheckHigh;
    [SerializeField]
    private float stepCheckDistance;
    [Header("FootSteps")]
    [SerializeField]
    private float footStepDistance = 1f;
    [Header("Water")]
    [SerializeField]
    private float waterHeight;
    [SerializeField]
    private float waterSlowDownMult;
    //Runtime References
    [NonSerialized]
    public Rigidbody rb;
    private float colliderStandingHeight;
    private float cameraStandingHeight;
    
    //Player States
    [NonSerialized]
    public bool grounded = true;
    [NonSerialized]
    public bool crouched = false;
    private float jumpTimer = 0f;

    private float groundAngle;
    private Vector3 groundNormal = Vector3.up;

    private float targetSpeed;
    
    private float distanceWalked;

    private float stamina;
    private float staminaGainDelayTime;

    private bool tooFarFromTrain;

    private Ladder currentLadder;
    //Input
    private Vector2 moveInput;
    private bool noclip;
    private void Awake()
    {
        active = this;
    }

    private void Start()
    {
        
        rb = GetComponent<Rigidbody>();

        colliderStandingHeight = playerCollider.height;
        cameraStandingHeight = cameraTransform.localPosition.y;
    }
    private void Update()
    {
        if (InputManager.debugCamAction.triggered && canNoclip)
        {
            if(noclip)
                DisableNoclip();
            else
                EnableNoclip();
        }
        
        moveInput = InputManager.active.moveInput;
        if (noclip)
        {
            //Noclip
            Vector3 movementVector = PlayerCamera.active.transform.forward * moveInput.y +
                                     PlayerCamera.active.transform.right * moveInput.x;
            movementVector = movementVector.normalized * (noclipSpeed * Time.deltaTime);

            if (InputManager.sprintAction.IsPressed())
                movementVector *= 3f;
            rb.MovePosition(transform.position + movementVector);
        }
        else
        {
            //Normal Movement
            
            //Crouching 
            if (crouched == false && InputManager.crouchAction.IsPressed())
            {
                crouched = true;
                playerCollider.height = colliderCrouchHeight;
                playerCollider.center = new Vector3(0, playerCollider.height * 0.5f, 0);
                cameraTransform.localPosition = new Vector3(0, cameraCrouchHeight, 0);

                //if (grounded == false)
                //rb.position += new Vector3(0, cameraStandingHeight - cameraCrouchHeight, 0);
            }
            else if (crouched && InputManager.crouchAction.IsPressed() == false)
            {
                crouched = false;
                playerCollider.height = colliderStandingHeight;
                playerCollider.center = new Vector3(0, playerCollider.height * 0.5f, 0);
                cameraTransform.localPosition = new Vector3(0, cameraStandingHeight, 0);

                //if (grounded == false)
                //rb.position -= new Vector3(0, cameraStandingHeight - cameraCrouchHeight, 0);
            }

            //Setting target speed
            targetSpeed = (InputManager.sprintAction.IsPressed() && stamina > 0f) ? sprintingSpeed : walkingSpeed;
            if (crouched)
                targetSpeed = crouchingSpeed;

            //Calculating Stamina
            if (InputManager.sprintAction.IsPressed() == false)
            {
                //Not Sprinting
                staminaGainDelayTime += Time.deltaTime;
                if (staminaGainDelayTime >= staminaGainDelay)
                {
                    stamina += staminaGain * Time.deltaTime;
                    stamina = Mathf.Clamp(stamina, 0, maxStamina);
                }
            }
            else if(moveInput != Vector2.zero)
            {
                //Sprinting
                staminaGainDelayTime = 0;
                stamina -= staminaLoss * Time.deltaTime;
                stamina = Mathf.Clamp(stamina, 0, maxStamina);
            }
            StatUI.active.UpdateStamina(Mathf.FloorToInt(stamina),maxStamina);
            
            //Debug.Log("TargetSpeed: "+targetSpeed);
            CheckGrounded();

            //Jumping
            jumpTimer -= Time.deltaTime;
            if (InputManager.jumpAction.IsPressed() && grounded && jumpTimer <= 0f)
            {
                float force = jumpForce;
                if (IsInWater())
                    force *= (1 - GetPercentUnderWater() * waterSlowDownMult);

                rb.AddForce(0f, crouched ? force * 0.5f : force, 0f);
                grounded = false;
                jumpTimer = jumpCoolDown;
                groundNormal = Vector3.up;

                if(IsInWater())
                    SoundManager.active.PlayAtPos(transform.position, "Water - Impact");
                else
                    SoundManager.active.PlayAtPos(transform.position, "Jump");
            }

            //Distance walked statistic
            float distKM = (new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude * Time.deltaTime) / 1000f;
            GameStateManager.AddToStatistic("Distance Walked", distKM, GameStateManager.StatisticType.DistanceKilometers);

            //FootStep Sound
            if (grounded == false)
                return;

            distanceWalked += rb.linearVelocity.magnitude * Time.deltaTime;
            if (distanceWalked > footStepDistance)
            {
                distanceWalked -= footStepDistance;
                if(IsInWater())
                    SoundManager.active.PlayAtPos(transform.position, "FootStep - Water");
                else
                    SoundManager.active.PlayAtPos(transform.position, "FootStep - Stone");
            }
        }
    }

    private void FixedUpdate()
    {
        if(noclip)
            return;
       

        if (currentLadder != null) 
        {
            //Ladder
            targetSpeed = climbingSpeed;



            Vector3 forceVector = Vector3.zero;
            Vector3 ladderDir = currentLadder.GetLadderDir();
            void CheckIfVectorIsClimbing(Vector3 vector)
            {
                if (vector == Vector3.zero) return;

                float angle = Vector2.Angle(new Vector2(vector.x, vector.z), new Vector2(ladderDir.x, ladderDir.z));
                if(angle < climbingAngleThreshold)
                {
                    forceVector += new Vector3(0f, vector.y + 0.3f, 0f); // +0.3f bias to climbing upwards
                }
                else
                {
                    forceVector += vector;
                }
            }
            CheckIfVectorIsClimbing(PlayerCamera.active.GetDir() * moveInput.y);
            CheckIfVectorIsClimbing(orientation.right * moveInput.x);

            forceVector = forceVector.normalized;

            rb.AddForce(forceVector * ((targetSpeed / walkingSpeed) * acceleration));

            //Limit speed
            float velocity = rb.linearVelocity.magnitude;
            if (velocity > targetSpeed && moveInput != Vector2.zero)
            {
                Vector3 n = rb.linearVelocity.normalized * targetSpeed;
                rb.linearVelocity = new Vector3(n.x, n.y, n.z);
            }

            //Drag
            if (moveInput == Vector2.zero && velocity > 0.1f)
            {
                rb.AddForce(-rb.linearVelocity.normalized * ((targetSpeed / walkingSpeed) * deceleration));
            }
            else if (moveInput == Vector2.zero)
            {
                rb.linearVelocity = new Vector3(0, 0, 0);
            }
        }
        else
        {
            //Gravity
            rb.AddForce(grounded ? -groundNormal * gravityForce * 0.15f : Vector3.down * gravityForce);

            //Movement
            Vector2 mag = FindVelRelativeToLook();
            float xMag = mag.x, zMag = mag.y;

            Debug.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + new Vector3(xMag, 0, zMag), Color.cyan);
            Debug.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + rb.linearVelocity, Color.green);

            if (moveInput.x > 0 && xMag > targetSpeed) moveInput.x = 0;
            if (moveInput.x < 0 && xMag < -targetSpeed) moveInput.x = 0;
            if (moveInput.y > 0 && zMag > targetSpeed) moveInput.y = 0;
            if (moveInput.y < 0 && zMag < -targetSpeed) moveInput.y = 0;

            Vector3 targetVector = ((orientation.forward * moveInput.y) + (orientation.right * moveInput.x)).normalized;
            Vector3 projectedVector = Vector3.ProjectOnPlane(targetVector, groundNormal).normalized;

            Debug.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + projectedVector * acceleration, grounded ? Color.beige : Color.red);

            //Water slow down
            if (IsInWater())
                targetSpeed *= (1 - GetPercentUnderWater() * waterSlowDownMult);



            //Max Distance 
            if (maxDistanceLimitEnabled && Train.playerTrain != null && GameStateManager.gameStarted)
            {
                DistanceWaypoint closestWaypoint = null;
                float distanceToWaypoint = float.MaxValue;
                foreach (DistanceWaypoint waypoint in GameStateManager.waypointList)
                {
                    float d = Vector3.Distance(transform.position, waypoint.transform.position);
                    if(d < distanceToWaypoint)
                    {
                        closestWaypoint = waypoint;
                        distanceToWaypoint = d;
                    }
                }

                float trainDist = Train.playerTrain.GetClosestDistanceToPos(transform.position);

                if (closestWaypoint != null && trainDist > distanceToWaypoint)
                {

                    if (distanceToWaypoint > closestWaypoint.maxDistance && Vector3.Distance(transform.position + projectedVector, closestWaypoint.transform.position) > distanceToWaypoint)
                    {
                        Debug.Log("Walking away from a waypoint!");
                        targetSpeed = 0f;
                        if (tooFarFromTrain == false)
                        {
                            Override title = new Override("Title", OverrideType.Text, "WARNING");
                            Override message = new Override("Message", OverrideType.Text, "You're to far from the structure! You may NOT go FURTHER!");
                            Override subText = new Override("SubText", OverrideType.Text, Mathf.Round(distanceToWaypoint) + "m");
                            MiniPrinter.active.AddNotification(PaperRenderer.active.RenderPaper("Message", new List<Override>() { title, message, subText }));
                        }
                        tooFarFromTrain = true;
                    }
                    else if (distanceToWaypoint < closestWaypoint.maxDistance - 2f)
                        tooFarFromTrain = false;
                }
                else
                {
                    if (trainDist >= maxDistancefromTrain && Train.playerTrain.GetClosestDistanceToPos(transform.position + projectedVector) > trainDist)
                    {
                        Debug.Log("Walking away from the train!");
                        targetSpeed = 0f;
                        if (tooFarFromTrain == false)
                        {
                            Override title = new Override("Title", OverrideType.Text, "WARNING");
                            Override message = new Override("Message", OverrideType.Text, "You're to far from the train! You may NOT go FURTHER!");
                            Override subText = new Override("SubText", OverrideType.Text, Mathf.Round(trainDist) + "m");
                            MiniPrinter.active.AddNotification(PaperRenderer.active.RenderPaper("Message", new List<Override>() { title, message, subText }));
                        }
                        tooFarFromTrain = true;
                    }
                    else if (trainDist < maxDistancefromTrain - 2f)
                        tooFarFromTrain = false;
                }
            }

            rb.AddForce(projectedVector * ((targetSpeed / walkingSpeed) * acceleration));

            if (grounded)
            {
                //Drag
                Vector3 velocityPlane = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                float velocity = velocityPlane.sqrMagnitude;
                if (moveInput == Vector2.zero && velocity > 0.1f)
                {
                    rb.AddForce(-velocityPlane.normalized * ((targetSpeed / walkingSpeed) * deceleration));
                }
                else if (moveInput == Vector2.zero)
                {
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                }

                //Limit speed
                float angleBetweenDirections = Vector2.Angle(new Vector2(rb.linearVelocity.x, rb.linearVelocity.z), new Vector2(targetVector.x, targetVector.z));
                if (velocity > targetSpeed && moveInput != Vector2.zero && angleBetweenDirections <= 90f)
                {
                    float fallSpeed = rb.linearVelocity.y;
                    Vector3 n = rb.linearVelocity.normalized * targetSpeed;
                    rb.linearVelocity = new Vector3(n.x, fallSpeed, n.z);
                }

                //Counter movement
                if (moveInput != Vector2.zero && angleBetweenDirections > 90f)
                {
                    rb.AddForce(projectedVector * ((targetSpeed / walkingSpeed) * acceleration));
                }
            }

            //Step detection
            if ((moveInput.x > 0 || moveInput.y > 0) && rb.linearVelocity.y < 0.1f)
            {
                if (Physics.Raycast(transform.position + new Vector3(0, stepCheckHigh, 0), projectedVector, stepCheckDistance, groundLayer) == false)
                {
                    if (Physics.Raycast(transform.position + new Vector3(0, stepCheckLow, 0), projectedVector, out RaycastHit hit, stepCheckDistance, groundLayer))
                    {
                        transform.position += projectedVector * hit.distance + new Vector3(0, stepCheckHigh, 0);
                        Debug.Log("Preformed step!");
                    }
                }
                Debug.DrawRay(transform.position + new Vector3(0, stepCheckLow, 0), projectedVector.normalized * stepCheckDistance, Color.red);
                Debug.DrawRay(transform.position + new Vector3(0, stepCheckHigh, 0), projectedVector.normalized * stepCheckDistance, Color.red);
            }
        }
    }
    
    private bool IsInWater()
    {
        return transform.position.y < waterHeight;
    }
    public bool IsSubmergedInWater()
    {
        if (IsInWater())
            return GetPercentUnderWater() >= 1f;
        return false;
    }
    private float GetPercentUnderWater()
    {
        float precentUnderWater = Mathf.Abs(transform.position.y - waterHeight) / playerCollider.height;
        return Mathf.Clamp01(precentUnderWater);     
    }
    private Vector2 FindVelRelativeToLook() {
        //Not my code. From Dani
        float lookAngle = orientation.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.linearVelocity.x, rb.linearVelocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitude = rb.linearVelocity.magnitude;
        float yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);
        
        return new Vector2(xMag, yMag);
    }

    private void CheckGrounded()
    {
        if (rb.constraints == RigidbodyConstraints.FreezeAll) return;

        if(currentLadder != null)
        {
            grounded = false;
            return;
        }

        Debug.DrawRay(transform.position + new Vector3(0, playerCollider.radius, 0),Vector3.down* playerCollider.radius * 2f);
        if (Physics.SphereCast(transform.position + new Vector3(0, playerCollider.radius, 0), playerCollider.radius - 0.01f, Vector3.down, out RaycastHit  hit , playerCollider.radius * 1.5f, groundLayer))
        {
            groundNormal = hit.normal;
            groundAngle = Vector3.Angle(Vector3.up, groundNormal);

            if (groundAngle <= maxAngle)
            {
                if (grounded == false && rb.linearVelocity.y < -0.25f)
                {
                    if (IsInWater())
                        SoundManager.active.PlayAtPos(transform.position, "Water - Impact");
                    else
                        SoundManager.active.PlayAtPos(transform.position, "Land");
                }
                grounded = true;
                
                if(hit.transform.CompareTag("Moving"))
                    MovingPlatformManager.active.AddEntry(rb,orientation,hit.transform,true);
                else
                    MovingPlatformManager.active.RemoveEntry(transform);
            }
            else
            {
                grounded = false;
                groundNormal = Vector3.up;
                MovingPlatformManager.active.RemoveEntry(transform);
            }
        }
        else
        {
            grounded = false;
            groundNormal = Vector3.up;
            MovingPlatformManager.active.RemoveEntry(transform);
        }
    }

    public void Freeze(RailCar lockToRailCar = null)
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;

        if (lockToRailCar != null)
        {
            MovingPlatformManager.active.AddEntry(rb, orientation, lockToRailCar.transform, true);
        }
    }
    public void UnFreeze()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        MovingPlatformManager.active.RemoveEntry(transform);
    }
    public void EnableNoclip()
    {
        Freeze();
        MovingPlatformManager.active.RemoveEntry(transform);
        playerCollider.enabled = false;
        noclip = true;
    }
    public void DisableNoclip()
    {
        UnFreeze();
        playerCollider.enabled = true;
        noclip = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(currentLadder == null && other.transform.TryGetComponent(out Ladder ladder))
        {
            currentLadder = ladder;
            MovingPlatformManager.active.RemoveEntry(transform);
            MovingPlatformManager.active.AddEntry(rb, orientation, currentLadder.ladderCollider.transform, true);
            grounded = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (currentLadder != null && other.transform == currentLadder.ladderCollider.transform)
        {
            currentLadder = null;
            MovingPlatformManager.active.RemoveEntry(transform);
        }
    }
}
