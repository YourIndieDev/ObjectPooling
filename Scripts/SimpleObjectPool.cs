using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Indie
{
    public class SimpleObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size;
        }

        [SerializeField] private List<Pool> pools;
        private Dictionary<string, Queue<GameObject>> poolDictionary;

        private void Awake()
        {
            InitializePools();
        }

        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = CreateNewPooledObject(pool.prefab);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }
        }

        private GameObject CreateNewPooledObject(GameObject prefab)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.parent = transform;
            return obj;
        }

        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
                return null;
            }

            Queue<GameObject> pool = poolDictionary[tag];
            GameObject objectToSpawn = pool.Dequeue();

            // If all objects are active, create a new one
            if (objectToSpawn.activeInHierarchy)
            {
                objectToSpawn = CreateNewPooledObject(pools.Find(p => p.tag == tag).prefab);
            }

            // Set up the object
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);

            // Add back to queue
            pool.Enqueue(objectToSpawn);

            // Initialize if implements IPooledObject
            if (objectToSpawn.TryGetComponent(out IPooledObject pooledObj))
            {
                pooledObj.OnObjectSpawn();
            }

            return objectToSpawn;
        }

        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
                return;
            }

            obj.SetActive(false);
        }

        // Method to check if a pool exists
        public bool HasPool(string tag)
        {
            return poolDictionary?.ContainsKey(tag) ?? false;
        }

        // Method to get current pool size
        public int GetPoolSize(string tag)
        {
            if (!HasPool(tag)) return 0;
            return poolDictionary[tag].Count;
        }
    }

    // Optional interface for objects that need initialization when spawned
    public interface IPooledObject
    {
        void OnObjectSpawn();
    }
}