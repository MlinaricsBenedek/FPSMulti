using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    Rigidbody rigidBody;
    private PhotonView photonView;
    Collider _collider;
    public float dropForwardForce = 2f;
    public float dropUpwardForce = 2f;
    Transform playerCameraTransform;
    private Vector3 BulletSpread = new Vector3(0.01f, 0.01f, 0.01f);
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] ParticleSystem impact;
    [SerializeField] GameObject prefab;
    float delay = 0.2f;
    float lastShootTime;
    float maxDistance = 100f;
    [SerializeField] Transform Direction;
    public bool droppedGun=false;
    FirstPersonController FPSController;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
       photonView = GetComponent<PhotonView>();
        prefab.SetActive(false);
        _collider = GetComponent<Collider>();
    }

    public void PickUp(Transform gunPosition)
    {
        droppedGun = false;
        FPSController = GetComponentInParent<FirstPersonController>();
        transform.position = gunPosition.position;
        transform.rotation = gunPosition.rotation;
        transform.localScale = transform.localScale / 2;
        transform.localPosition = new Vector3(0.5f, 0.2f, 0.404f);
        transform.localRotation = Quaternion.Euler(0f, -105.06f, 0f);

        rigidBody.isKinematic = true;
        _collider.isTrigger = true;
    }

    public void DropDown()
    {
        droppedGun = true;
        rigidBody.isKinematic = false;
        _collider.isTrigger = false;
        rigidBody.AddForce(playerCameraTransform.forward * dropForwardForce, ForceMode.Impulse);
        rigidBody.AddForce(playerCameraTransform.up * dropUpwardForce, ForceMode.Impulse);
        float random = Random.Range(-0.01f, 0.01f);
        rigidBody.AddTorque(new Vector3(random, random, random) * 3);
    }

    public void Shoot()
    {
        if (lastShootTime + delay < Time.time)
        {
            Vector3 vector = Direction.forward;
            if (Physics.Raycast(Direction.position, vector, out RaycastHit hit, maxDistance))
            {
                prefab.SetActive(true);
                GameObject trailGO = PhotonNetwork.Instantiate("TrailRoot", Direction.position, Quaternion.LookRotation(vector));
                TrailRenderer trail = trailGO.GetComponentInChildren<TrailRenderer>();

                StartCoroutine(SpawnTrail(trail, hit));
                muzzleFlash.Play();
                lastShootTime = Time.time;
            }
        }
        prefab.SetActive(false);
    }

    public void GetDirection(Transform _playerCameraTransform)
    {
        playerCameraTransform = _playerCameraTransform;
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
        ParticleSystem particles = Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal));
        particles.Play();
        Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
        Destroy(trail.gameObject, trail.time);
    }
}
