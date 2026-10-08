using UnityEngine;

public class CircleBullet : BulletBase
{

    /// <summary>
    /// CircleBullet.cs 丸形弾
    /// 三角に強く、四角に弱い
    /// </summary>

    public override void Update()
    {
        base.Update();
    }

    // 同じ弾なら破壊
    public override void OnCircleBulletHit()
    {
        base.OnCircleBulletHit();
        PlayHitParticle();

        AddComboCount();
        base.Release();
    }

    // 自分に強い弾なら破壊
    public override void OnSquareBulletHit()
    {
        base.OnSquareBulletHit();
        PlayHitParticle();

        ResetComboCount();
        base.Release();
    }

    // 自分に弱い弾ならそのまま
    public override void OnTriangleBulletHit()
    {
        base.OnTriangleBulletHit();
        AddComboCount();
    }
}
