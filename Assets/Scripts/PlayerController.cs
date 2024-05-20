using UnityEngine;

public class PlayerController : MonoBehaviour{

    public float mouseSensitivity = 10f;
    public Vector2 pitchMinMax = new Vector2(-85f, 85f);
    
    public float walkSpeed = 3f;
    public float runSpeed = 7f;

    public float speedSmoothTime = 0.1f;
    float speedSmoothVelocity;

    public float turnSmoothTime = 0.1f;
    Vector3 turnSmoothVelocity;

    public float maxSlopeAngle = 45f;
    
    public float jumpHeight = 2f;


    float pitch;
    float yaw;
    Vector3 currentRotation;

    float currentSpeed;
    bool grounded;
    float velocityY;
    Vector3 contactNormal;
    Vector3 movementDir;


    Transform cameraT;
    CharacterController body;
    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraT = Camera.main.transform;    
        body = GetComponent<CharacterController>();
    }

    void FixedUpdate(){
        grounded = isGrounded();


        //Player and Camera rotation
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;

        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
        body.transform.localRotation =      Quaternion.Euler(0,                 currentRotation.y, 0);
        cameraT.transform.localRotation = Quaternion.Euler(currentRotation.x,                 0, 0);

        
        //Player Movement
        //direction
        float theta = -cameraT.eulerAngles.y * Mathf.Deg2Rad;
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = input.x * new Vector2(Mathf.Cos(theta), Mathf.Sin(theta))  +  input.y * new Vector2(-Mathf.Sin(theta), Mathf.Cos(theta)); 
        Vector2 inputDir = input.normalized;

        //keep movement direction the same as previous update if now in the air or input is null
        if (grounded && inputDir != Vector2.zero) { 
            movementDir = new Vector3(inputDir.x, 0f, inputDir.y);
            movementDir = Vector3.ProjectOnPlane(movementDir, contactNormal).normalized;
        }

        //speed
        bool running = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = (running ? runSpeed : walkSpeed) * inputDir.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        //velocity
        velocityY += Physics.gravity.y * Time.deltaTime;
        if (grounded) {
            velocityY = 0;

            if(Input.GetKey(KeyCode.Space)) {
                velocityY += Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight);
            }
        }
        Vector3 velocity = movementDir * currentSpeed;
        velocity += Vector3.up * velocityY;
        body.Move(velocity * Time.deltaTime);
    }

    float GetModifiedSmoothTime(float smoothTime) {
        if (grounded) {
            return smoothTime;
        }
        return float.MaxValue;
    }

    bool isGrounded(){
        RaycastHit hitInfo;
        bool groundHit = Physics.Raycast(transform.position, Vector3.down, out hitInfo, 1.2f);
        contactNormal = hitInfo.normal;

        if (groundHit) {
            float angle = Vector3.Angle(Vector3.up, contactNormal);
            return angle < maxSlopeAngle;
        }
        return false;
    }
}
