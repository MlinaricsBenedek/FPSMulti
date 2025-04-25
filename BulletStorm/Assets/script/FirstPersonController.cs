using Photon.Pun;
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
    private bool cameraCanMove = true;
    private float mouseSensitivity = 100f;
    private float maxLookAngle = 50f;
    private bool lockCursor = true;
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private bool playerCanMove = true;
    private float walkSpeed = 5f;
    private float maxVelocityChange = 5.0f;
    float zDinstance = 10f;
    private bool slotFull;
    private bool equipped;
    private float dropForwardForce = 10f;
    private float dropUpwardForce =10f;
    private Transform GunPosition;
    GunController gunController;
    [SerializeField] RotationConstraint LeftHandConstraint;
    [SerializeField] RotationConstraint RightHandConstraint;
    IKController IKController;
    private float currentHeal;
    PlayerManager playerManager;
    PhotonView PV;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera.fieldOfView = fov;
        PV = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
        GunPosition = GetComponentInChildren<Transform>().Find("GunPosition");
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        slotFull = false;
        equipped =false;
        IKController = GetComponent<IKController>();
        currentHeal = 100;
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>());
        }
        RotationConstraint[] constraints = GetComponentsInChildren<RotationConstraint>(true);

        foreach (RotationConstraint constraint in constraints)
        {
            if (constraint.gameObject.name == "")
            {
                Debug.Log("Megtaláltam az elsõ constraintet: " + constraint.name);
                // Itt csinálhatsz vele bármit, pl. engedélyezheted
                constraint.enabled = true;
            }
            else if (constraint.gameObject.name == "MásodikConstraintNeve")
            {
                Debug.Log("Megtaláltam a második constraintet: " + constraint.name);
                constraint.enabled = false;
            }
        }
    }

    float camRotation;

    private void Update()
    {
        if (PV.IsMine)
        {
            HandleCamera();
            HandleInput();
        }
    }

    void FixedUpdate()
    {
        if (PV.IsMine)
        {
            Move();
        }
    }

    public void HandleInput()
    {
        PickUp();
        DropDown();
        if (slotFull is true)
        {
           Shoot();
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
            if (slotFull is false)
            {
                gunController = hit.collider.GetComponent<GunController>();
                if (gunController != null)
                {

                    gunController.PickUp();
                    GetRotationConstraints(gunController);
                    equipped = true;
                    slotFull = true;
                }
            }
        }
    }

    private void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out hit) &&
            hit.collider != null &&
            Input.GetMouseButtonDown(0))
        {
            gunController.Shoot();
            FirstPersonController enemy = hit.collider.GetComponent<FirstPersonController>();
            enemy.TakeDamage(20);
        }
    }

    public void GetRotationConstraints(GunController gun)
    {
        while (LeftHandConstraint.sourceCount > 0)
        {
            LeftHandConstraint.RemoveSource(0);
        }

        ConstraintSource leftHandconstraintSource = new()
        {
            weight = 1f,
            sourceTransform = gun.gameObject.transform.Find("LeftHandTarget"),
        };
        IKController.SetLeftHandTargetTransform(leftHandconstraintSource.sourceTransform);
        LeftHandConstraint.AddSource(leftHandconstraintSource);

        while (RightHandConstraint.sourceCount > 0)
        {
            RightHandConstraint.RemoveSource(0);
        }

        ConstraintSource rightHandconstraintSource = new()
        {
            weight = 1f,
            sourceTransform = gun.gameObject.transform.Find("RightHandTarget"),
        };
       
        IKController.SetRightHandTargetTransform(rightHandconstraintSource.sourceTransform);
        RightHandConstraint.AddSource(rightHandconstraintSource);
        RightHandConstraint.constraintActive = true;
    }

    public void DropDown()
    {
        if (slotFull is true && Input.GetKeyUp(KeyCode.Q))
        {
            if (gunController is not null)
            {
                 gunController.DropDown();
                IKController.SetLeftHandTargetTransform(null);
                IKController.SetRightHandTargetTransform(null);
            }
        equipped = false;
        slotFull = false;
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("beleptunk a takedamagebe");
        PV.RPC(nameof(RPC_TakeDamage), PV.Owner, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        Debug.Log("beleptunk az RPC_TAKEDAMAGE-be!!!!");
        if (currentHeal > damage)
        {
            currentHeal -= damage;
        }
        else
        {
            currentHeal = 0;
            Die();
        }
    }
    void Die()
    {
        Debug.Log("the player die");
        playerManager.Die();
    }
}
