using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    Rigidbody Rigidbody;
    [SerializeField] Transform GunPosition;
    Collider Collider;
    public float dropForwardForce = 10f;
    public float dropUpwardForce = 10f;
    [SerializeField] Transform PlayerCamera;

    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        if (Rigidbody is null)
        {
            Debug.Log("rigi is null");
        }
        
        Collider = GetComponent<Collider>();
    }
   
    public void PickUp()
    {
        //transform.SetParent(null);
        transform.SetParent(GunPosition);
        transform.localScale = transform.localScale / 2;
        transform.localPosition = new Vector3(0.5f, 0.2f, 0.404f);
        transform.localRotation = Quaternion.Euler(0f,-105.06f,0f);
        //transform.position = Vector3.zero;
        //transform.localScale = Vector3.one;
        ////Make Rigidbody kinematic and BoxCollider a trigger
        Rigidbody.isKinematic = true;
        Collider.isTrigger = true;
    }

    public void DropDown()
    {
        transform.SetParent(null);
        Rigidbody.isKinematic = false;
        Collider.isTrigger = false;
        //Gun carries momentum of player
        transform.localScale = transform.localScale * 2;
        //AddForce
        Rigidbody.AddForce(PlayerCamera.forward * dropForwardForce, ForceMode.Impulse);
        Rigidbody.AddForce(PlayerCamera.up * dropUpwardForce, ForceMode.Impulse);
        //Add random rotation
        float random = Random.Range(-0.01f, 0.01f);
        Rigidbody.AddTorque(new Vector3(random, random, random) * 3);
    }
}
