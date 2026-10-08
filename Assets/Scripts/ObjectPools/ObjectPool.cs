using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] public uint initPoolSize;
    [SerializeField] public PooledObject pooledObject;
    [SerializeField] private HitParticleObjectPool hitParticle;

    public Stack<PooledObject> poolStack;

    private void Start()
    {
        SetupPool();
    }

    // プールを作成する
    public virtual void SetupPool()
    {
        poolStack = new Stack<PooledObject>();
        PooledObject instance = null;
        for (int i = 0; i < initPoolSize; i++)
        {
            instance = Instantiate(pooledObject);
            instance.GetComponent<BulletBase>().Initialize(_target, hitParticle);
            instance.transform.SetParent(this.transform);
            instance.Pool = this;
            instance.gameObject.SetActive(false);
            poolStack.Push(instance);
        }
    }

    public virtual PooledObject GetPooledObject()
    {
        // Poolが足りなければ新しく生成
        if (poolStack.Count == 0)
        {
            PooledObject newInstance = Instantiate(pooledObject);
            newInstance.GetComponent<BulletBase>().Initialize(_target, hitParticle);
            newInstance.transform.SetParent(this.transform);
            newInstance.Pool = this;
            return newInstance;
        }

        // Stackから取り出す
        PooledObject nextInstance = poolStack.Pop();
        nextInstance.gameObject.SetActive(true);
        return nextInstance;
    }

    public void ReturnToPool(PooledObject pooledObject)
    {
        if (pooledObject.gameObject.activeSelf == false) return;

        poolStack.Push(pooledObject);
        pooledObject.gameObject.SetActive(false);
    }

    public void ClearAllPoolObjects()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                PooledObject pooled = child.GetComponent<PooledObject>();
                if (pooled != null)
                {
                    ReturnToPool(pooled);
                }
            }
        }
    }
}
