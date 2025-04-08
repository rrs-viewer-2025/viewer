using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetLight : MonoBehaviour {

    public Light[] lights;
    public bool isOn;

    private void Start() {
        SetLight(isOn);
    }

    public void SetLight(bool isOn) {
        this.isOn = isOn;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        Shader shader = Shader.Find(isOn ? "Unlit/Color" : "Standard");
        mr.materials[1].color = lights[0].color;
        mr.materials[1].shader = shader;

        foreach(Light l in lights) {
            l.gameObject.SetActive(isOn);
        }
    }
}
