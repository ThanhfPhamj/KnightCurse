using UnityEngine;
using Assets.Scripts.Enemy;

public class EnemyEffectController : MonoBehaviour
{
    [Header("Hit Effect")]
    public GameObject hitEffect;
    public Transform effectPoint;

    [Header("Status Points")]
    public Transform headPoint;
    public Transform bodyPoint;

    [Header("Status Effects (World VFX)")]
    public GameObject stunEffect;
    public GameObject shockEffect;
    public GameObject freezeEffect;

    SpriteRenderer sr;
    EnemyHealth enemy;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        enemy = GetComponent<EnemyHealth>();
    }

    void OnEnable()
    {
        EnemyEvents.OnHit += HandleHit;
        EnemyEvents.OnStatus += HandleStatus;
    }

    void OnDisable()
    {
        EnemyEvents.OnHit -= HandleHit;
        EnemyEvents.OnStatus -= HandleStatus;
    }

    // =========================================================
    // HIT EFFECT
    // =========================================================
    void HandleHit(EnemyHealth e, int dmg)
    {
        if (e != enemy) return;

        Spawn(hitEffect, effectPoint);
    }

    // =========================================================
    // STATUS EFFECT
    // =========================================================
    void HandleStatus(EnemyHealth e, StatusType type)
    {
        if (e != enemy) return;

        switch (type)
        {
            case StatusType.Silence:
                Spawn(stunEffect, headPoint);
                break;

            case StatusType.Shock:
                Spawn(shockEffect, bodyPoint);
                break;

            case StatusType.Freeze:
                Spawn(freezeEffect, headPoint);
                break;
        }
    }

    // =========================================================
    // SAFE SPAWN
    // =========================================================
    void Spawn(GameObject prefab, Transform point)
    {
        if (prefab == null || point == null) return;

        GameObject fx = Instantiate(prefab, point.position, Quaternion.identity);

        Destroy(fx, 2f);
    }

    // optional flash
    public System.Collections.IEnumerator Flash()
    {
        if (sr == null) yield break;

        sr.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        sr.color = Color.white;
    }
}