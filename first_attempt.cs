using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CreatureGenerator : Agent
{
    public float spawnHeight = 5.0f;
    public PhysicMaterial creaturePhysicsMaterial;
    public LayerMask groundLayer;

    private GameObject targetCube; // The target cube
    private Rigidbody torsoRb;
    private HingeJoint[] joints;
    private Vector3[] limbPositions;
    private float[] motorInputValues;

    public override void Initialize()
    {
        InitializeTorso();
        InitializeLimbPositions();
        joints = new HingeJoint[12]; // 4 limbs with up to 3 parts each
        motorInputValues = new float[12]; // Motor inputs for each joint
    }

    public override void OnEpisodeBegin()
    {
        GenerateMotorInputValues();
        GenerateLimbs();
        CreateTargetCube(); // Create and position the target cube
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Torso information
        sensor.AddObservation(torsoRb.transform.localPosition);
        sensor.AddObservation(torsoRb.transform.localRotation);
        sensor.AddObservation(torsoRb.velocity);
        sensor.AddObservation(torsoRb.angularVelocity);

        // Ground detection
        RaycastHit hit;
        if (Physics.Raycast(torsoRb.position, Vector3.down, out hit, 10f, groundLayer))
        {
            sensor.AddObservation(hit.distance); // Distance to the ground
        }
        else
        {
            sensor.AddObservation(10f); // No ground detected
        }

        // Joint information
        foreach (var joint in joints)
        {
            if (joint != null)
            {
                sensor.AddObservation(joint.angle);
                sensor.AddObservation(joint.velocity);
            }
            else
            {
                sensor.AddObservation(0f); // Joint is missing
                sensor.AddObservation(0f); // Joint is missing
            }
        }

        // Add target cube position as an observation
        if (targetCube != null)
        {
            sensor.AddObservation(transform.InverseTransformPoint(targetCube.transform.position));
        }
        else
        {
            sensor.AddObservation(Vector3.zero); // No target cube
        }
    }

    private void InitializeTorso()
    {
        torsoRb = GetComponent<Rigidbody>();
        if (torsoRb == null)
        {
            torsoRb = gameObject.AddComponent<Rigidbody>();
        }
        torsoRb.useGravity = true;
        torsoRb.constraints = RigidbodyConstraints.FreezeRotation;

        float torsoWidth = Random.Range(1.0f, 1.5f);
        float torsoHeight = Random.Range(1.5f, 2.0f);
        float torsoDepth = Random.Range(1.0f, 1.5f);

        transform.localScale = new Vector3(torsoWidth, torsoHeight, torsoDepth);
        transform.position = new Vector3(transform.position.x, spawnHeight + torsoHeight / 2, transform.position.z);

        gameObject.AddComponent<BoxCollider>().material = creaturePhysicsMaterial;
    }

    private void InitializeLimbPositions()
    {
        limbPositions = new Vector3[]
        {
            new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0),
            new Vector3(0, 0.5f, -0.5f), new Vector3(0, 0.5f, 0.5f),
            new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, -0.5f),
            new Vector3(-0.5f, 0, 0.5f), new Vector3(0.5f, 0, 0.5f),
            new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0),
            new Vector3(0, -0.5f, -0.5f), new Vector3(0, -0.5f, 0.5f)
        };
    }

    private void GenerateMotorInputValues()
    {
        for (int i = 0; i < motorInputValues.Length; i++)
        {
            motorInputValues[i] = Random.Range(-1f, 1f);
        }
    }

    private void GenerateLimbs()
    {
        ClearExistingLimbs();

        int limbCount = Random.Range(1, 5); // Random number of limbs
        for (int i = 0; i < limbCount; i++)
        {
            int positionIndex = Random.Range(0, limbPositions.Length);
            Vector3 limbPosition = limbPositions[positionIndex];
            Color limbColor = Random.ColorHSV();
            GenerateLimb(limbPosition, limbColor, i * 3); // Generate limb with a starting index for its joints
        }
    }

    private void GenerateLimb(Vector3 attachmentPoint, Color limbColor, int jointStartIndex)
    {
        int subpartCount = Random.Range(2, 4); // Random number of subparts
        Rigidbody prevRb = torsoRb;

        for (int j = 0; j < subpartCount; j++)
        {
            GameObject limbPart = CreateLimbPart(limbColor, attachmentPoint, prevRb, jointStartIndex + j);
            prevRb = limbPart.GetComponent<Rigidbody>();
        }
    }

    private GameObject CreateLimbPart(Color color, Vector3 attachmentPoint, Rigidbody connectedBody, int jointIndex)
    {
        GameObject limbPart = GameObject.CreatePrimitive(PrimitiveType.Cube);
        limbPart.transform.SetParent(transform);

        float partLength = Random.Range(0.5f, 1.0f);
        limbPart.transform.localScale = new Vector3(Random.Range(0.3f, 0.7f), partLength, Random.Range(0.3f, 0.7f));
        limbPart.transform.position = transform.TransformPoint(attachmentPoint) + transform.up * partLength / 2;
        limbPart.GetComponent<Renderer>().material.color = color;

        Rigidbody limbPartRb = limbPart.AddComponent<Rigidbody>();
        limbPartRb.mass = Random.Range(0.5f, 2f);

        HingeJoint joint = limbPart.AddComponent<HingeJoint>();
        joint.connectedBody = connectedBody;
        joint.axis = Random.onUnitSphere;
        joint.useMotor = true;

        JointMotor motor = new JointMotor
        {
            targetVelocity = motorInputValues[jointIndex] * 50f, // Use pre-generated motor value
            force = 1000
        };
        joint.motor = motor;

        joints[jointIndex] = joint;

        return limbPart;
    }

    private void ClearExistingLimbs()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateTargetCube()
    {
        if (targetCube != null)
        {
            Destroy(targetCube);
        }

        // Create a new cube and position it above the floor
        targetCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        targetCube.transform.localScale = new Vector3(1f, 1f, 1f);

        // Define a local offset for the cube within a circular area above the agent
        float radius = 5f; // Define the radius around the agent
        Vector2 randomPoint = Random.insideUnitCircle * radius;
        float spawnHeightAboveFloor = 3f; // Height above the floor to spawn the cube
        Vector3 localOffset = new Vector3(randomPoint.x, spawnHeightAboveFloor, randomPoint.y);

        // Convert the local offset to a global position
        Vector3 globalPosition = transform.TransformPoint(localOffset);
        targetCube.transform.position = globalPosition;

        // Add a Rigidbody to the cube so it is affected by physics
        Rigidbody cubeRb = targetCube.AddComponent<Rigidbody>();
        cubeRb.useGravity = true; // Ensure gravity is enabled
    }



    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i] != null)
            {
                var motor = joints[i].motor;
                motor.targetVelocity = motorInputValues[i] * 50f;
                motor.force = 1000;
                joints[i].motor = motor;
            }
        }

        if (targetCube != null)
        {
            float distanceToTarget = Vector3.Distance(this.transform.position, targetCube.transform.position);
            AddReward(-distanceToTarget * 0.001f); // Reward for being closer to the target

            if (distanceToTarget < 1f) // Close enough to the target
            {
                AddReward(1.0f); // Reward for reaching the target
                EndEpisode();
                CreateTargetCube(); // Spawn a new target
            }
        }

        AddReward(-0.001f); // Small penalty for each step to encourage efficiency
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        for (int i = 0; i < continuousActions.Length; i++)
        {
            continuousActions[i] = Random.Range(-1f, 1f);
        }
    }
}
