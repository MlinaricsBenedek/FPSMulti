using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public class FirstPersonController : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    private Rigidbody rb;
    public Animator animator;
    public Camera playerCamera;
    public Transform AimTarget;
    public float fov = 60f;
    private float mouseSensitivity = 10f;
    private float maxLookAngle = 20f;
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private bool playerCanMove = true;
    private float walkSpeed = 5f;
    private float maxVelocityChange = 5.0f;
    float zDinstance = 10f;
    private bool slotFull;
    [SerializeField] public Transform GunPosition;
    GunController gunController;
    GameObject gunPv; 
    RotationConstraint LeftHandConstraint;
    RotationConstraint RightHandConstraint;
    IKController IKController;
    private float currentHeal;
    PlayerManager playerManager;
    PhotonView PV;
    Dictionary<Player, float> playersDamage = new();
    Dictionary<Player, float> shootTimers = new();
    Transform gunEndPoint;
    [SerializeField] GameObject Cap;
    [SerializeField] GameObject Vest;
    string Team;
    GameObject gun;
    void Start()
    {
        Cap.SetActive(false);
        Vest.SetActive(false);
        rb = GetComponent<Rigidbody>();
        playerCamera.fieldOfView = fov;
        PV = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

        Cursor.lockState = CursorLockMode.Locked;
        slotFull = false;
        IKController = GetComponent<IKController>();
        currentHeal = 100;
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>());
        }
        RotationConstraint[] constraints = GetComponentsInChildren<RotationConstraint>(true);

        foreach (RotationConstraint constraint in constraints)
        {
            if (constraint.gameObject.name == "hand_r")
            {
                constraint.enabled = true;
                RightHandConstraint = constraint;
            }
            else if (constraint.gameObject.name == "hand_l")
            {
                constraint.enabled = true;
                LeftHandConstraint = constraint;
            }
        }
        photonView.Owner.CustomProperties.TryGetValue("team", out object team);
        Team = team != null ? team.ToString(): "team";
        if (Team == "team")
        {
            Debug.Log("A player nem tartozik egyik csapathoz sem.");
        }
        Debug.Log(Team);
        Debug.Log(Teams.BlueTeams.Count);
        Debug.Log(Teams.RedTeams.Count);
        SetTeam(Team);
    }

    float camRotation;
   

    private void Update()
    {
        if (PV.IsMine)
        {
            HandleCamera();
            HandleInput();
  
        if (gunController is not null && !gunController.droppedGun)
        {
            gunController.gameObject.transform.position = GunPosition.position;
            gunController.gameObject.transform.rotation = GunPosition.rotation;
            gunController.GetDirection(playerCamera.transform);
        }
        }
    }

    void LateUpdate()
    {
        if (photonView is null || gunController is null) return;
        if (!photonView.IsMine && !gunController.droppedGun && slotFull)
        {
            if (gun != null && gun.transform.position != null)
            {
                gun.transform.position = GunPosition.position;
                gun.transform.rotation = GunPosition.rotation;
            }
        }
    }

    void SetTeam(string team)
    {
        if (team == "blueTeam")
        {
            photonView.RPC("RPC_Cap", RpcTarget.AllBuffered, true);
            photonView.RPC("RPC_Vest", RpcTarget.AllBuffered, false);
        }
        if(team == "redTeam")
        {
            photonView.RPC("RPC_Cap", RpcTarget.AllBuffered, false);
            photonView.RPC("RPC_Vest", RpcTarget.AllBuffered, true);

        }
    }

    [PunRPC]
    void RPC_Cap(bool isActive)
    {
        if (Cap != null)
            Cap.SetActive(isActive);
    }

    [PunRPC]
    void RPC_Vest(bool isActive)
    {
        if (Vest != null)
            Vest.SetActive(isActive);
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
        if(Input.GetKeyUp(KeyCode.Q)) DropDown();
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
        int gunLayerMask = LayerMask.GetMask("GUN");
        //Debug.Log("Beléptünk a pickupba.");
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out hit,100f,gunLayerMask) &&
            hit.collider != null &&
            Input.GetKey(KeyCode.E))
        {
            Debug.Log("Az ektakált object" + hit.collider.name);
            if (slotFull is false)
            {
                
                gunPv = hit.collider.gameObject;
                IKController.SetPickedUpGUn(true);
                IKController.SetGun(hit.collider.gameObject.transform);
                if (gunPv is null)
                {
                    Debug.Log("global gun null");
                }
                PhotonView gunPhotonView = gunPv.GetComponent<PhotonView>();
               // photonView.RPC(nameof(RPC_AttachGun), RpcTarget.AllBuffered,
               //photonView.ViewID,          // a fegyver ID-je
               //hit.collider.gameObject.transform.GetComponentInParent<PhotonView>().ViewID);
                gunPhotonView.RequestOwnership();
            }
        }
    }

    [PunRPC]
    void DestroyGunRPC(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view != null)
        {
            PhotonNetwork.Destroy(view.gameObject);
        }
    }

    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gunController.Shoot();
            int playerLayerMask = LayerMask.GetMask("Player");
            if (Physics.Raycast(gunEndPoint.position, gunEndPoint.forward, out RaycastHit hit, 100f, playerLayerMask) &&
                hit.collider != null)
            {
                Debug.DrawRay(gunEndPoint.position, gunEndPoint.forward * 100f, Color.red, 100f);

                Debug.Log(hit.collider.name);

                GameObject enemy = hit.collider.gameObject;
                Debug.Log(enemy);

                PhotonView enemyPV = enemy.GetPhotonView();
                if (enemyPV == photonView)
                {
                    Debug.LogError("the pv is ours");
                }
                string theirTeam = enemyPV.Owner.CustomProperties["team"]?.ToString();
                Debug.Log("enemy team:" + theirTeam);
                Debug.Log("my team" + Team);
                if (enemyPV != null && theirTeam != Team)
                {
                    enemyPV.RPC(nameof(RPC_TakeDamage), enemyPV.Owner, 60f);
                }
            }
        }
    }


    public void GetRotationConstraints(GunController gun,int photonViewID)
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
        Debug.Log("leftHandconstraintSource.sourceTransform)");
        IKController.SetLeftHandTargetTransform(leftHandconstraintSource.sourceTransform);
        LeftHandConstraint.AddSource(leftHandconstraintSource);
        LeftHandConstraint.constraintActive = true;
        Debug.Log("lefutott a lefthandconstrait:" + LeftHandConstraint);
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
        int gunId = gun.GetComponent<PhotonView>().ViewID;
        photonView.RPC(nameof(RPC_SetHandTargets), RpcTarget.AllBuffered, gunId);
    }

    [PunRPC]
    void RPC_SetHandTargets(int gunViewId)
    {
        PhotonView gunPV = PhotonView.Find(gunViewId);
        if (gunPV == null) return;       // ha még nem jött létre, késõbb újrahívódik a buffer miatt

        Transform gunTransform = gunPV.transform;
        Debug.Log(gunTransform);
        Transform left = gunTransform.Find("LeftHandTarget");
        Transform right = gunTransform.Find("RightHandTarget");
        IKController.SetLeftHandTargetTransform(left);
        IKController.SetRightHandTargetTransform(right);
        Debug.Log("RPC constraint " + LeftHandConstraint+", left: "+left);
        if (LeftHandConstraint != null && left != null)
        { 
            LeftHandConstraint.AddSource(new ConstraintSource { sourceTransform = left, weight = 1f });
            LeftHandConstraint.constraintActive = true;
            IKController.SetLeftHandTargetTransform(left);
        
        }
        Debug.Log("right:" + right);
        Debug.Log("Righhandconstrait" + RightHandConstraint);
        if (RightHandConstraint != null && right != null)
        {
            RightHandConstraint.AddSource(new ConstraintSource { sourceTransform = right, weight = 1f });
            RightHandConstraint.constraintActive = true;
            IKController.SetRightHandTargetTransform(right);
            Debug.Log("RPC constraint " + RightHandConstraint);

        }
    }

    public void DropDown()
    {
        if (slotFull is true )
        {
            if (gunController is not null)
            {
                gunController.DropDown();
                IKController.SetLeftHandTargetTransform(null);
                IKController.SetRightHandTargetTransform(null);
            }
        slotFull = false;
        }
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info)
    {
        if (!PV.IsMine) return;
        Player attacker = info.Sender;
       
        if (playersDamage.ContainsKey(attacker))
        {
            playersDamage[attacker] += damage;
            shootTimers[attacker] = Time.time;
        }

        else
        {
            playersDamage.Add(attacker, damage);
            shootTimers.Add(attacker, Time.time);
        }
        if (currentHeal > damage)
        {
            currentHeal -= damage;
        }
        else
        {
            currentHeal = 0;
            Die();

            PlayerManager.Find(attacker).GetKill();
        }
        HandleAssits(attacker);
    }

    public void HandleAssits(Player killer)
    {
        foreach (var pair in playersDamage)
        {
            Player assister = pair.Key;

            if (assister != killer)
            {
                float lastHitTime = shootTimers[assister];
                if (Time.time - lastHitTime <= 50f)
                {
                    PlayerManager assisterPlayerManager = PlayerManager.Find(assister);
                    if (assisterPlayerManager != null)
                    {
                        PhotonView assisterPV = assisterPlayerManager.GetComponent<PhotonView>();
                        assisterPV.RPC("RPC_GetAssist", assister);
                    }
                }
            }
        }
    }

    void Die()
    {
        Debug.Log("the player die");
        DropDown();
        playerManager.Die();
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    { }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if (!PV.IsMine) return;

        if (gunPv != null && targetView.gameObject == gunPv)
        {
            PhotonNetwork.Destroy(gunPv);
            gun = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Gun"), GunPosition.position, GunPosition.rotation);
            gunEndPoint = gun.transform.Find("Direction");
            gunController = gun.GetComponent<GunController>();
            gunController.PickUp(GunPosition);
            int viewId = gun.GetPhotonView().ViewID;
            GetRotationConstraints(gunController,viewId);
            slotFull = true;
        }
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    { }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
