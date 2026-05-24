using Assets.Scripts.Enemy;
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    // =========================
    // CORE REF
    // =========================
    public int hp = 99999999;
    public Animator anim;
    public EnemyEffectController fx;
    Transform player;

    // =========================
    // STATUS FLAGS
    // =========================
    bool isSilenced;
    bool isFrozened;
    bool isShocked;

    // =========================
    // FREEZE RESIST
    // =========================
    float freezeResist = 1f;
    float minFreezeMultiplier = 0.2f;

    Coroutine freezeRoutine;
    Coroutine shockRoutine;

    // =========================================================
    // INIT
    // =========================================================
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Awake()
    {
        anim = GetComponent<Animator>();

        fx = GetComponent<EnemyEffectController>();
    }

    // =========================================================
    // UPDATE LOOP
    // =========================================================
    void Update()
    {
        UpdateFreezeResist();

        // nếu bị lock → không chạy AI
        if (IsLocked()) return;

        HandleAI();
    }

    // =========================================================
    // AI
    // =========================================================
    void HandleAI()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > 5f) return;

        transform.localScale =
            (player.position.x > transform.position.x)
            ? Vector3.one
            : new Vector3(-1, 1, 1);
    }

    // =========================================================
    // DAMAGE
    // =========================================================
    public void TakeDamage(int dmg)
    {
        hp -= dmg;

        // bắn event HIT
        EnemyEvents.Hit(this, dmg);

        if (fx != null)
        {
            StartCoroutine(fx.Flash());
        }

        if (hp <= 0)
        {
            EnemyEvents.Death(this);
            Destroy(gameObject);
        }
    }

    // =========================================================
    // LOCK CHECK
    // =========================================================
    bool IsLocked()
    {
        return isFrozened || isShocked || isSilenced;
    }

    // =========================================================
    // FREEZE RESIST GROWTH
    // =========================================================
    void UpdateFreezeResist()
    {
        freezeResist += Time.deltaTime * 0.15f;

        freezeResist = Mathf.Clamp(freezeResist, minFreezeMultiplier, 1f);
    }

    // =========================================================
    // SILENCE (STUN)
    // =========================================================
    public void ApplySilence(float duration)
    {
        isSilenced = true;

        EnemyEvents.Status(this, StatusType.Silence);

        StartCoroutine(SilenceRoutine(duration));
    }

    IEnumerator SilenceRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isSilenced = false;
    }

    // =========================================================
    // FREEZE
    // =========================================================
    public void ApplyFreeze(float duration)
    {
        EnemyEvents.Status(this, StatusType.Freeze);

        float finalDuration = duration * freezeResist;

        if (finalDuration < 0.2f) return;

        freezeResist *= 0.7f;

        freezeResist = Mathf.Clamp(freezeResist, minFreezeMultiplier, 1f);

        if (freezeRoutine != null)
            StopCoroutine(freezeRoutine);

        freezeRoutine = StartCoroutine(FreezeRoutine(finalDuration));
    }

    IEnumerator FreezeRoutine(float duration)
    {
        isFrozened = true;

        if (anim != null)
            anim.speed = 0;

        yield return new WaitForSeconds(duration);

        if (anim != null)
            anim.speed = 1;

        isFrozened = false;
    }

    // =========================================================
    // SHOCK
    // =========================================================
    public void ApplyShock(float duration)
    {
        EnemyEvents.Status(this, StatusType.Shock);

        if (shockRoutine != null)
            StopCoroutine(shockRoutine);

        shockRoutine = StartCoroutine(ShockRoutine(duration));
    }

    IEnumerator ShockRoutine(float duration)
    {
        isShocked = true;

        yield return new WaitForSeconds(duration);

        isShocked = false;
    }
}