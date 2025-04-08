using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceSiren : MonoBehaviour {

    public GameObject blueLight, redLight;
    public bool isSirenOn;
    public float colorInterval;
    private float timer;
    private MeshRenderer mr;
    private Shader defShader, unlitShader;

    private void Start() {
        mr = GetComponent<MeshRenderer>();
        defShader = Shader.Find("Standard");
        unlitShader = Shader.Find("Unlit/Color");
    }

    private void Update() {
        if (isSirenOn) {
            if(timer > colorInterval) {
                // index 3 : blue, index 4 : red
                bool isBlueUnlit = mr.materials[3].shader == unlitShader;

                blueLight.SetActive(!isBlueUnlit);
                redLight.SetActive(isBlueUnlit);

                mr.materials[3].shader = isBlueUnlit ? defShader : unlitShader;
                mr.materials[4].shader = isBlueUnlit ? unlitShader : defShader;

                timer = 0;
            }

            timer += Time.deltaTime;
        }
    }
}
