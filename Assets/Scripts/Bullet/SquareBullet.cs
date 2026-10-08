using UnityEngine;

public class SquareBullet : BulletBase
{
    public override void OnEnable()
    {
        base.OnEnable();

    }

    public override void Shoot(Transform target, string tag, float bulletSpeed)
    {
        base.Shoot(target, tag, bulletSpeed);

        Vector3 direction = targetTransform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        angle += -90f;
        this.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void OnTriangleBulletHit()
    {
        base.OnTriangleBulletHit();

        PlayHitParticle();
        ResetComboCount();
        base.Release();
    }

    public override void OnSquareBulletHit()
    {
        base.OnSquareBulletHit();

        PlayHitParticle();
        AddComboCount();
        base.Release();
    }

    public override void OnCircleBulletHit()
    {
        base.OnCircleBulletHit();
        AddComboCount();
    }
}
