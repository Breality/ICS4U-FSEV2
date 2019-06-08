using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyLimitations : MonoBehaviour {

    private Rigidbody rb;
    private Transform t;

    public bool limitX;
    public bool limitY;
    public bool limitZ;

    public Vector3 basePosition;
    public Vector3 minLocalPosition;
    public Vector3 maxLocalPosition;

    public bool limitAngle;
    public float maxLocalAngle;
    public Vector3 limitAngleAxis;

    void Awake() {
        t = GetComponent<Transform>();
    }

    void FixedUpdate() {
        if (rb == null) {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
                return;
        }

        Vector3 correctionTranslation = GetCorrectionVector();
        rb.MovePosition(rb.position + correctionTranslation);

        Quaternion correctionRotation = GetCorrectionAxisRotation(limitAngleAxis);
        rb.MoveRotation(correctionRotation * rb.rotation);
    }

    public Vector3 GetCorrectionVector() {
        Vector3 localPosition = t.localPosition - basePosition;

        float x = limitX ? Clamp(localPosition.x, minLocalPosition.x, maxLocalPosition.x) : localPosition.x;
        float y = limitY ? Clamp(localPosition.y, minLocalPosition.y, maxLocalPosition.y) : localPosition.y;
        float z = limitZ ? Clamp(localPosition.z, minLocalPosition.z, maxLocalPosition.z) : localPosition.z;

        localPosition = new Vector3(x, y, z) + basePosition;

        Vector3 rbPosition;
        if (transform.parent == null)
            rbPosition = localPosition;
        else
            rbPosition = transform.parent.TransformPoint(localPosition);
        Vector3 correctionVector = rbPosition - t.position;
        return correctionVector;
    }

    public Quaternion GetCorrectionRotation() {
        Quaternion localRotation;
        if (transform.parent == null)
            localRotation = t.rotation;
        else
            localRotation = Quaternion.Inverse(transform.parent.rotation) * t.rotation;

        Quaternion clampedRotation = limitAngle ? Quaternion.RotateTowards(Quaternion.identity, localRotation, maxLocalAngle) : localRotation;

        Quaternion rbRotation;
        if (transform.parent == null)
            rbRotation = clampedRotation;
        else
            rbRotation = transform.parent.rotation * clampedRotation;
        Quaternion correctionRotation = rbRotation * Quaternion.Inverse(t.rotation);
        return correctionRotation;
    }

    public Quaternion GetCorrectionAxisRotation(Vector3 axis) {
        Quaternion localRotation;
        if (transform.parent == null)
            localRotation = t.rotation;
        else
            localRotation = Quaternion.Inverse(transform.parent.rotation) * t.rotation;

        Quaternion twist = GetTwist(localRotation, axis);

        Quaternion clampedTwist = limitAngle ? Quaternion.RotateTowards(Quaternion.identity, twist, maxLocalAngle) : localRotation;

        Quaternion clampedRotation = clampedTwist;

        Quaternion rbRotation;
        if (transform.parent == null)
            rbRotation = clampedRotation;
        else
            rbRotation = transform.parent.rotation * clampedRotation;
        Quaternion correctionRotation = rbRotation * Quaternion.Inverse(t.rotation);
        return correctionRotation;
    }

    public static Quaternion GetTwist(Quaternion rotation, Vector3 axis) {
        Vector3 ra = new Vector3(rotation.x, rotation.y, rotation.z); // rotation axis
        Vector3 p = Vector3.Project(ra, axis); // return projection v1 on to v2  (parallel component)
        Quaternion twist = new Quaternion(p.x, p.y, p.z, rotation.w);
        Quaternion normalizedTwist = Normalize(twist);
        return normalizedTwist;
    }

    public static Quaternion GetSwing(Quaternion rotation, Quaternion twist) {
        Quaternion swing = rotation * Conjugated(twist);
        return swing;
    }

    public static Quaternion GetSwing(Quaternion rotation, Vector3 axis) {
        Quaternion twist = GetTwist(rotation, axis);
        Quaternion swing = GetSwing(rotation, twist);
        return swing;
    }
    /**
       Decompose the rotation on to 2 parts.
       1. Twist - rotation around the "direction" vector
       2. Swing - rotation around axis that is perpendicular to "direction" vector
       The rotation can be composed back by 
       rotation = swing * twist

       has singularity in case of swing_rotation close to 180 degrees rotation.
       if the input quaternion is of non-unit length, the outputs are non-unit as well
       otherwise, outputs are both unit
    */
    public static void SwingTwistDecomposition(Quaternion rotation,
                                        Vector3 direction,
                                        out Quaternion swing,
                                        out Quaternion twist) {
        Vector3 ra = new Vector3(rotation.x, rotation.y, rotation.z ); // rotation axis
        Vector3 p = Vector3.Project(ra, direction); // return projection v1 on to v2  (parallel component)
        twist = new Quaternion(p.x, p.y, p.z, rotation.w);
        twist = Normalize(twist);

        swing = rotation * Conjugated(twist);
    }

    static Quaternion Normalize(Quaternion q) {
        float length = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        float scale = 1.0f / length;
        Quaternion q1 = new Quaternion(q.x * scale, q.y * scale, q.z * scale, q.w * scale);
        return q1;
    }

    static Quaternion Conjugated(Quaternion q) {
        Quaternion q1 = new Quaternion(-q.x, -q.y, -q.z, q.w);
        return q1;
    }

private float Clamp(float x, float min, float max) {
        float hysteresis = (max - min) * 0.1F;
        if (x < min) {
            if (!minReached) {
                minReached = true;
                OnMinReached();
            }
            return min;
        }
        else if (x > min + hysteresis) { // 0.9 for hysteresis
            minReached = false;
        }
        if (x > max) {
            if (!maxReached) {
                maxReached = true;
                OnMaxReached();
            }
            return max;
        }
        else if (x < max - hysteresis) { // 0.9 for hysteresis
            maxReached = false;
        }
        return x;
    }

    #region Events
    public delegate void LimitationEvent();

    public event LimitationEvent MinReached;
    public event LimitationEvent MaxReached;

    public enum EventTriggerType {
        MinReached,
        MaxReached
    };
    private bool minReached = false;
    private bool maxReached = false;

    [System.Serializable]
    public class TriggerEvent : UnityEvent<float> { }

    [System.Serializable]
    public class Entry {
        public EventTriggerType eventID;
        public TriggerEvent callback;
    }

    [SerializeField]
    public List<Entry> m_Delegates;

    private void Execute(EventTriggerType id) {
        for (int i = 0, imax = m_Delegates.Count; i < imax; ++i) {
            var ent = m_Delegates[i];
            if (ent.eventID == id && ent.callback != null)
                ent.callback.Invoke(0);
        }
    }

    public virtual void OnMinReached() {
        if (MinReached != null)
            MinReached();
        Execute(EventTriggerType.MinReached);
    }

    public virtual void OnMaxReached() {
        if (MaxReached != null)
            MaxReached();
        Execute(EventTriggerType.MaxReached);
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected() {
        Gizmos.color = new Color(1, 0.4F, 0); // Passer orange
        if (limitX) {
            Gizmos.color = Color.red;
            Vector3 basePositionX = new Vector3(basePosition.x, transform.localPosition.y, transform.localPosition.z);
            Vector3 minPosition = basePositionX + new Vector3(minLocalPosition.x, 0, 0);
            Vector3 maxPosition = basePositionX + new Vector3(maxLocalPosition.x, 0, 0);
            DrawRange(minPosition, maxPosition);
        }
        if (limitY) {
            Gizmos.color = Color.green;
            Vector3 basePositionY = new Vector3(transform.localPosition.x, basePosition.y, transform.localPosition.z);
            Vector3 minPosition = basePositionY + new Vector3(0, minLocalPosition.y, 0);
            Vector3 maxPosition = basePositionY + new Vector3(0, maxLocalPosition.y, 0);
            DrawRange(minPosition, maxPosition);
        }
        if (limitZ) {
            Gizmos.color = Color.blue;
            Vector3 basePositionZ = new Vector3(transform.localPosition.x, transform.localPosition.y, basePosition.z);
            Vector3 minPosition = basePositionZ + new Vector3(0, 0, minLocalPosition.z);
            Vector3 maxPosition = basePositionZ + new Vector3(0, 0, maxLocalPosition.z);
            DrawRange(minPosition, maxPosition);
        }
    }

    private void DrawRange(Vector3 minPosition, Vector3 maxPosition) {
        if (transform.parent != null) {
            minPosition = transform.parent.TransformPoint(minPosition);
            maxPosition = transform.parent.TransformPoint(maxPosition);
        }
        Gizmos.DrawLine(minPosition, maxPosition);
        Gizmos.DrawSphere(minPosition, 0.005F);
        Gizmos.DrawSphere(maxPosition, 0.005F);
    }


    #endregion
}
