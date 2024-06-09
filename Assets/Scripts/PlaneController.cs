using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    private new Rigidbody rigidbody;
    private PlaneController planeController;
    public float throttleIncrement = 0.1f;
    public float maxThrust = 200f;
    public float responsiveness = 10f;

    private float throttle;
    private float pitch;
    private float yaw;
    private float roll;
    // public float lift = 135f;

    public float YawInput {get; set;}
    public float PitchInput {get; set;}
    public float RollInput {get; set;}
    public bool AccelerateInput {get; set;}
    public bool DecelerateInput {get; set;}


    private float ResponseModifier {
        get {
            return rigidbody.mass * responsiveness / 10f;
        }
    }

    private void Awake() {
        rigidbody = GetComponent<Rigidbody>();
        planeController = GetComponent<PlaneController>();
    }

    private void HandleInputs(){
        roll = Input.GetAxis("Roll");
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");
    
        bool accelerate = Input.GetKey(KeyCode.Space);
        bool decelerate = Input.GetKey(KeyCode.LeftControl);
    
        planeController.PitchInput = pitch;
        planeController.RollInput = roll;
        planeController.YawInput = yaw;
        planeController.AccelerateInput = accelerate;
        planeController.DecelerateInput = decelerate;
    }


    private void FixedUpdate() {
        ProcessActions();
        HandleInputs();
    }

    private void ProcessActions() {
        if (PitchInput != 0f) {
            rigidbody.AddTorque(pitch * ResponseModifier * transform.right);
        }
        if (RollInput != 0f) {
            rigidbody.AddTorque(ResponseModifier * roll * transform.forward);
        }
        if (YawInput != 0f) {
            rigidbody.AddTorque(ResponseModifier * yaw * transform.up);
        }
        rigidbody.AddForce(-transform.up * maxThrust * throttle);
        if (AccelerateInput) {
            throttle += throttleIncrement;
        }
        if (DecelerateInput) {
            throttle -= throttleIncrement;
        }
        throttle = Mathf.Clamp(throttle, 0f, 100f);
    }
}
