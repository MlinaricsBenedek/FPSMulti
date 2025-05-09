using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    Rigidbody rigidBody;
    private PhotonView photonView;
    Collider _collider;
    public float dropForwardForce = 10f;
    public float dropUpwardForce = 10f;
    Transform playerCameraTransform;
    private Vector3 BulletSpread = new Vector3(0.01f, 0.01f, 0.01f);
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] ParticleSystem impact;
    [SerializeField] GameObject prefab;
    [SerializeField] Transform spawnPoint;
    float delay = 0.5f;
    float lastShootTime;
    float maxDistance = 100f;
    Vector3 Direction;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
       photonView = GetComponent<PhotonView>();
        prefab.SetActive(false);
        _collider = GetComponent<Collider>();
    }

    public void PickUp(Transform gunPosition)
    {
        Debug.Log($"PickUp meghívva: gunPosition = .{gunPosition.transform.position}");

        transform.SetParent(gunPosition);
        transform.localScale = transform.localScale / 2;
        transform.localPosition = new Vector3(0.5f, 0.2f, 0.404f);
        transform.localRotation = Quaternion.Euler(0f, -105.06f, 0f);

        rigidBody.isKinematic = true;
        _collider.isTrigger = true;
    }

    public void DropDown()
    {
        transform.SetParent(null);
        rigidBody.isKinematic = false;
        _collider.isTrigger = false;
        transform.localScale = transform.localScale * 2;
        rigidBody.AddForce(playerCameraTransform.forward * dropForwardForce, ForceMode.Impulse);
        rigidBody.AddForce(playerCameraTransform.up * dropUpwardForce, ForceMode.Impulse);
        float random = Random.Range(-0.01f, 0.01f);
        rigidBody.AddTorque(new Vector3(random, random, random) * 3);
    }
    
    public void Shoot()
    {
        if (lastShootTime + delay < Time.time)
        {
            Direction.Normalize();
            if (Physics.Raycast(spawnPoint.position, Direction, out RaycastHit hit, maxDistance))
            {
                prefab.SetActive(true);
                GameObject trailGO = PhotonNetwork.Instantiate("TrailRoot", spawnPoint.position, Quaternion.LookRotation(Direction));
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
        Direction = playerCameraTransform.transform.forward;
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
