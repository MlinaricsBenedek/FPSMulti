using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class IKController : MonoBehaviour, IPunObservable
{
    public Animator animator;

    [SerializeField] private Transform gunHolder;
    private Quaternion initialRotation;

    private PhotonView photonView;

    private Transform rightHandTarget;
    private Transform leftHandTarget;
    private Transform aimTarget;

    public float maxTilt = 60f;
    public float tiltSpeed = 5f;
    private Vector3 rightHandPos, leftHandPos;
    private Quaternion rightHandRot, leftHandRot;
    bool pickup=false;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

        aimTarget = transform.Find("AimTarget");
    }

    private void Start()
    {
        if (gunHolder != null)
            initialRotation = gunHolder.localRotation;
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine &&pickup)
        {
            if (rightHandTarget is null)
            {
                Debug.Log("rightHandTarget is null");
            }
            rightHandTarget.SetPositionAndRotation(rightHandPos, rightHandRot);
            if(leftHandTarget is null)
            {
                Debug.Log("leftHandTarget is null");
            }
            leftHandTarget.SetPositionAndRotation(leftHandPos, leftHandRot);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        ApplyHandIK();

        if (photonView.IsMine)
        {
            Tilt();
        }
    }

    public void SetGun(Transform gunTransform)
    {
        if (gunTransform == null) return;

        Transform left = gunTransform.Find("LeftHandTarget");
        Debug.Log(left);
        Transform right = gunTransform.Find("RightHandTarget");
        Debug.Log(right);
        SetLeftHandTargetTransform(left);
        SetRightHandTargetTransform(right);
    }


    private void ApplyHandIK()
    {
        if (rightHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }

        if (leftHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
    }

    private void Tilt()
    {
        if (gunHolder == null || aimTarget == null) return;

        Vector3 aimPos = gunHolder.InverseTransformPoint(aimTarget.position);
        float tilt = Mathf.Clamp(aimPos.y * maxTilt, -maxTilt, maxTilt);
        Quaternion rotation = initialRotation * Quaternion.Euler(0f, 0f, tilt);

        gunHolder.localRotation = Quaternion.Slerp(
            gunHolder.localRotation, rotation, Time.deltaTime * tiltSpeed);
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rightHandTarget != null);
            Debug.Log(rightHandTarget);
            if (rightHandTarget != null)
            {
                stream.SendNext(rightHandTarget.position);
                stream.SendNext(rightHandTarget.rotation);
            }

            stream.SendNext(leftHandTarget != null);
            Debug.Log(leftHandTarget);
            if (leftHandTarget != null)
            {
                stream.SendNext(leftHandTarget.position);
                stream.SendNext(leftHandTarget.rotation);
            }
        }
        else
        {
            if ((bool)stream.ReceiveNext())
            {
                rightHandPos = (Vector3)stream.ReceiveNext();
                rightHandRot = (Quaternion)stream.ReceiveNext();
                Debug.Log("righthandpos: " + rightHandPos);
                Debug.Log("righthandpos: " + rightHandRot);
            }

            if ((bool)stream.ReceiveNext())
            {
                leftHandPos = (Vector3)stream.ReceiveNext();
                leftHandRot = (Quaternion)stream.ReceiveNext();
                Debug.Log("lefhandpos" + leftHandPos);
                Debug.Log("leftHandRot" + leftHandRot);
            }
        }
    }

#nullable enable
    public void SetRightHandTargetTransform(Transform? _rightHandTarget)
    {
        rightHandTarget = _rightHandTarget;
    }
    #nullable enable
    public void SetLeftHandTargetTransform(Transform? _leftHandTarget)
    {
        leftHandTarget = _leftHandTarget;
    }

    public void SetPickedUpGUn(bool pickedUpGUn)
    {
        pickup = pickedUpGUn;
    }
}
