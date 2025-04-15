using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour
{
    public Animator animator;
    Transform rightHandTarget;
    Transform leftHandTarget;
    public Transform aimTarget; // Ez az ami követi az egeret
    public Transform gunHolder; // Ez a gun parentje
    public float maxTiltAngle = 10f; // Max dõlés szög
    public float tiltSpeed = 5f;
    private Quaternion initialRotation;
    private void Awake()
    {
        rightHandTarget = null;
        leftHandTarget = null;

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
            Debug.Log("IK-ban lévõ adat jobb kéz: "+rightHandTarget);
            // Jobb kéz mozgásának beállítása
            if (rightHandTarget != null)
            {
                Debug.Log("itt már lefut az Ik script");
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }

            // Bal kéz mozgásának beállítása (ha támogatott)
            if (gunHolder == null || aimTarget == null) return;
            else 
            {
                Vector3 localTargetDir = gunHolder.InverseTransformPoint(aimTarget.position);

                float tiltAmount = Mathf.Clamp(localTargetDir.y * maxTiltAngle, -maxTiltAngle, maxTiltAngle);
                Quaternion targetRotation = initialRotation * Quaternion.Euler(-tiltAmount, 0f, 0f);
                gunHolder.localRotation = Quaternion.Slerp(gunHolder.localRotation, targetRotation, Time.deltaTime * tiltSpeed);
            }

            if (leftHandTarget != null)
            {
                Debug.Log("itt már lefut az Ik script");
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
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
}
