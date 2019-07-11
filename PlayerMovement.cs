using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool isDebugMode = false;
    private CharacterController cc;
    private Vector3 PlayerVelocity = Vector3.zero;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    [Range(0,10)]
    public float TurnSensitivity = 10.0f;
    private float HorizontalLookInput = 0.0f;
    private float VerticalLookInput = 0.0f;

    [Header("Movement Settings")]
    [Range(-2,2)]
    public float gravity = -9.8f;
    [Range(0,50)]
    public float jumpForce = 100;
    [Range(0,100)]
    public float maxSpeed = 10;
    [Range(0,100)]
    public float acceleration = 6;
    [Range(0,100)]
    public float groundFriction = 6;
    [Range(0,100)]
    public float airFriction = 0;
    private bool wishJump = false;
    private bool didJump = false;


    //Booleans used to track if player just walked off a ledge
    private bool didWalkOffLedge = false;
    private bool PreviousGroundedState = false;
    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        PreviousGroundedState = cc.isGrounded;
    }

    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        MovePlayer();
        
    }

    private void MovePlayer(){
        

        Vector3 input = GetMovementInput();
        Vector3 wishMovement = transform.TransformDirection(input);

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump")) && cc.isGrounded)
        {
            wishJump = true; //prevents deceleration right before a jump which can be super annoying
            didJump = true;
        }

        
        if(PreviousGroundedState == true && cc.isGrounded == false)
        {
            Debug.Log("walked off ledge");
            didWalkOffLedge = true;
        }

        ApplyFriction(input);
        if(input != Vector3.zero)
        {
            ApplyAcceleration(wishMovement);
        }
        ApplyGravity();

        ApplyClamp();

        //must be located before the cc.Move or else it bugs the fuck out
        PreviousGroundedState = cc.isGrounded;

        cc.Move(PlayerVelocity * Time.deltaTime);
        PlayerVelocity = cc.velocity;

        
    }

    private void MoveCamera()
    {
        VerticalLookInput -= Input.GetAxis("Mouse Y") * TurnSensitivity;
        HorizontalLookInput += Input.GetAxis("Mouse X") * TurnSensitivity;

        VerticalLookInput = Mathf.Clamp(VerticalLookInput, -90, 90);

        cameraTransform.localEulerAngles = new Vector3(VerticalLookInput, 0, 0);
        transform.localEulerAngles = new Vector3(0, HorizontalLookInput, 0);

    }

    public Vector3 GetMovementInput()
    {
        var forwardInput = Input.GetAxisRaw("Horizontal");
        var rightInput = Input.GetAxisRaw("Vertical");

        return new Vector3(forwardInput, 0, rightInput);
    }

    public void ApplyAcceleration(Vector3 wishMove)
    {
        /*
        Debug.Log(inputVec);

        Vector3 move = normalizedVec * acceleration * Time.deltaTime;
        PlayerVelocity += move;
        PlayerVelocity.x = Mathf.Clamp(PlayerVelocity.x, -maxSpeed * Mathf.Abs(inputVec.x), maxSpeed * Mathf.Abs(inputVec.x));
        PlayerVelocity.y = Mathf.Clamp(PlayerVelocity.y, -maxSpeed * Mathf.Abs(inputVec.y), maxSpeed * Mathf.Abs(inputVec.y));
        */

        wishMove = wishMove * acceleration * Time.deltaTime;
        wishMove.y = 0;
        PlayerVelocity += wishMove;
    }

    public void ApplyFriction(Vector3 input)
    {
        float friction;
        if(cc.isGrounded)
        {
            friction = groundFriction;
        }
        else
        {
            friction = airFriction;
        }

        Vector3 inversePlayerVelocity = -(PlayerVelocity.normalized) * friction * Time.deltaTime;
        if (PlayerVelocity.magnitude > inversePlayerVelocity.magnitude)
        {
            PlayerVelocity += inversePlayerVelocity;
        }
        else
        {
            PlayerVelocity.x = 0;
            PlayerVelocity.z = 0;
        }
    }

    void ApplyGravity()
    {
        if (wishJump)
        {
            PlayerVelocity.y = jumpForce;
            wishJump = false;
        }
        else if (cc.isGrounded)
        {
            PlayerVelocity.y = -cc.stepOffset / Time.deltaTime;
            didJump = false;

        }
        else if(didWalkOffLedge && !didJump)
        {
            PlayerVelocity.y = 0;
            didWalkOffLedge = false;
        }
        else
        {
            PlayerVelocity.y += gravity;
        }
    }

    private void ApplyClamp()
    {
        float yAxis = PlayerVelocity.y;
        PlayerVelocity.y = 0; //dont want to include y in our clamping
        PlayerVelocity = Vector3.ClampMagnitude(PlayerVelocity, maxSpeed);
        
        PlayerVelocity.y = yAxis;
    }

    private void OnGUI()
    {
        if (isDebugMode)
        {
            GUI.Label(new Rect(10, 10, 200, 20), "Movement: " + cc.velocity);
            GUI.Label(new Rect(10, 20, 200, 20), "Is Grounded: " + cc.isGrounded);
        }
    }
}
