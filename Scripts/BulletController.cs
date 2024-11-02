using UnityEngine;

namespace Indie
{
    // Bullet Controller class to handle bullet behavior
    public class BulletController : MonoBehaviour, IPooledObject
    {
        private SimpleObjectPool objectPool;
        private string poolTag;
        private float speed;
        private float lifetime;
        private LayerMask hitLayers;
        private float spawnTime;

        public void Initialize(SimpleObjectPool pool, string tag, float bulletSpeed, float bulletLifetime, LayerMask layers)
        {
            objectPool = pool;
            poolTag = tag;
            speed = bulletSpeed;
            lifetime = bulletLifetime;
            hitLayers = layers;
            spawnTime = Time.time;
        }

        public void OnObjectSpawn()
        {
            spawnTime = Time.time;
        }

        private void Update()
        {
            // Move bullet forward
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            // Check lifetime
            if (Time.time - spawnTime >= lifetime)
            {
                ReturnToPool();
                return;
            }

            // Check for collisions
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, speed * Time.deltaTime, hitLayers))
            {
                HandleHit(hit);
            }
        }

        private void HandleHit(RaycastHit hit)
        {
            // Here you can add impact effects, damage dealing, etc.
            // Example:
            // IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            // if (damageable != null)
            // {
            //     damageable.TakeDamage(damage);
            // }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (objectPool != null)
            {
                objectPool.ReturnToPool(poolTag, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}