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

        // Ensure the torso is of a minimum size
        float torsoWidth = Mathf.Max(Random.Range(0.5f, 1.5f), 1.0f);
        float torsoHeight = Mathf.Max(Random.Range(0.5f, 1.5f), 1.2f);
        float torsoDepth = Mathf.Max(Random.Range(0.5f, 1.5f), 1.0f);

        // Apply random dimensions to the torso
        transform.localScale = new Vector3(torsoWidth, torsoHeight, torsoDepth);

        // Position the torso at the spawn height
        transform.position = new Vector3(transform.position.x, spawnHeight, transform.position.z);

        // Generate the head
        GenerateHead(torsoWidth, torsoHeight);

        // Generate limbs (legs and arms)
        GenerateLimbs(torsoWidth, torsoHeight, torsoDepth, true); // Legs
        GenerateLimbs(torsoWidth, torsoHeight, torsoDepth, false); // Arms
    }

    void GenerateHead(float torsoWidth, float torsoHeight)
    {
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        float headSize = torsoWidth * 0.5f;
        head.transform.localScale = new Vector3(headSize, headSize, headSize);
        head.transform.position = transform.position + new Vector3(0, torsoHeight / 2 + headSize / 2, 0);

        // Add Rigidbody to the head
        Rigidbody headRb = head.AddComponent<Rigidbody>();
        headRb.useGravity = true;
        headRb.mass = 1f; // Adjust as needed

        // Connect the head to the torso with a fixed joint
        FixedJoint headJoint = head.AddComponent<FixedJoint>();
        headJoint.connectedBody = GetComponent<Rigidbody>();
    }


    void GenerateLimbs(float torsoWidth, float torsoHeight, float torsoDepth, bool isLeg)
    {
        // Determine the number of limbs (always 2 for bipedal creatures)
        int limbCount = 2;

        for (int i = 0; i < limbCount; i++)
        {
            GameObject upperLimb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            float upperLimbWidth = Random.Range(0.1f, 0.3f);
            float upperLimbHeight = Random.Range(0.5f, 1.5f);
            float upperLimbDepth = Random.Range(0.1f, 0.3f);
            upperLimb.transform.localScale = new Vector3(upperLimbWidth, upperLimbHeight, upperLimbDepth);

            // Set the upper limb's position relative to the torso
            float posX = (i % 2 == 0) ? -(torsoWidth / 2 + upperLimbWidth / 2) : (torsoWidth / 2 + upperLimbWidth / 2);
            float posY = isLeg ? -(torsoHeight / 2 + upperLimbHeight / 2) : (torsoHeight / 2 + upperLimbHeight / 2);
            float posZ = 0; // Centered on the side
            upperLimb.transform.position = transform.position + new Vector3(posX, posY, posZ);

            // Add Rigidbody to the upper limb
            Rigidbody upperLimbRb = upperLimb.AddComponent<Rigidbody>();

            // Create HingeJoint to connect the upper limb to the torso
            HingeJoint upperJoint = upperLimb.AddComponent<HingeJoint>();
            upperJoint.connectedBody = GetComponent<Rigidbody>();
            upperJoint.axis = new Vector3(1, 0, 0);
            upperJoint.anchor = isLeg ? new Vector3(0, upperLimbHeight / 2, 0) : new Vector3(0, -upperLimbHeight / 2, 0);

            // Configure the motor of the upper joint
            JointMotor upperMotor = upperJoint.motor;
            upperMotor.force = Random.Range(100, 200);
            upperMotor.targetVelocity = Random.Range(-100, 100);
            upperMotor.freeSpin = false;
            upperJoint.motor = upperMotor;
            upperJoint.useMotor = true;

            // Randomly decide whether to add a lower limb
            if (Random.value > 0.7f)
            {
                GameObject lowerLimb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                float lowerLimbWidth = Random.Range(0.1f, 0.3f);
                float lowerLimbHeight = Random.Range(0.5f, 1.5f);
                float lowerLimbDepth = Random.Range(0.1f, 0.3f);
                lowerLimb.transform.localScale = new Vector3(lowerLimbWidth, lowerLimbHeight, lowerLimbDepth);

                // Set the lower limb's position relative to the upper limb
                float lowerLimbPosY = isLeg ? -upperLimbHeight / 2 - lowerLimbHeight / 2 : upperLimbHeight / 2 + lowerLimbHeight / 2;
                lowerLimb.transform.position = upperLimb.transform.position + new Vector3(0, lowerLimbPosY, 0);

                // Add Rigidbody to the lower limb
                Rigidbody lowerLimbRb = lowerLimb.AddComponent<Rigidbody>();

                // Create HingeJoint to connect the lower limb to the upper limb
                HingeJoint lowerJoint = lowerLimb.AddComponent<HingeJoint>();
                lowerJoint.connectedBody = upperLimbRb;
                lowerJoint.axis = new Vector3(1, 0, 0);
                lowerJoint.anchor = isLeg ? new Vector3(0, lowerLimbHeight / 2, 0) : new Vector3(0, -lowerLimbHeight / 2, 0);

                // Configure the motor of the lower joint
                JointMotor lowerMotor = lowerJoint.motor;
                lowerMotor.force = Random.Range(100, 200);
                lowerMotor.targetVelocity = Random.Range(-100, 100);
                lowerMotor.freeSpin = false;
                lowerJoint.motor = lowerMotor;
                lowerJoint.useMotor = true;
            }
        }
    }
}
