using System.Collections.Generic;
using UnityEngine;

public class HitParticleObjectPool : ObjectPool
{
    public override void SetupPool()
    {
        base.poolStack = new Stack<PooledObject>();
        PooledObject instance = null;
        for (int i = 0; i < initPoolSize; i++)
        {
            instance = Instantiate(pooledObject);
            instance.transform.SetParent(this.transform);
            instance.Pool = this;
            instance.gameObject.SetActive(false);
            poolStack.Push(instance);
        }
    }

    public override PooledObject GetPooledObject()
    {
        // Poolが足りなければ新しく生成
        if (poolStack.Count == 0)
        {
            PooledObject newInstance = Instantiate(pooledObject);
            newInstance.transform.SetParent(this.transform);
            newInstance.Pool = this;
            return newInstance;
        }

        // Stackから取り出す
        PooledObject nextInstance = poolStack.Pop();
        nextInstance.gameObject.SetActive(true);
        return nextInstance;
    }
}
