using UnityEngine;

namespace Side_Logic.GoapActionsFeedBuck
{
    public class ChestSteal : MonoBehaviour
    {

        private Animator _animator;
        public void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public void FeedBuckHouseSteal() => _animator.SetTrigger("open");
    }
}
