using UnityEngine;
using UniRx;
using UnityEngine.UI;
using LitMotion;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Transform _player;
    [SerializeField]
    private GameObject _reticle;
    [SerializeField]
    private BulletLineRenderer _bulletLine;
    [SerializeField]
    private EnemyManager _enemyManager;
    [SerializeField]
    private PlayerManager _playerManager;
    [SerializeField]
    private PlayerStatus _playerStatus;
    [Alchemy.Inspector.Title("UI")]
    [SerializeField]
    private Slider _playerHpSlider;
    [SerializeField]
    private Slider _enemyHpSlider;
    [SerializeField]
    private Image _shootCdCircle;
    [SerializeField]
    private TextMeshProUGUI _comboText;

    [Alchemy.Inspector.Title("Start And End UI")]
    [SerializeField]
    private GameObject _startBackground;
    [SerializeField]
    private Button _startButton;
    [SerializeField]
    private GameObject _endBackground;
    [SerializeField]
    private Button _restartButton;
    [SerializeField]
    private TextMeshProUGUI _resultText;
    public static bool IsGameEnd = true;
    public static ReactiveProperty<int> ComboCount = new(0);

    private CompositeDisposable _gameSessionDisposable = new CompositeDisposable();
    private EnemyStatus _enemyStatus;

    private void Awake()
    {
        _bulletLine.Initialize(_player, _reticle);
    }

    private void Start()
    {
        _startBackground.SetActive(true);
        _endBackground.SetActive(false);
        IsGameEnd = true;

        _startButton.onClick.AddListener(() =>
        {
            _startBackground.SetActive(false);
            GameStart().Forget();
        });

        ComboCount.Subscribe(x =>
        {
            _comboText.text = $"{x} Combo";
        });

        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(() =>
            {
                ResetGame();
            });
        }
    }

    private async UniTask GameStart()
    {
        _gameSessionDisposable.Clear();

        // 新しいEnemyを取得
        await _enemyManager.GameStart();
        _enemyStatus = _enemyManager.GetEnemyStatus();

        _playerStatus.playerHp.Value = _playerStatus.maxPlayerHp;
        if (_enemyStatus != null)
        {
            _enemyStatus.enemyHp.Value = _enemyStatus.maxEnemyHp;
        }
        _playerHpSlider.value = 1f;
        _enemyHpSlider.value = 1f;

        IsGameEnd = false;

        // PlayerのGameOverを検知
        _playerStatus.playerHp
        .Where(hp => hp <= 0)
        .Subscribe(hp =>
        {
            _resultText.text = "Player Lose...";
            GameEnd();
        }).AddTo(_gameSessionDisposable);

        // PlayerHP減少のTween
        _playerStatus.playerHp.Pairwise()
        .Subscribe(hp =>
        {
            LMotion.Create(hp.Previous, hp.Current, 0.25f)
                    .WithEase(Ease.Linear)
                    .Bind(x =>
                    {
                        _playerHpSlider.value = x / _playerStatus.maxPlayerHp;
                    }).AddTo(this);
        }).AddTo(_gameSessionDisposable);

        if (_enemyStatus != null)
        {
            // PlayerWinの検知
            _enemyStatus.enemyHp
            .Where(hp => hp <= 0)
            .Subscribe(hp =>
            {
                _resultText.text = "Player Win !";
                GameEnd();
            }).AddTo(_gameSessionDisposable);

            // EnemyHP減少のTween
            _enemyStatus.enemyHp.Pairwise()
            .Subscribe(hp =>
            {
                LMotion.Create(hp.Previous, hp.Current, 0.25f)
                        .WithEase(Ease.Linear)
                        .Bind(x =>
                        {
                            _enemyHpSlider.value = x / _enemyStatus.maxEnemyHp;
                        }).AddTo(this);
            }).AddTo(_gameSessionDisposable);
        }

        // ShootCoolDownの円のあにめーしょん
        _playerManager.currentShootCoolDown.Subscribe(x =>
        {
            _shootCdCircle.fillAmount = x / _playerManager.shootCoolDown;
        }).AddTo(_gameSessionDisposable);
    }

    private void GameEnd()
    {
        IsGameEnd = true;
        _enemyManager.StopLoop();
        _endBackground.SetActive(true);
    }

    private void ResetGame()
    {
        // _gameSessionDisposable.Clear();
        // _playerManager.ResetPlayer();
        // _enemyManager.ResetEnemy();

        // _startBackground.SetActive(true);
        // _endBackground.SetActive(false);
        // IsGameEnd = true;

        // ごちゃごちゃやるより再読み込みしたほうがいいに決まってるんですわ
        SceneManager.LoadScene(0);
    }

    private void Update()
    {
        // Reticleの位置をカーソルの位置にする
        Vector3 pos = Camera.main.WorldToScreenPoint(_reticle.transform.position);
        _shootCdCircle.rectTransform.parent.position = pos;

        if (IsGameEnd) return;
        _enemyManager.EnemyBehaviour();
    }

    private void OnDestroy()
    {
        _gameSessionDisposable.Dispose();
    }
}
