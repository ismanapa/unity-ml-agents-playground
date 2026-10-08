using UnityEngine;

namespace UnityMLPlayground.Samples.AvoiderAgent
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float Lifetime = 1.5f;
        // How far past the agent's center the bullet must be to count as dodged
        [SerializeField] private float PassMargin = 1f;

        private AvoiderAgent _agent;
        private bool _passedAgent;

        void Start()
        {
            Destroy(gameObject, Lifetime);
        }

        public void SetAgent(AvoiderAgent agent)
        {
            _agent = agent;
        }

        void FixedUpdate()
        {
            if (_passedAgent || _agent == null) return;

            if (transform.position.x < _agent.transform.position.x - PassMargin)
            {
                _passedAgent = true;
                _agent.BulletDodged(transform);
            }
        }
    }
}
