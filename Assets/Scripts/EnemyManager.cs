using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyPrefab;

    [SerializeField]
    private CirclePool _circlePool;
    [SerializeField]
    private SquarePool _squarePool;
    [SerializeField]
    private TrianglePool _trianglePool;
    [SerializeField]
    private Transform _playerTransform;

    private GameObject _enemy;

    private float upDownDistance = 4f;
    private float upDownTimes = 3f;
    private Vector3 _defaultPos = new Vector3(7f, 0f, 0f);

    private Vector3 _triangleFirstPos = new Vector3(3.5f, 3.3f, 0f);
    private Vector3 _triangleSecondPos = new Vector3(7f, 0f, 0f);
    private Vector3 _triangleThirdPos = new Vector3(3.5f, -3.3f, 0f);
    private MotionHandle _activeMotion;
    private CancellationTokenSource _restartCts;
    public EnemySequence currentEnemySequence;
    private float _currentCircleTime = 0f;
    private float _currentSquareTime = 0f;
    private float _currentTriangleTime = 0f;
    public enum EnemySequence
    {
        None = 0,
        Triangle,
        UpDown,
    }
    private const string TAG_ENEMY = "Enemy";
    private enum BulletType
    {
        circle = 0,
        square,
        triangle,
    }

    public async UniTask GameStart()
    {
        // Enemyの出現と初期設定
        if (_enemy == null) _enemy = Instantiate(_enemyPrefab);

        currentEnemySequence = EnemySequence.None;
        _enemy.transform.position = _defaultPos;

        _restartCts = new CancellationTokenSource();
        await UniTask.WaitForSeconds(0.1f);

        EnemyLoop(_restartCts.Token).Forget();
    }

    public void StopLoop()
    {
        if (_restartCts != null)
        {
            _restartCts.Cancel();
            _restartCts.Dispose();
            _restartCts = null;
        }

        if (_activeMotion.IsActive())
        {
            _activeMotion.Cancel();
        }

        if (_enemy != null) Destroy(_enemy);
    }

    public void ResetEnemy()
    {
        if (_circlePool != null) _circlePool.ClearAllPoolObjects();
        if (_squarePool != null) _squarePool.ClearAllPoolObjects();
        if (_trianglePool != null) _trianglePool.ClearAllPoolObjects();
        StopLoop();
    }

    public void EnemyBehaviour()
    {
        if (currentEnemySequence == EnemySequence.None) return;

        if (currentEnemySequence == EnemySequence.Triangle)
        {
            TriangleBehaviour();
        }

        if (currentEnemySequence == EnemySequence.UpDown)
        {
            UpDownBehaviour();
        }
    }

    private void TriangleBehaviour()
    {
        _currentTriangleTime += Time.deltaTime;
        _currentCircleTime += Time.deltaTime;

        if (_currentCircleTime > 3.2f)
        {
            ShootBullet(BulletType.circle);

            _currentCircleTime = 0f;
        }

        if (_currentTriangleTime > 1.8f)
        {
            ShootBullet(BulletType.triangle);

            _currentTriangleTime = 0f;
        }
    }

    private void UpDownBehaviour()
    {
        _currentSquareTime += Time.deltaTime;
        _currentCircleTime += Time.deltaTime;

        if (_currentCircleTime > 3.2f)
        {
            ShootBullet(BulletType.circle);

            _currentCircleTime = 0f;
        }

        if (_currentSquareTime > 1.8f)
        {
            ShootBullet(BulletType.square);

            _currentSquareTime = 0f;
        }
    }

    private async UniTaskVoid EnemyLoop(CancellationToken ct)
    {
        currentEnemySequence = EnemySequence.None;
        await UniTask.Delay(1000);

        while (!ct.IsCancellationRequested)
        {

            int nextSequence = UnityEngine.Random.Range(0, 1 + 1);
            switch (nextSequence)
            {
                case 0:
                    int times = UnityEngine.Random.Range(5, 7 + 1);
                    currentEnemySequence = EnemySequence.Triangle;
                    await TriangleSequence(times, ct);
                    break;
                case 1:
                    float moveTime = UnityEngine.Random.Range(5, 7 + 1);
                    currentEnemySequence = EnemySequence.UpDown;
                    await UpDownSequence(moveTime, ct);
                    break;

                default:
                    break;
            }

            currentEnemySequence = EnemySequence.None;
            await UniTask.Delay(2000, cancellationToken: ct);
        }
    }

    private async UniTask TriangleSequence(int time, CancellationToken ct)
    {
        int currentTime = 0;
        int currentTarget = 0;
        int nextTarget = 0;
        while (currentTime < time)
        {
            while (currentTarget == nextTarget)
            {
                nextTarget = UnityEngine.Random.Range(0, 2 + 1);
            }
            currentTarget = nextTarget;

            Vector3 targetPos = Vector3.zero;
            switch (nextTarget)
            {
                case 0:
                    targetPos = _triangleFirstPos;
                    break;
                case 1:
                    targetPos = _triangleSecondPos;
                    break;
                case 2:
                    targetPos = _triangleThirdPos;
                    break;
            }

            if (_enemy == null) return;

            _activeMotion = LMotion.Create(_enemy.transform.position, targetPos, 1.5f)
                        .WithEase(Ease.Linear)
                        .Bind(x =>
                        {
                            _enemy.transform.position = x;
                        });


            currentTime += 1;
        }

        if (_enemy == null) return;

        await _activeMotion.ToUniTask(cancellationToken: ct);

        _activeMotion = LMotion.Create(_enemy.transform.position, _defaultPos, 1f)
                    .WithEase(Ease.Linear)
                    .Bind(x =>
                    {
                        _enemy.transform.position = x;
                    });

        await _activeMotion.ToUniTask(cancellationToken: ct);
    }

    private async UniTask UpDownSequence(float time, CancellationToken ct)
    {
        float currentTime = 0f;
        float startTime = Time.time;

        while (currentTime < time)
        {
            if (_enemy == null) return;

            float passedTime = Time.time - startTime;
            float newY = _defaultPos.y + Mathf.Sin((passedTime * 2 * Mathf.PI) / upDownTimes) * upDownDistance;
            _enemy.transform.position = new Vector3(_defaultPos.x, newY, _defaultPos.z);

            currentTime += Time.deltaTime;
            await UniTask.Yield(cancellationToken: ct);
        }

        if (_enemy == null) return;

        _activeMotion = LMotion.Create(_enemy.transform.position, _defaultPos, 1f)
                    .WithEase(Ease.Linear)
                    .Bind(x =>
                    {
                        _enemy.transform.position = x;
                    });

        await _activeMotion.ToUniTask(cancellationToken: ct);
    }

    private void ShootBullet(BulletType bulletType)
    {
        PooledObject obj;
        if (bulletType == BulletType.circle)
        {
            obj = _circlePool.GetPooledObject();
            obj.transform.position = _enemy.transform.position + new Vector3(-0.5f, 0f, 0f);
            obj.GetComponent<BulletBase>().Shoot(_playerTransform, TAG_ENEMY, 3.0f);
        }
        else if (bulletType == BulletType.square)
        {
            obj = _squarePool.GetPooledObject();
            obj.transform.position = _enemy.transform.position + new Vector3(-0.5f, 0f, 0f);
            obj.GetComponent<BulletBase>().Shoot(_playerTransform, TAG_ENEMY, 3.0f);
        }
        else if (bulletType == BulletType.triangle)
        {
            obj = _trianglePool.GetPooledObject();
            obj.transform.position = _enemy.transform.position + new Vector3(-0.5f, 0f, 0f);
            obj.GetComponent<BulletBase>().Shoot(_playerTransform, TAG_ENEMY, 3.0f);
        }
        else return;
    }

    public EnemyStatus GetEnemyStatus()
    {
        EnemyStatus enemyStatus = _enemy.gameObject.GetComponent<EnemyStatus>();
        if (enemyStatus == null) return null;
        else return enemyStatus;
    }

}