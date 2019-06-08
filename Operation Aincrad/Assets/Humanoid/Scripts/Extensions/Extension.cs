using UnityEngine;
using System.Collections;
using Passer;

public class IVR_Controller : MonoBehaviour {

    [HideInInspector]
    protected HumanoidControl humanoid;

    [HideInInspector]
    public Transform target;

    protected Vector3 startPosition;
    public Vector3 GetStartPosition() { return startPosition; }

    protected Quaternion startRotation;
    public Quaternion GetStartRotation() { return startRotation; }

    protected Vector3 controllerPosition = Vector3.zero;
    protected Quaternion controllerRotation = Quaternion.identity;

    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Quaternion rotation;

    protected bool extrapolation = false;

    protected bool present = false;
    public bool isPresent() {
        return present;
    }
    public bool tracking = false;

    [HideInInspector]
    private float updateTime;

    [HideInInspector]
    private Vector3 lastPosition = Vector3.zero;
    [HideInInspector]
    private Quaternion lastRotation = Quaternion.identity;
    private Vector3 positionalVelocity = Vector3.zero;
    private float angularVelocity = 0;
    private Vector3 velocityAxis = Vector3.one;

    void Start() {
        updateTime = Time.time;
    }

    public virtual void StartController(HumanoidControl _humanoid, Transform _target) {
        humanoid = _humanoid;
        target = _target;

        startPosition = target.position - humanoid.transform.position;
        startRotation = Quaternion.Inverse(humanoid.transform.rotation) * target.rotation;

        lastPosition = startPosition;
        lastRotation = startRotation;
    }

    public virtual void OnTargetReset() {
            Calibrate(true);
    }

    public void Calibrate(bool calibrateOrientation) {
    }

    public void TransferCalibration(IVR_Controller lastController) {
    }

    [HideInInspector]
    private bool indirectUpdate = false;

    public virtual void UpdateController() {
        Vector3 localPosition = Vector3.zero;
        Quaternion localRotation = Quaternion.identity;

        localPosition = controllerPosition;
        localRotation = controllerRotation;

        position = humanoid.transform.position + humanoid.transform.rotation * localPosition;
        rotation = humanoid.transform.rotation * localRotation;

        if (extrapolation == false) {
            target.position = position;
            target.rotation = rotation;
        } else {
            float deltaTime = Time.time - updateTime;
            CalculateVelocity(deltaTime);

            if (deltaTime > 0) {
                updateTime = Time.time;
                indirectUpdate = true;
            }
        }
    }

    private void CalculateVelocity(float deltaTime) {
        if (deltaTime > 0) {
            float angle = 0;
            Quaternion rotationalChange = Quaternion.Inverse(lastRotation) * rotation;

            rotationalChange.ToAngleAxis(out angle, out velocityAxis);
            if (angle == 0)
                velocityAxis = Vector3.one;

            positionalVelocity = (position - lastPosition) / deltaTime;
            angularVelocity = angle / deltaTime;

            lastPosition = position;
            lastRotation = rotation;
        }
    }

    void Update() {
        if (indirectUpdate) {
            float dTime = Time.time - updateTime;
            target.position = lastPosition + positionalVelocity * dTime;
            target.rotation = lastRotation * Quaternion.AngleAxis(angularVelocity * dTime, velocityAxis);
        }
    }
}