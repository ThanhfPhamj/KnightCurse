using Assets.Scripts.Enemy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEvents : MonoBehaviour
{
    public static event Action<EnemyHealth, int> OnHit;
    public static event Action<EnemyHealth, StatusType> OnStatus;
    public static event Action<EnemyHealth> OnDeath;

    // =========================
    // Bắn event khi bị hit
    // =========================
    public static void Hit(EnemyHealth enemy, int dmg)
    {
        OnHit?.Invoke(enemy, dmg);
    }

    // =========================
    // Bắn event khi bị status
    // =========================
    public static void Status(EnemyHealth enemy, StatusType type)
    {
        OnStatus?.Invoke(enemy, type);
    }

    public static void Death(EnemyHealth enemy)
    {
        OnDeath?.Invoke(enemy);
    }
}
