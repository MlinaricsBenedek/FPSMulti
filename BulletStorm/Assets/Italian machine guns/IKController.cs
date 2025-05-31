using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IKController : MonoBehaviour
{
    public Animator animator;
    Transform rightHandTarget;
    Transform leftHandTarget;
    Transform aimTarget; 
    Transform gunHolder; 
    public float maxTilt = 10f; 
    public float tiltSpeed = 5f;
    private Quaternion initialRotation;
    PhotonView PhotonView;
    private void Awake()
    {
        rightHandTarget = null;
        leftHandTarget = null;
        aimTarget = GetComponentInChildren<Transform>().Find("AimTarget");
        gunHolder = GetComponentInChildren<Transform>().Find("GunPosition");
        PhotonView = gameObject.GetComponent<PhotonView>(); 
    }
    private void Start()
    {
        if (gunHolder != null)
            initialRotation = gunHolder.localRotation;

    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            Debug.Log("righthandtarget" + rightHandTarget);
            if (rightHandTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }
            Debug.Log("leftHandTarget" + leftHandTarget);
            if (leftHandTarget != null)
            { 
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            }
            if (gunHolder == null || aimTarget == null) return;
            else
            {
                Tilt();
            }
        }
        else
        {
            // Ha nincs fegyver, engedjük el az IK-t, hogy visszatérjen az alapállapotba
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
    }

    private void Update()
    {
        if (!PhotonView.IsMine) return;

        if (rightHandTarget != null && leftHandTarget != null)
        {
            PhotonView.RPC("GetIKTargets", RpcTarget.Others,
                rightHandTarget.position, rightHandTarget.rotation,
                leftHandTarget.position, leftHandTarget.rotation);
        }
    }

    [PunRPC]
    void GetIKTargets(Vector3 rightPos, Quaternion rightRot, Vector3 leftPos, Quaternion leftRot)
    {
        if (rightHandTarget == null)
            rightHandTarget = new GameObject("RemoteRightHandTarget").transform;
        if (leftHandTarget == null)
            leftHandTarget = new GameObject("RemoteLeftHandTarget").transform;

        rightHandTarget.position = rightPos;
        rightHandTarget.rotation = rightRot;

        leftHandTarget.position = leftPos;
        leftHandTarget.rotation = leftRot;
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

    private void Tilt()
    {
        Vector3 targetDirection = gunHolder.InverseTransformPoint(aimTarget.position);
        float tilt = Mathf.Clamp(targetDirection.y * maxTilt, -maxTilt, maxTilt);
        Quaternion targetRotation = initialRotation * Quaternion.Euler(-tilt, 0f, 0f);
        gunHolder.localRotation = Quaternion.Slerp(gunHolder.localRotation, targetRotation, Time.deltaTime * tiltSpeed);
    }
}
