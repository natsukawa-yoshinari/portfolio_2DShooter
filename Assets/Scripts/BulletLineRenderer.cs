using UnityEngine;

public class BulletLineRenderer : MonoBehaviour
{
    [SerializeField]
    private LineRenderer _lineRenderer;
    [SerializeField]
    private Gradient _lineGradient;

    private Transform _player;
    private GameObject _reticleObj;
    private Reticle _reticle;
    private Vector2 _rayFromPos;

    private void Start()
    {
        _rayFromPos = (Vector2)_player.position + new Vector2(0.5f, 0f);

        _lineRenderer.colorGradient = _lineGradient;
        _lineRenderer.startWidth = 0.1f;
        _lineRenderer.endWidth = 0.1f;
    }

    public void Initialize(Transform player, GameObject reticle)
    {
        _player = player;
        _reticleObj = reticle;
        _reticle = _reticleObj.gameObject.GetComponent<Reticle>();
    }

    private void Update()
    {
        _lineRenderer.SetPosition(0, _rayFromPos);

        // 飛ばすベクトル
        Vector2 to = ((Vector2)_reticleObj.transform.position - _rayFromPos).normalized;

        // 開始からReticleまでのRayの距離取得
        float distance = Vector2.Distance(_rayFromPos, _reticleObj.transform.position);

        RaycastHit2D hit = Physics2D.Raycast(_rayFromPos, to, distance);
        Vector3 mousePos = _reticle.GetMousePosition();

        // Rayが当たった
        if (hit.collider != null)
        {
            _lineRenderer.SetPosition(1, hit.point);

            // カーソル位置取得

            // 衝突地点よりも奥にカーソルあったら
            if (Mathf.Abs(mousePos.x) > Mathf.Abs(hit.point.x))
            {
                // x座標を衝突地点のx座標にする
                Vector3 reticlePos = new Vector3(hit.point.x, mousePos.y);
                _reticleObj.transform.position = reticlePos;
            }
            else
            {
                _reticleObj.transform.position = mousePos;
            }

        }
        // Ray当たらない
        else
        {
            _lineRenderer.SetPosition(1, _reticleObj.transform.position);
            _reticleObj.transform.position = mousePos;
        }

        // if (mousePos.x < _rayFromPos.x)
        // {
        //     mousePos.x = _rayFromPos.x;
        //     _reticle.transform.position = mousePos;
        // }
    }
}
