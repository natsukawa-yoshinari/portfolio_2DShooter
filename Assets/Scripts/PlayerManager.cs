using Alchemy.Inspector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    private const string TAG_PLAYER = "Player";
    [SerializeField]
    private CirclePool _circlePool;
    [SerializeField]
    private SquarePool _squarePool;
    [SerializeField]
    private TrianglePool _trianglePool;
    [SerializeField]
    private Transform _reticle;
    [SerializeField]
    private Transform _player;
    [SerializeField]
    private GameObject _mousePos;

    [Alchemy.Inspector.Title("UI")]
    [SerializeField]
    private RectTransform _selectBoxRect;
    [SerializeField]
    private Image _circleImage;
    [SerializeField]
    private Image _squareImage;
    [SerializeField]
    private Image _triangleImage;

    [SerializeField]
    private Slider _changeBulletSlider;

    private bool _canShootCircle = true;
    private bool _canShootSquare = false;
    private bool _canShootTriangle = false;
    private ReactiveProperty<float> _currentShootCoolDown = new ReactiveProperty<float>(0f);
    public ReactiveProperty<float> currentShootCoolDown => _currentShootCoolDown;
    private float _shootCoolDown = 0.5f;
    public float shootCoolDown => _shootCoolDown;
    private bool _canShoot = false;

    private bool _canChangeBullet = true;
    private float _changeBulletCoolDown = 0.75f;
    private ReactiveProperty<float> _currentChangeBulletCD = new(0.75f);

    private void Start()
    {
        _currentChangeBulletCD.Subscribe(x =>
        {
            _changeBulletSlider.value = x / _changeBulletCoolDown;
        }).AddTo(this);
    }

    private void Update()
    {
        if (GameManager.IsGameEnd) return;

        // 弾の変更
        if (_canChangeBullet)
        {
            if (_canShootCircle == false)
                if (Input.GetKeyDown(KeyCode.Q)) SetBullet(true, false, false);
            if (_canShootSquare == false)
                if (Input.GetKeyDown(KeyCode.W)) SetBullet(false, true, false);
            if (_canShootTriangle == false)
                if (Input.GetKeyDown(KeyCode.E)) SetBullet(false, false, true);
        }
        // 変更した時のクールタイム
        else if (_canChangeBullet == false)
        {
            _currentChangeBulletCD.Value += Time.deltaTime;

            if (_currentChangeBulletCD.Value > _changeBulletCoolDown)
                _canChangeBullet = true;

        }

        // 発射のクールタイム
        if (_canShoot == false)
        {
            _currentShootCoolDown.Value += Time.deltaTime;

            if (_currentShootCoolDown.Value > _shootCoolDown)
            {
                _canShoot = true;
            }
        }

        // 弾の発射
        if (Input.GetMouseButtonDown(0) && _canShoot)
        {
            PooledObject obj;
            if (_canShootCircle)
            {
                obj = _circlePool.GetPooledObject();
                obj.transform.position = _player.position + new Vector3(0.5f, 0f, 0f);
                obj.GetComponent<BulletBase>().Shoot(_mousePos.gameObject.transform, TAG_PLAYER, 5.0f);
            }
            else if (_canShootSquare)
            {
                obj = _squarePool.GetPooledObject();
                obj.transform.position = _player.position + new Vector3(0.5f, 0f, 0f);
                obj.GetComponent<BulletBase>().Shoot(_mousePos.gameObject.transform, TAG_PLAYER, 5.0f);
            }
            else if (_canShootTriangle)
            {
                obj = _trianglePool.GetPooledObject();
                obj.transform.position = _player.position + new Vector3(0.5f, 0f, 0f);
                obj.GetComponent<BulletBase>().Shoot(_mousePos.gameObject.transform, TAG_PLAYER, 5.0f);
            }
            else return;
            _canShoot = false;
            _currentShootCoolDown.Value = 0f;
        }
    }

    // リスタート時のリセット
    public void ResetPlayer()
    {
        if (_circlePool != null) _circlePool.ClearAllPoolObjects();
        if (_squarePool != null) _squarePool.ClearAllPoolObjects();
        if (_trianglePool != null) _trianglePool.ClearAllPoolObjects();
        _currentShootCoolDown.Value = 0f;
        _canShoot = false;
        SetBullet(true, false, false);
    }

    // テスト用
    [Button]
    public void SetBullet(int i)
    {
        PooledObject obj;
        if (i == 1)
        {
            obj = _circlePool.GetPooledObject();
            obj.transform.position = _player.position + new Vector3(4f, 0f, 0f);
        }
        else if (i == 2)
        {
            obj = _squarePool.GetPooledObject();
            obj.transform.position = _player.position + new Vector3(4f, 0f, 0f);
        }
        else if (i == 3)
        {
            obj = _trianglePool.GetPooledObject();
            obj.transform.position = _player.position + new Vector3(4f, 0f, 0f);
        }
    }

    private Color _offColor = new Color(1f, 1f, 1f, 0.3f);
    private Color _onColor = new Color(1f, 1f, 1f, 1f);

    // trueのやつを明るく、それ以外を暗くする
    private void SetBullet(bool circle, bool square, bool triangle)
    {
        _canChangeBullet = false;
        _currentChangeBulletCD.Value = 0f;

        _canShootCircle = circle;
        _canShootSquare = square;
        _canShootTriangle = triangle;

        if (circle)
        {
            _selectBoxRect.transform.SetParent(_circleImage.transform.parent);
            _selectBoxRect.localPosition = new Vector3(0f, 0f, 0f);

            _circleImage.color = _onColor;
            _squareImage.color = _offColor;
            _triangleImage.color = _offColor;
        }
        else if (square)
        {
            _selectBoxRect.transform.SetParent(_squareImage.transform.parent);
            _selectBoxRect.localPosition = new Vector3(0f, 0f, 0f);

            _circleImage.color = _offColor;
            _squareImage.color = _onColor;
            _triangleImage.color = _offColor;
        }
        else if (triangle)
        {
            _selectBoxRect.transform.SetParent(_triangleImage.transform.parent);
            _selectBoxRect.localPosition = new Vector3(0f, 0f, 0f);

            _circleImage.color = _offColor;
            _squareImage.color = _offColor;
            _triangleImage.color = _onColor;
        }
    }
}
