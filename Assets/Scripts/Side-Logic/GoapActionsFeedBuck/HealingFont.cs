using UnityEngine;

namespace Side_Logic.GoapActionsFeedBuck
{
    public class HealingFont : MonoBehaviour
    {
        public ParticleSystem psHealing;

        public void FeedBuckHealing() => psHealing.Play();
    }
}
