using UnityEngine;

namespace UnityMLPlayground.Samples.AvoiderAgent
{
    public class Cannon : MonoBehaviour
    {
        [SerializeField] private AvoiderAgent Agent;
        [SerializeField] private Bullet BulletPrefab;
        [SerializeField] private float ShootInterval = 1f;
        [SerializeField] private float BulletSpeed = 10f;

        private float _timeSinceLastShot;
        private Bullet _activeBullet;

        public void ResetCannon()
        {
            if (_activeBullet != null)
            {
                // Destroy only happens at the end of the frame, so deactivate the bullet
                // right away to keep it from hitting the agent again in the meantime
                _activeBullet.gameObject.SetActive(false);
                Destroy(_activeBullet.gameObject);
            }
            _activeBullet = null;
            _timeSinceLastShot = 0f;
        }

        void FixedUpdate()
        {
            _timeSinceLastShot += Time.fixedDeltaTime;

            if (_timeSinceLastShot >= ShootInterval)
            {
                Shoot();
                _timeSinceLastShot = 0f;
            }
        }

        void Shoot()
        {
            _activeBullet = Instantiate(BulletPrefab, transform.position, Quaternion.identity, transform.parent);
            _activeBullet.SetAgent(Agent);

            var rb = _activeBullet.GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.left * BulletSpeed;

            Agent.TrackBullet(_activeBullet.transform);
        }
    }
}
