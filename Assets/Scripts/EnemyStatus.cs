using UniRx;
using UnityEngine;

public class EnemyStatus : MonoBehaviour, IDamageable
{
    [SerializeField]
    private ReactiveProperty<float> _enemyHp = new ReactiveProperty<float>(10);
    public ReactiveProperty<float> enemyHp => _enemyHp;
    public float maxEnemyHp;

    private void Start()
    {
        maxEnemyHp = _enemyHp.Value;
    }

    public void TakeDamage(int damage)
    {
        _enemyHp.Value -= damage;
    }
}
