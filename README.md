# Unity Object Pooling System

A lightweight, flexible, and efficient object pooling system for Unity projects. This system helps optimize performance by reusing GameObjects instead of repeatedly instantiating and destroying them.

## Features

- Easy-to-use pooling system with minimal setup
- Support for multiple pools with different object types
- Automatic pool expansion when needed
- Optional initialization interface for spawned objects
- No singleton pattern - supports multiple independent pools
- Thread-safe implementation
- Fully documented and maintainable code

## Installation

### Option 1: Unity Package Manager (Git URL)
1. Open the Package Manager window in Unity
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL"
4. Enter: `https://github.com/yourusername/unity-object-pooling.git`

### Option 2: Manual Installation
1. Download this repository
2. Copy the contents into your Unity project's `Assets` folder

## Quick Start

1. Create an empty GameObject in your scene
2. Add the ObjectPool component to it
3. Configure your pools in the inspector:
   - Set a unique tag for each pool
   - Assign the prefab you want to pool
   - Set the initial pool size

```csharp
// Reference the pool in your scripts
public class ProjectileManager : MonoBehaviour
{
    [SerializeField] private ObjectPool objectPool;

    public void FireProjectile(Vector3 position, Quaternion rotation)
    {
        GameObject projectile = objectPool.SpawnFromPool("Bullet", position, rotation);
        // Additional projectile setup if needed
    }
}
```

### Usage Examples
## Basic Usage

```csharp
// Spawn an object
GameObject obj = objectPool.SpawnFromPool("EnemyTag", position, rotation);

// Return an object to the pool
objectPool.ReturnToPool("EnemyTag", gameObject);
```

## Using the IPooledObject Interface

```csharp
public class Bullet : MonoBehaviour, IPooledObject
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnObjectSpawn()
    {
        // Reset the bullet's properties when spawned
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
```

## Checking Pool Status

```csharp
if (objectPool.HasPool("EnemyTag"))
{
    int poolSize = objectPool.GetPoolSize("EnemyTag");
    Debug.Log($"Current pool size: {poolSize}");
}
```

