using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStartPosition : MonoBehaviour
{
    public GameObject player;
    Vector3 posi = new Vector3();
    float centerX = 0;
    float centerZ = 0;
    float radius = 0;

    void Awake()
    {
        if(player == null) player = this.gameObject;
    }
    void Start()
    {
        // リセットボタンの設定
        Button resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
        resetButton.onClick.AddListener(ResetSimulation); // リセットボタンをクリックしたときにResetSimulationを呼び出す
    }
    public void setCenterPosition(float X, float Z, float width, float height)
    {
        centerX = X;
        centerZ = Z;

        //半径のセット
        if(width < height)
        {
            radius = height / 2f / 2f;
        }
        else
        {
            radius = width / 2f / 2f;
        }
    }

    public bool check(int x, int y)
    {
        posi = player.transform.position;
        float px = x / 1000f;
        float pz = y / 1000f;
        float dx = px - centerX;
        float dz = pz - centerZ;
        float distance = Mathf.Sqrt(dx * dx + dz * dz);

        if(distance < radius) //道路が半径の内側にある場合
        {
            posi.x = px;
            posi.z = pz;
            player.transform.position = posi;
            return true;
        }
        else
        {
            return false;
        }
    }

    void ResetSimulation()
    {
        player.transform.position = posi;
    }
}
