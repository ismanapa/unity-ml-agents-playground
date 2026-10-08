using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityMLPlayground.Samples.AvoiderAgent
{
    public class AvoiderAgent : Agent
    {
        [SerializeField] private Cannon Cannon;
        [SerializeField] private float JumpForce = 10f;
        [SerializeField] private float DodgeReward = 1f;
        [SerializeField] private float HitPenalty = -1f;
        [SerializeField] private float JumpPenalty = -0.1f;
        // Used to scale the bullet distance observation to roughly [-1, 1]
        [SerializeField] private float MaxBulletDistance = 8f;

        private Rigidbody _rb;
        private Transform _trackedBullet;
        private bool _jumpIsReady = true;

        public override void Initialize()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void TrackBullet(Transform bullet)
        {
            _trackedBullet = bullet;
        }

        public void BulletDodged(Transform bullet)
        {
            if (bullet != _trackedBullet) return;

            AddReward(DodgeReward);
            _trackedBullet = null;
        }

        public override void OnEpisodeBegin()
        {
            _rb.angularVelocity = Vector3.zero;
            _rb.linearVelocity = Vector3.zero;
            transform.localPosition = new Vector3(-4, 0.5f, 0);
            transform.localRotation = Quaternion.identity;
            _jumpIsReady = true;
            _trackedBullet = null;

            Cannon.ResetCannon();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            var hasBullet = _trackedBullet != null;
            var bulletDistance = hasBullet
                ? (_trackedBullet.position.x - transform.position.x) / MaxBulletDistance
                : 1f;

            sensor.AddObservation(hasBullet ? 1f : 0f);
            sensor.AddObservation(bulletDistance);
            sensor.AddObservation(transform.localPosition.y);
            sensor.AddObservation(_rb.linearVelocity.y / JumpForce);
            sensor.AddObservation(_jumpIsReady ? 1f : 0f);
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var jump = actionBuffers.DiscreteActions[0] == 1;

            if (jump && _jumpIsReady)
            {
                _rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
                _jumpIsReady = false;
                AddReward(JumpPenalty);
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut[0] = 0;

            var keyboard = Keyboard.current;

            if (keyboard != null && keyboard.spaceKey.isPressed)
            {
                discreteActionsOut[0] = 1;
            }
        }

        void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Floor"))
            {
                _jumpIsReady = true;
            }
        }

        // Bullets are triggers, so they detect the hit without pushing the agent
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Bullet>() != null)
            {
                AddReward(HitPenalty);
                EndEpisode();
            }
        }
    }
}
