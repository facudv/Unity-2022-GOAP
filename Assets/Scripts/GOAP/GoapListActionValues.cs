using UnityEngine;

namespace GOAP
{
    [RequireComponent(typeof(Planner))]
    public class GoapListActionValues : MonoBehaviour
    {
        public const int iDamageAfterJump = 75;
    
        public const int iHealAmount = 100;
    
        public const float fMoneyCanStealChest = 25.5f;
        public const float fMoneyGainStealChest = 75.5f;

        public const float fMoneyCanBuyAntiFog = 50.5f;
        public const float fMoneyLossBuyAntiFog = 50.5f;
    
    
    }
}
