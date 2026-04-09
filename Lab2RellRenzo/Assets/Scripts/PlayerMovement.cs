using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // Razne varijable za odredivanje kretanja
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Stamina")]
    public Image staminaBar;
    public float currentStamina;
    public float maxStamina;
    public float sprintCost;
    public float jumpCost;
    public float chargeRate;
    private Coroutine recharge;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
    
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;
    
    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    // Stanje pokretanja lika
    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // Provjera da se potvrdi da smo na zemlji
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        
        MyInput();
        SpeedControl();
        StateHandler();

        // Provjera za trenje
        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }

        // Sprint zaustavlja regen, a svi ostali stateovi ga mogu pokrenuti
        if (state == MovementState.sprinting)
        {
            // Sprint trosi staminu kontinuirano po sekundi
            currentStamina -= sprintCost * Time.deltaTime;

            if (recharge != null)
            {
                StopCoroutine(recharge);
                recharge = null;
            }
        }
        else if (currentStamina < maxStamina && recharge == null)
        {
            recharge = StartCoroutine(StaminaRegenDelay());
        }

        StaminaRegen();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Provjera svih elemenata kada lik skoci i pokretanje jump cooldowna
        
        // Jump trosi fiksni stamina cost jednom po pritisku tipke
        if(Input.GetKeyDown(jumpKey) && readyToJump && grounded && currentStamina >= jumpCost)
        {
            readyToJump = false;
            
            Jump();
            
            Invoke(nameof(ResetJump), jumpCooldown);

            // Trigger za oduzimanje stamine tijekom skoka
            currentStamina -= jumpCost;
            staminaBar.fillAmount = currentStamina / maxStamina;
        }

        // Provjera croucha i trigger za cucanj lika
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

            // Rjesenje problema kad cucne like da ne lebdi u zraku model, jer se skupi 
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // Provjera jesmo li pustili crouch botun
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        }
    }

    private void StateHandler()
    {
        // Mode za cucanj
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        // Mode za spritanje 
        else if (grounded && Input.GetKey(sprintKey) && currentStamina > 0f)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        // Mode za setanje
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        // Mode za u zraku
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        // Odredivanje smjera kretanja lika 
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Na kosini
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            // Da je lik zaljepljen za kosinu, jer skakuce i poleti bez ovoga
            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // Odredivanje smjera na zemlji
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        // U zraku
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // Iskljucivanje gravitacije dok smo na kosini da ne klizimo nizbrdo
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // Ogranicavanje brzine na kosini, bez toga smo malo brzi na kosini
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }

        // Ogranicavanje brzine na zemlji i u zraku
        else
        {
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // Ogranicenje ubrzanja lika, bez toga se ubrza iznad zadane brzine
            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                rb.linearVelocity =  new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            }    
        }
    }

    private void Jump()
    {
        // Omogucuje skakanje na kosini
        exitingSlope = true;

        // Resetiranje Y ubrzanja
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }
    
    // Funkcija za regeneraciju stamine
    private void StaminaRegen()
    {
        if (currentStamina < 0)
        {
            currentStamina = 0;
        }
        else if (currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }
        staminaBar.fillAmount = currentStamina / maxStamina;
    }

    // Funkcija za upravljanje staminom
    private IEnumerator StaminaRegenDelay()
    {
        yield return new WaitForSeconds(1f);

        while (currentStamina < maxStamina)
        {
            currentStamina += chargeRate / 10f;

            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }

            staminaBar.fillAmount = currentStamina / maxStamina;

            yield return new WaitForSeconds(0.1f);
        }

        recharge = null;
    }

    // Provjera jesmo li na kosini
    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    // Provjera jesmo li na uzbrdici ili nizbrdici
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
