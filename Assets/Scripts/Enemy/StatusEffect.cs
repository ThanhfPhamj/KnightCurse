
using System.Collections.Generic;
namespace Assets.Scripts.Enemy
{
    public enum StatusType
    {
        Stun,
        Shock,
        Freeze,
        Silence
    }

    [System.Serializable]
    public class StatusEffect
    {

        public StatusType type;
        public float duration;
        public float timer;
        public StatusEffect(StatusType type, float duration)
        {
            this.type = type;
            this.duration = duration;
        }
    }
}
