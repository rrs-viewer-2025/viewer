using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingLights : MonoBehaviour {
    public int windowMaterialIndex;
    public Color lightColor;
    public bool areLightsOn;
    private Color defaultColor;
    private MeshRenderer mr;

    private void Start() {
        mr = GetComponent<MeshRenderer>();
        defaultColor = mr.materials[windowMaterialIndex].color;
        SetLights(areLightsOn);
    }

    public void SetLights(bool isOn) {
        mr.materials[windowMaterialIndex].shader = isOn ? Shader.Find("Unlit/Color") : Shader.Find("Standard");
        mr.materials[windowMaterialIndex].color = isOn ? lightColor : defaultColor;
    }
}
