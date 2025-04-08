using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public class FirstPersonController : MonoBehaviour
{
    private Rigidbody rb;
    public Animator animator;
    public Camera playerCamera;
    public Transform AimTarget;
    public float fov = 60f;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 50f;
    public bool lockCursor = true;
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 5.0f;
    float zDinstance = 10f;
    public bool slotFull;
    public bool equipped;
    public float dropForwardForce = 10f;
    public float dropUpwardForce =10f;
    public Transform GunPosition;
    GunController gunController;
    [SerializeField] RotationConstraint LeftHandConstraint;
    [SerializeField] RotationConstraint RightHandConstraint;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera.fieldOfView = fov;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        slotFull = false;
        equipped =false;
    }

    float camRotation;

    private void Update()
    {
        HandleCamera();
        PickUp();
    }

    void FixedUpdate()
    {
        Move();
        HandleInput();
    }

    public void HandleInput()
    {
        if (slotFull is not true && Input.GetKeyUp(KeyCode.E))
        {
         //   PickUp();
        }
        else if(slotFull is not false && Input.GetKeyUp(KeyCode.Q))
        {
            DropDown();
        }
    }

    private void HandleCamera()
    {
        yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.localEulerAngles = new Vector3(0, yaw, 0);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = zDinstance;
        Vector3 mouseWorldPosition = playerCamera.ScreenToWorldPoint(mouseScreenPosition);
        AimTarget.transform.position = mouseWorldPosition;
    }

    private void Move()
    {
        if (playerCanMove)
        {
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (targetVelocity.x != 0 || targetVelocity.z != 0)
            {
                animator.SetBool("IsMoving", true);
            }
            else
            {
                animator.SetBool("IsMoving", false);
            }
            targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;
            Vector3 velocity = rb.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }

    public void PickUp()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out hit) &&
            hit.collider != null &&
            Input.GetKey(KeyCode.E))
        {
            gunController = hit.collider.GetComponent<GunController>();
            //GetRotationConstraints(gunController);
            if (gunController != null)
            {
                Debug.Log("Fegyver találva, PickUp meghívva");
                gunController.PickUp();
                GetRotationConstraints(gunController);
            }
            else
            {
                Debug.LogWarning("Talált objektumon nincs GunController komponens!");
            }
        }
        equipped = true;
        slotFull = true;
      
    }

    public void GetRotationConstraints(GunController gun)
    {
        ConstraintSource leftHandconstraintSource = new();
        while (LeftHandConstraint.sourceCount > 0)
        {
            LeftHandConstraint.RemoveSource(0);
        }
        Debug.Log(gun.name);
        leftHandconstraintSource.weight = 1f;
        leftHandconstraintSource.sourceTransform = gun.gameObject.transform.Find("LeftHandTarget");
        Debug.Log(gun.gameObject.transform.Find("LeftHandTarget"));
        LeftHandConstraint.AddSource(leftHandconstraintSource);
        
        ConstraintSource rightHandconstraintSource = new();
        while (RightHandConstraint.sourceCount > 0)
        {
            RightHandConstraint.RemoveSource(0);
        }
        rightHandconstraintSource.weight = 1f;
        rightHandconstraintSource.sourceTransform = gun.gameObject.transform.Find("RightHandTarget");
        Debug.Log("rightsource:"+rightHandconstraintSource);
        Debug.Log(gun.gameObject.transform.Find("RightHandTarget"));
        RightHandConstraint.AddSource(rightHandconstraintSource);
        RightHandConstraint.constraintActive = true;
       // RightHandConstraint.transform.localRotation = Quaternion.Euler(-90, 0, 0);
    }

    public void DropDown()
    {
        if (gunController is not null)
        {
            gunController.DropDown();
        }
        equipped = false;
        slotFull = false;

    }
}
