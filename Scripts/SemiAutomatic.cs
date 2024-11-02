using UnityEngine;

namespace Indie
{
    public class SemiAutomaticGun : MonoBehaviour, IOnHold
    {
        [Header("Gun Settings")]
        [SerializeField] private string bulletPoolTag = "Bullet";
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.2f;
        [SerializeField] private float bulletSpeed = 30f;
        [SerializeField] private float bulletLifetime = 3f;
        [SerializeField] private LayerMask hitLayers;

        [Header("Spread Settings")]
        [SerializeField] private float baseSpread = 1f;
        [SerializeField] private float maxSpread = 5f;
        [SerializeField] private float spreadIncreasePerShot = 0.5f;
        [SerializeField] private float spreadRecoveryRate = 2f;
        [SerializeField] private float spreadRecoveryDelay = 0.5f;

        [Header("Recoil Settings")]
        [SerializeField] private float recoilForce = 2f;
        [SerializeField] private float recoilRecoverySpeed = 5f;
        [SerializeField] private AnimationCurve recoilCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Effects")]
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private AudioSource gunAudioSource;
        [SerializeField] private AudioClip shootSound;

        // Private variables
        private SimpleObjectPool objectPool;
        private float nextFireTime;
        private float currentSpread;
        private float lastShotTime;
        private float currentRecoil;
        private bool isRecoiling;
        private bool isGunHeld; // New variable to track if the gun is held

        private void Awake()
        {
            objectPool = FindObjectOfType<SimpleObjectPool>();
            if (objectPool == null)
            {
                Debug.LogError("No SimpleObjectPool found in scene!");
            }

            if (firePoint == null)
            {
                firePoint = transform;
            }

            currentSpread = baseSpread;
        }

        private void Update()
        {
            HandleShooting();
            UpdateSpread();
            HandleRecoilRecovery();
        }

        private void HandleShooting()
        {
            // Only allow shooting if the gun is held
            if (isGunHeld && Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
            {
                Fire();
            }
        }

        private void Fire()
        {
            if (!objectPool.HasPool(bulletPoolTag))
            {
                Debug.LogError($"No bullet pool with tag {bulletPoolTag} exists!");
                return;
            }

            nextFireTime = Time.time + fireRate;
            lastShotTime = Time.time;

            // Calculate spread
            float spreadX = Random.Range(-currentSpread, currentSpread);
            float spreadY = Random.Range(-currentSpread, currentSpread);
            Quaternion spreadRotation = Quaternion.Euler(spreadX, spreadY, 0f);

            // Spawn bullet
            GameObject bullet = objectPool.SpawnFromPool(
                bulletPoolTag,
                firePoint.position,
                firePoint.rotation * spreadRotation
            );

            BulletController bulletController = bullet.GetComponent<BulletController>() ?? bullet.AddComponent<BulletController>();
            bulletController.Initialize(objectPool, bulletPoolTag, bulletSpeed, bulletLifetime, hitLayers);

            ApplyRecoil();
            PlayShootEffects();
            IncreaseSpread();
        }

        private void UpdateSpread()
        {
            if (Time.time - lastShotTime > spreadRecoveryDelay)
            {
                currentSpread = Mathf.MoveTowards(
                    currentSpread,
                    baseSpread,
                    spreadRecoveryRate * Time.deltaTime
                );
            }
        }

        private void IncreaseSpread()
        {
            currentSpread = Mathf.Min(currentSpread + spreadIncreasePerShot, maxSpread);
        }

        private void ApplyRecoil()
        {
            currentRecoil = recoilForce;
            isRecoiling = true;
            StartCoroutine(RecoilCoroutine());
        }

        private System.Collections.IEnumerator RecoilCoroutine()
        {
            float elapsedTime = 0f;
            Vector3 startPosition = transform.position;

            while (elapsedTime < fireRate)
            {
                float recoilProgress = elapsedTime / fireRate;
                float recoilValue = recoilCurve.Evaluate(recoilProgress);

                Vector3 recoilOffset = Vector3.back * currentRecoil * recoilValue;
                transform.position = startPosition + recoilOffset;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            isRecoiling = false;
        }

        private void HandleRecoilRecovery()
        {
            if (!isRecoiling)
            {
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    Vector3.zero,
                    recoilRecoverySpeed * Time.deltaTime
                );
            }
        }

        private void PlayShootEffects()
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            if (gunAudioSource != null && shootSound != null)
            {
                gunAudioSource.PlayOneShot(shootSound);
            }
        }

        // Public method to set the gun held status
        public void SetGunHeld(bool held)
        {
            isGunHeld = held;
        }

        // Public methods for external control
        public void SetFireRate(float rate)
        {
            fireRate = Mathf.Max(0.1f, rate);
        }

        public void SetBulletSpeed(float speed)
        {
            bulletSpeed = Mathf.Max(1f, speed);
        }

        public void SetSpreadSettings(float baseValue, float maxValue, float increaseRate)
        {
            baseSpread = Mathf.Max(0f, baseValue);
            maxSpread = Mathf.Max(baseSpread, maxValue);
            spreadIncreasePerShot = Mathf.Max(0f, increaseRate);
        }

        public void OnHold()
        {
            isGunHeld = true;
        }

        public void OnRelease()
        {
            isGunHeld = false;
        }
    }
}
