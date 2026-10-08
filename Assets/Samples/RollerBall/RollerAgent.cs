using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

namespace UnityMLPlayground.Samples.RollerBall
{
    public class RollerAgent : Agent
    {
        [SerializeField] private Transform Target;
        [SerializeField] private float forceMultiplier = 10;

        private Rigidbody rBody;

        void Start()
        {
            rBody = GetComponent<Rigidbody>();
        }

        public override void OnEpisodeBegin()
        {
            // If the Agent fell, zero its momentum
            if (transform.localPosition.y < 0)
            {
                rBody.angularVelocity = Vector3.zero;
                rBody.linearVelocity = Vector3.zero;
                transform.localPosition = new Vector3(0, 0.5f, 0);
            }

            // Move the target to a new spot
            Target.localPosition = new Vector3(Random.value * 8 - 4,
                                               0.5f,
                                               Random.value * 8 - 4);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Target and Agent positions
            sensor.AddObservation(Target.localPosition);
            sensor.AddObservation(transform.localPosition);

            // Agent velocity
            sensor.AddObservation(rBody.linearVelocity.x);
            sensor.AddObservation(rBody.linearVelocity.z);
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            // Actions, size = 2
            var controlSignal = Vector3.zero;
            controlSignal.x = actionBuffers.ContinuousActions[0];
            controlSignal.z = actionBuffers.ContinuousActions[1];
            rBody.AddForce(controlSignal * forceMultiplier);

            // Rewards
            var distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

            // Reached target
            if (distanceToTarget < 1.42f)
            {
                SetReward(1.0f);
                EndEpisode();
            }

            // Fell off platform
            else if (transform.localPosition.y < 0)
            {
                EndEpisode();
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = 0;
            continuousActionsOut[1] = 0;

            // Safety check
            if (continuousActionsOut.Length < 2)
            {
                Debug.LogError($"ContinuousActions array size is {continuousActionsOut.Length}, but needs to be at least 2. Check Behavior Parameters component.");
                return;
            }

            var keyboard = Keyboard.current;

            if (keyboard != null)
            {
                var move = Vector2.zero;
                if (keyboard.wKey.isPressed) move.y += 1;
                if (keyboard.sKey.isPressed) move.y -= 1;
                if (keyboard.aKey.isPressed) move.x -= 1;
                if (keyboard.dKey.isPressed) move.x += 1;

                continuousActionsOut[0] = move.x;
                continuousActionsOut[1] = move.y;
            }
        }
    }
}
