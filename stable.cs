using UnityEngine;

public class CreatureGenerator : MonoBehaviour
{
    public float spawnHeight = 5.0f; // Height above the ground where the creature will be spawned

    void Start()
    {
        // Ensure this GameObject has a Rigidbody before adding another
        Rigidbody torsoRb = gameObject.GetComponent<Rigidbody>();
        if (torsoRb == null)
        {
            torsoRb = gameObject.AddComponent<Rigidbody>();
        }

        torsoRb.useGravity = true;

        // Set realistic proportions for the torso
        float torsoWidth = Mathf.Max(Random.Range(0.5f, 2.0f), 1.0f);
        float torsoHeight = Mathf.Max(Random.Range(1.0f, 3.0f), 1.2f);
        float torsoDepth = Mathf.Max(Random.Range(0.5f, 2.0f), 1.0f);

        // Apply dimensions to the torso
        transform.localScale = new Vector3(torsoWidth, torsoHeight, torsoDepth);

        // Position the torso at the spawn height
        transform.position = new Vector3(transform.position.x, spawnHeight, transform.position.z);

        // Generate limbs (legs and arms)
        GenerateLimbs(torsoWidth, torsoHeight, torsoDepth, true); // Legs
        GenerateLimbs(torsoWidth, torsoHeight, torsoDepth, false); // Arms
    }

    void GenerateLimbs(float torsoWidth, float torsoHeight, float torsoDepth, bool isLeg)
    {
        // Determine the number of limbs (always 2 for bipedal creatures)
        int limbCount = 2;

        // Define limb colors (one for each limb, four in total)
        Color[] limbColors = { Color.red, Color.green, Color.blue, Color.yellow };

        for (int i = 0; i < limbCount; i++)
        {
            // Select color for the limb
            // Use a different index depending on whether it's an arm or leg
            int colorIndex = isLeg ? i : i + 2;
            Color limbColor = limbColors[colorIndex];

            // Generate up to three limb parts
            Rigidbody prevLimbRb = GetComponent<Rigidbody>();
            Vector3 prevLimbEndPos = transform.position;
            for (int j = 0; j < 3; j++)
            {
                GameObject limbPart = GameObject.CreatePrimitive(PrimitiveType.Cube);

                // Set realistic proportions for the limb parts
                float limbPartWidth = Random.Range(0.3f, 0.6f);
                float limbPartHeight = Random.Range(1.0f, 2.0f);
                float limbPartDepth = Random.Range(0.3f, 0.6f);
                limbPart.transform.localScale = new Vector3(limbPartWidth, limbPartHeight, limbPartDepth);

                // Calculate limb part's position
                float posX = (i % 2 == 0) ? -(torsoWidth / 2 + limbPartWidth / 2) : (torsoWidth / 2 + limbPartWidth / 2);
                float posY = prevLimbEndPos.y - (prevLimbRb == GetComponent<Rigidbody>() ? (isLeg ? torsoHeight / 2 : 0) : limbPartHeight);
                float posZ = isLeg ? 0 : (i % 2 == 0 ? -(torsoDepth / 2 + limbPartDepth / 2) : (torsoDepth / 2 + limbPartDepth / 2)); // Centered on the side for arms
                limbPart.transform.position = new Vector3(posX, posY, posZ);

                // Update prevLimbEndPos for the next iteration
                prevLimbEndPos = limbPart.transform.position - new Vector3(0, limbPartHeight / 2, 0);

                // Add Rigidbody to the limb part
                Rigidbody limbPartRb = limbPart.AddComponent<Rigidbody>();

                // Set the color for the limb part
                limbPart.GetComponent<Renderer>().material.color = limbColor;

                // Create HingeJoint to connect the limb part to the torso or previous limb part
                HingeJoint joint = limbPart.AddComponent<HingeJoint>();
                joint.connectedBody = prevLimbRb;
                joint.axis = new Vector3(1, 0, 0);
                joint.anchor = new Vector3(0, limbPartHeight / 2, 0);

                // Configure the motor of the joint
                JointMotor motor = joint.motor;
                motor.force = Random.Range(200, 400);
                motor.targetVelocity = Random.Range(-100, 100);
                motor.freeSpin = false;
                joint.motor = motor;
                joint.useMotor = true;

                // Update prevLimbRb for the next iteration
                prevLimbRb = limbPartRb;
            }
        }
    }


}
