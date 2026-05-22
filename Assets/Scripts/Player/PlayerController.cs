using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // =========================================================
    // COMPONENTS
    // =========================================================

    [Header("Components")]
    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer sr;

    // =========================================================
    // MOVEMENT
    // =========================================================

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 14f;

    [Header("Gravity")]
    public float gravityScale = 3f;
    public float fallMultiplier = 2.5f;

    // =========================================================
    // DASH
    // =========================================================

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;

    bool isDashing;
    float dashTimer;
    float dashCooldownTimer;

    // =========================================================
    // GROUND
    // =========================================================

    [Header("Ground")]
    public Transform groundCheck;
    public float groundRadius = 0.15f;
    public LayerMask groundLayer;

    bool isGrounded;

    // =========================================================
    // COMBAT
    // =========================================================

    [Header("Combat")]
    public BoxCollider2D hitBox;
    public LayerMask enemyLayer;

    [Header("Damage")]
    public int combo1Damage = 5;
    public int combo2Damage = 12;
    public int combo3Damage = 25;
    public int airAttackDamage = 18;

    [Header("Knockback")]
    public float combo3Knockback = 8f;
    public float airAttackKnockback = 10f;

    [Header("Combo")]
    public float comboResetTime = 0.8f;

    int comboStep;
    float comboResetTimer;

    bool isAttacking;
    bool inputBuffered;
    bool isAirAttack;

    Coroutine attackRoutine;

    // =========================================================
    // INPUT
    // =========================================================

    float move;
    bool jump;
    bool dash;

    float facing = 1f;

    // =========================================================
    // UNITY
    // =========================================================

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = gravityScale;
    }

    void Update()
    {
        ReadInput();

        GroundCheck();

        HandleJump();
        HandleDash();
        HandleMove();

        HandleBetterFall();

        HandleAnimation();

        HandleComboReset();
    }

    // =========================================================
    // INPUT
    // =========================================================

    void ReadInput()
    {
        move = Input.GetAxisRaw("Horizontal");

        jump = Input.GetKeyDown(KeyCode.Space);

        dash = Input.GetKeyDown(KeyCode.LeftControl);

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (isAttacking)
            {
                inputBuffered = true;
            }
            else
            {
                StartAttack();
            }
        }
    }

    // =========================================================
    // MOVE
    // =========================================================

    void HandleMove()
    {
        if (isDashing)
            return;

        // ground combo khóa di chuyển
        if (isAttacking && !isAirAttack)
            return;

        rb.velocity = new Vector2(
            move * moveSpeed,
            rb.velocity.y
        );

        if (move != 0)
            facing = Mathf.Sign(move);

        sr.flipX = facing < 0;
    }

    // =========================================================
    // JUMP
    // =========================================================

    void HandleJump()
    {
        if (jump && isGrounded && !isDashing)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                jumpForce
            );
        }
    }

    // =========================================================
    // BETTER FALL
    // =========================================================

    void HandleBetterFall()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up *
                Physics2D.gravity.y *
                (fallMultiplier - 1) *
                Time.deltaTime;
        }
    }

    // =========================================================
    // DASH
    // =========================================================

    void HandleDash()
    {
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        if (
            dash &&
            !isDashing &&
            !isAttacking &&
            dashCooldownTimer <= 0
        )
        {
            StartDash();
        }

        if (!isDashing)
            return;

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0)
            EndDash();
    }

    void StartDash()
    {
        isDashing = true;

        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        rb.gravityScale = 0;

        float dir =
            move != 0
            ? Mathf.Sign(move)
            : facing;

        rb.velocity = new Vector2(
            dir * dashSpeed,
            0
        );

        anim.SetBool("IsDashing", true);
    }

    void EndDash()
    {
        isDashing = false;

        rb.gravityScale = gravityScale;

        anim.SetBool("IsDashing", false);
    }

    // =========================================================
    // ATTACK
    // =========================================================

    void StartAttack()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        isAirAttack = !isGrounded;

        // AIR ATTACK không cộng combo
        if (!isAirAttack)
        {
            comboStep++;

            if (comboStep > 3)
                comboStep = 1;

            comboResetTimer = comboResetTime;
        }

        attackRoutine = StartCoroutine(
            AttackRoutine()
        );
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // =====================================================
        // AIR ATTACK
        // =====================================================

        if (isAirAttack)
        {
            anim.SetTrigger("AirAttack");

            rb.velocity = new Vector2(
                facing * 6f,
                2f
            );

            yield return new WaitForSeconds(0.28f);

            DealDamage();

            yield return new WaitForSeconds(0.18f);
        }

        // =====================================================
        // GROUND COMBO
        // =====================================================

        else
        {
            anim.SetInteger("Combo", comboStep);
            anim.SetTrigger("Attack");

            yield return new WaitForSeconds(0.07f);

            DealDamage();

            yield return new WaitForSeconds(
                GetAttackDuration(comboStep)
            );
        }

        EndAttack();

        // combo buffer
        if (inputBuffered)
        {
            inputBuffered = false;
            StartAttack();
        }
    }

    void EndAttack()
    {
        isAttacking = false;
        isAirAttack = false;
    }

    float GetAttackDuration(int step)
    {
        switch (step)
        {
            case 1:
                return 0.15f;

            case 2:
                return 0.20f;

            case 3:
                return 0.28f;
        }

        return 0.15f;
    }

    // =========================================================
    // DAMAGE
    // =========================================================

    //void DealDamage()
    //{
    //    if (hitBox == null)
    //        return;

    //    Collider2D[] hits =
    //        Physics2D.OverlapBoxAll(
    //            hitBox.bounds.center,
    //            hitBox.bounds.size,
    //            0,
    //            enemyLayer
    //        );

    //    foreach (Collider2D hit in hits)
    //    {
    //        EnemyHealth enemy =
    //            hit.GetComponent<EnemyHealth>();

    //        if (enemy == null)
    //            continue;

    //        int damage = GetDamage();

    //        enemy.TakeDamage(damage);

    //        Debug.Log("Hit Enemy: " + damage);

    //        Vector2 hitPos = hit.ClosestPoint(hitBox.bounds.center);

    //        // =================================================
    //        // COMBO 2 = STUN
    //        // =================================================

    //        if (!isAirAttack && comboStep == 2)
    //        {

    //            Vector2 dir = (enemy.transform.position - transform.position).normalized;
    //            dir.y = 1f;
    //            enemy.ApplyFreeze(1.2f);
    //        }

    //        // =================================================
    //        // COMBO 3 = HEAVY KNOCKBACK
    //        // =================================================

    //        if (!isAirAttack && comboStep == 3)
    //        {
    //            Vector2 dir =
    //                (
    //                    enemy.transform.position -
    //                    transform.position
    //                ).normalized;
    //            enemy.ApplyShock(0.35f);
    //        }

    //        // =================================================
    //        // AIR ATTACK = LAUNCH
    //        // =================================================

    //        if (isAirAttack)
    //        {
    //            Vector2 dir =
    //                (
    //                    enemy.transform.position -
    //                    transform.position
    //                ).normalized;

    //            dir.y = 1f;

    //            enemy.ApplyKnockback(
    //                dir.normalized,
    //                airAttackKnockback
    //            );
    //        }
    //    }
    //}
    void DealDamage()
    {
        if (hitBox == null)
            return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            hitBox.bounds.center,
            hitBox.bounds.size,
            0,
            enemyLayer
        );

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null) continue;

            int damage = GetDamage();

            enemy.TakeDamage(damage);

            Debug.Log("Hit Enemy: " + damage);

            // =================================================
            // HIT POSITION (nếu sau này cần VFX chính xác)
            // =================================================
            Vector2 hitPos = hit.ClosestPoint(hitBox.bounds.center);

            // =================================================
            // COMBO 2 = FREEZE (STUN CONTROL)
            // =================================================
            if (!isAirAttack && comboStep == 2)
            {
                enemy.ApplyFreeze(1.2f);
            }

            // =================================================
            // COMBO 3 = SHOCK (HEAVY CONTROL)
            // =================================================
            if (!isAirAttack && comboStep == 3)
            {
                enemy.ApplyShock(0.35f);
            }            
        }
    }
    int GetDamage()
    {
        if (isAirAttack)
            return airAttackDamage;

        switch (comboStep)
        {
            case 1:
                return combo1Damage;

            case 2:
                return combo2Damage;

            case 3:
                return combo3Damage;
        }

        return combo1Damage;
    }

    // =========================================================
    // COMBO RESET
    // =========================================================

    void HandleComboReset()
    {
        if (comboStep == 0)
            return;

        comboResetTimer -= Time.deltaTime;

        if (comboResetTimer <= 0)
        {
            comboStep = 0;
        }
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    void HandleAnimation()
    {
        anim.SetFloat(
            "Speed",
            Mathf.Abs(rb.velocity.x)
        );

        anim.SetFloat(
            "YVelocity",
            rb.velocity.y
        );

        anim.SetBool(
            "IsGrounded",
            isGrounded
        );
    }

    // =========================================================
    // GROUND CHECK
    // =========================================================

    void GroundCheck()
    {
        isGrounded =
            Physics2D.OverlapCircle(
                groundCheck.position,
                groundRadius,
                groundLayer
            );
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    void OnDrawGizmosSelected()
    {
        if (hitBox == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(
            hitBox.bounds.center,
            hitBox.bounds.size
        );
    }
}