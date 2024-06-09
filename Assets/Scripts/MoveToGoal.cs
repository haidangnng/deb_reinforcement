using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class MoveToGoal : Agent {
    [SerializeField] private List<Transform> targets;
    [SerializeField] private Rigidbody rigidbody;
    // [SerializeField] private Transform target;
    [SerializeField] private Material targetMat;
    [SerializeField] private Material winMat;
    [SerializeField] private Material loseMat;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    [SerializeField] public float targetTime = 120.0f;
    public Transform parentTransform;
    public float throttleIncrement = 1f;
    public float maxThrust = 400f;
    public float responsiveness = 0.5f;

    private float throttle = 20f;
    public int numberOfTransforms = 5;
    private float ResponseModifier {
        get {
            return rigidbody.mass * responsiveness / 10f;
        }
    }

    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void DestroyTargets() {
        foreach (var target in targets)
        {
            Destroy(target.gameObject); 
        }
        targets.Clear();
    }

    void GenerateRandomTransforms()
    {
        for (int i = 0; i < numberOfTransforms; i++)
        {
            // Create a new GameObject
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = "RandomTransform_" + i;

            if (parentTransform != null)
                {
                    obj.transform.SetParent(parentTransform);
                }

            // Set its position to a random value within the bounds
            Vector3 randomPosition = new Vector3(
                UnityEngine.Random.Range(-400f, 400f),
                UnityEngine.Random.Range(150f, 400f),
                UnityEngine.Random.Range(-400f, 400f)
            );
            obj.transform.localPosition = randomPosition;

            // Set its scale to a random value within the bounds
            Vector3 randomScale = new Vector3(
                40f,
                40f,
                40f
            );
            obj.transform.localScale = randomScale;

            // Assign the material if provided
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (targetMat != null)
                {
                    renderer.material = targetMat;
                }
                else
                {
                    Debug.LogWarning("Object material is not assigned in the Inspector.");
                }
            }
            else
            {
                Debug.LogError("Renderer component not found on the GameObject.");
            }

            // Assign the tag
            obj.tag = "collectible";

            // Add the Transform to the list
            targets.Add(obj.transform);
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0f, 250f, 0f);
        GenerateRandomTransforms();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Pitch");
        continuousActions[1] = Input.GetAxis("Roll");
        continuousActions[2] = Input.GetAxis("Yaw");
        continuousActions[3] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        continuousActions[4] = Input.GetKey(KeyCode.LeftControl) ? 1 : 0;

    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "collectible") {
            AddReward(5f);
            Destroy(other.gameObject); 
            targets.Remove(other.transform);
            targetTime = 120.0f;
        }

        if (targets.Count == 0)
        {
            AddReward(20f);
            floorMeshRenderer.material = winMat;
            EndEpisode();
        }

        if (other.gameObject.tag == "wall") {
            AddReward(-30f);
            DestroyTargets();
            EndEpisode();
            floorMeshRenderer.material = loseMat;
        }
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float pitch = actions.ContinuousActions[0];
        float roll = actions.ContinuousActions[1];
        float yaw = actions.ContinuousActions[2];

        bool accelerate = actions.ContinuousActions[3] > 0;
        bool decelerate = actions.ContinuousActions[4] > 0;

        if (pitch != 0f) {
            rigidbody.AddTorque(pitch * ResponseModifier * transform.right);
        }
        if (roll != 0f) {
            rigidbody.AddTorque(ResponseModifier * roll * transform.forward);
        }
        if (yaw != 0f) {
            rigidbody.AddTorque(ResponseModifier * yaw * transform.up);
        }
        rigidbody.AddForce(-transform.up * maxThrust * throttle);
        if (accelerate) {
            throttle += throttleIncrement;
        }
        if (decelerate) {
            throttle -= throttleIncrement;
        }
        throttle = Mathf.Clamp(throttle, 20f, maxThrust);
    }

    private void FixedUpdate() {
        targetTime -= Time.deltaTime;

        if (targetTime <= 0.0f) {
            AddReward(-10f);
            DestroyTargets();
            EndEpisode();
            floorMeshRenderer.material = loseMat;
            targetTime = 120f;
        }
    }
}