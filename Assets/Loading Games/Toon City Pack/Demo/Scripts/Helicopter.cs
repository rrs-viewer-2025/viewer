using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : MonoBehaviour {

    public float bladeRotateSpeed;
    public Transform blades;
    public Transform tailBlades;

    private Vector3 startPos;
    private Vector3 tempPos;
    public float hoverFrequency = 1;
    public float hoverAmplitude = .05f;

    private void Start() {
        startPos = transform.position;
    }

    private void Update() {
        Hover();
        RotateBlades();
    }

    private void Hover() {
        tempPos = new Vector3(transform.position.x, startPos.y, transform.position.z);
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * hoverFrequency) * hoverAmplitude;
        transform.position = tempPos;
    }

    private void RotateBlades() {
        blades.Rotate(Vector3.forward * bladeRotateSpeed * Time.deltaTime);
        tailBlades.Rotate(Vector3.right * bladeRotateSpeed * Time.deltaTime);
    }

}
