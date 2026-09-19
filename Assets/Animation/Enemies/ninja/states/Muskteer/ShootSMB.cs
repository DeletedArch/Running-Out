using System.Buffers.Text;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class ShootSMB : EnemyStateBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] Vector2 fireOffset = new Vector2(0.8f, 0.8f);

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        Context.enemyMovement?.Stop();

        // calculate the spawn point based on facing direction
        int facing = Context.enemyMovement != null ? Context.enemyMovement.FacingDirection : 1;
        Vector2 spawnPos = (Vector2)Controller.transform.position + new Vector2(fireOffset.x * facing, fireOffset.y);

        // Direction from gun tip to the locked aim position
        Vector2 targetPos = Context.AimLockedPosition;
        Vector2 direction = (targetPos - spawnPos).normalized;

        // Spawn and fire the bullet
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        MusketeerBullet bullet = bulletObj.GetComponent<MusketeerBullet>();
        if (bullet != null)
        {
            bullet.Setup(direction);
        }
    }
}
