using UnityEngine;

public class BulletBase : PooledObject
{
    private const int LAYER_CIRCLE = 10;
    private const int LAYER_SQUARE = 11;
    private const int LAYER_TRIANGLE = 12;
    private const string TAG_PLAYER = "Player";
    private const string TAG_ENEMY = "Enemy";


    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField]
    private ParticleSystem _hitParticle;

    [SerializeField]
    private float _lifeTime = 5f;
    public float lifeTime => _lifeTime;

    private float _bulletSpeed = 0;

    private Transform _targetTransform;
    public Transform targetTransform => _targetTransform;

    private float _currentTime = 0f;
    private Vector3 _shootVector = new Vector3();
    private HitParticleObjectPool _hitParticlePool;
    private Vector3 _defaultScale = new Vector3(0.5f, 0.5f, 0.5f);

    public virtual void OnEnable()
    {
        // デフォルトの状態に戻す
        _currentTime = 0f;
        gameObject.transform.localScale = _defaultScale;

        // Playerであれば
        if (this.gameObject.tag == TAG_PLAYER)
        {
            // コンボが続いているとScaleを大きくする
            float currentCombo = (float)GameManager.ComboCount.Value;
            gameObject.transform.localScale *= (1 + (currentCombo * 0.03f));
            // Debug.Log(gameObject.transform.localScale);
        }
    }

    public virtual void Update()
    {
        // LifeTimeを設定して自動でPoolにもどる
        _currentTime += Time.deltaTime;
        if (_lifeTime < _currentTime)
        {
            _currentTime = 0f;
            Release();
            return;
        }

        if (GameManager.IsGameEnd) return;

        // 打たれた方向にすすむ
        this.gameObject.transform.position += _shootVector * _bulletSpeed * Time.deltaTime;
    }

    public void Initialize(Transform targetTransform, HitParticleObjectPool hitParticleObjectPool)
    {
        _targetTransform = targetTransform;
        _hitParticlePool = hitParticleObjectPool;
    }

    /// <summary>
    /// BulletBase.cs Player, 敵の両方の弾の基底クラス
    /// Colliderを使用した衝突判定を使用 
    /// </summary>
    public virtual void Shoot(Transform target, string tag, float bulletSpeed)
    {
        // 敵が打ったか、自分が撃ったかでColorを変える
        this.gameObject.tag = tag;
        if (tag == "Enemy") _spriteRenderer.color = Color.softRed;
        if (tag == "Player") _spriteRenderer.color = Color.softGreen;

        _targetTransform = target;
        _shootVector = Vector3.Normalize(target.position - transform.position);
        _bulletSpeed = bulletSpeed;
    }

    // 衝突判定
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 同じTagの弾は無視
        if (collision.gameObject.CompareTag(this.gameObject.tag)) return;

        // 違うTagであるなら
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 弾ならReturn
            if (CheckIsBullet(collision.gameObject)) return;
            OnEnemyHit(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            if (CheckIsBullet(collision.gameObject)) return;
            OnPlayerHit(collision.gameObject);
        }
    }

    // 弾かどうかを判断する
    private bool CheckIsBullet(GameObject collision)
    {
        if (collision.gameObject.layer == LAYER_CIRCLE)
        {
            OnCircleBulletHit();
            return true;
        }
        else if (collision.gameObject.layer == LAYER_SQUARE)
        {
            OnSquareBulletHit();
            return true;
        }
        else if (collision.gameObject.layer == LAYER_TRIANGLE)
        {
            OnTriangleBulletHit();
            return true;
        }
        else return false;
    }

    public virtual void OnEnemyHit(GameObject enemyObj)
    {
        enemyObj.GetComponent<IDamageable>().TakeDamage(1);
        PlayHitParticle();
        Release();
    }

    public virtual void OnPlayerHit(GameObject playerObj)
    {
        playerObj.GetComponent<IDamageable>().TakeDamage(1);
        PlayHitParticle();
        Release();
    }

    // 他の弾に当たった時
    public virtual void OnCircleBulletHit() { }
    public virtual void OnSquareBulletHit() { }
    public virtual void OnTriangleBulletHit() { }

    // 弾が何かしらに当たった時のParticleの出現
    public void PlayHitParticle()
    {
        PooledObject obj = _hitParticlePool.GetPooledObject();
        obj.transform.position = this.gameObject.transform.position;
        obj.GetComponent<ParticleSystem>().Emit(8);
    }

    // コンボの増加
    public void AddComboCount()
    {
        if (this.gameObject.tag == TAG_PLAYER) GameManager.ComboCount.Value += 1;
    }

    public void ResetComboCount()
    {
        if (this.gameObject.tag == TAG_PLAYER) GameManager.ComboCount.Value = 0;
    }
}
