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
    private Vector3 BulletSpread = new Vector3(0.01f, 0.01f, 0.01f);
    [SerializeField] ParticleSystem muzzleFlash;
    
    [SerializeField] ParticleSystem Impact;
    [SerializeField] GameObject Prefab;
    [SerializeField] Transform SpawnPoint;
    float Delay = 0.5f;
    float LastShootTime;
    float MaxDistance = 100f;

    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        if (Rigidbody is null)
        {
            Debug.Log("rigi is null");
        }
        
        Debug.Log(SpawnPoint.transform.position);
        Collider = GetComponent<Collider>();
    }

    public void PickUp()
    {
        //transform.SetParent(null);
        transform.SetParent(GunPosition);
        transform.localScale = transform.localScale / 2;
        transform.localPosition = new Vector3(0.5f, 0.2f, 0.404f);
        transform.localRotation = Quaternion.Euler(0f, -105.06f, 0f);
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

    public void Shoot()
    {
        if (LastShootTime + Delay < Time.time)
        {
            Debug.Log(SpawnPoint);
            Vector3 direction = GetDirection();
            if (Physics.Raycast(SpawnPoint.position, direction, out RaycastHit hit, MaxDistance))
            {
                GameObject trailGO = Instantiate(Prefab, SpawnPoint.position, Quaternion.LookRotation(direction));

                // Innen lekérheted a TrailRenderer komponenst:
                TrailRenderer trail = trailGO.GetComponentInChildren<TrailRenderer>();
                //TrailRenderer trail = Instantiate(Prefab, SpawnPoint.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, hit));
                muzzleFlash.Play();
                LastShootTime = Time.time;
            }
        }
    }
    private Vector3 GetDirection()
    {
        Vector3 direction = PlayerCamera.transform.forward;
        direction.Normalize();
        return direction;
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit)
    {
        float time = 0;
        Vector3 startPos = trail.transform.position;
        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPos, hit.point, time);
            time += Time.deltaTime / trail.time;

            yield return null;
        }
        trail.transform.position = hit.point;
        ParticleSystem particles = Instantiate(Impact, hit.point, Quaternion.LookRotation(hit.normal));
        particles.Play();
        Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
        Destroy(trail.gameObject, trail.time);
    }
}
