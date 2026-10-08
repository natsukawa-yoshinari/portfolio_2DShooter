using UniRx;
using UnityEngine;

public class PlayerStatus : MonoBehaviour, IDamageable
{
    [SerializeField]
    private ReactiveProperty<float> _playerHp = new ReactiveProperty<float>(5);
    public ReactiveProperty<float> playerHp => _playerHp;
    public float maxPlayerHp;

    private void Start()
    {
        maxPlayerHp = _playerHp.Value;
    }

    public void TakeDamage(int damage)
    {
        _playerHp.Value -= damage;
    }
}
