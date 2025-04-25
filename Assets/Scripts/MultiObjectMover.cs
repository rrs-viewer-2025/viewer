using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndividualMover : MonoBehaviour
{
    public GameObject prefab;
    public int count = 10;
    public float speed = 3f;

    private class MovingObj
    {
        public GameObject obj;
        public List<Vector3> path;
        public int index;
    }

    private List<MovingObj> movers = new List<MovingObj>();

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 start = new Vector3(i * 2f, 0, 0);
            GameObject obj = Instantiate(prefab, start, Quaternion.identity);

            // 各オブジェクトにランダムな経由地点（3点）
            List<Vector3> path = new List<Vector3> {
                start,
                start + new Vector3(Random.Range(-2, 2), 0, Random.Range(3, 6)),
                start + new Vector3(Random.Range(3, 6), 0, Random.Range(8, 12))
            };

            movers.Add(new MovingObj { obj = obj, path = path, index = 0 });
        }

        StartCoroutine(MoveAll());
    }

    IEnumerator MoveAll()
    {
        while (movers.Count > 0)
        {
            for (int i = movers.Count - 1; i >= 0; i--)
            {
                var m = movers[i];
                if (m.index >= m.path.Count - 1)
                {
                    movers.RemoveAt(i);
                    continue;
                }

                Vector3 target = m.path[m.index + 1];
                Vector3 dir = (target - m.obj.transform.position).normalized;

                if (dir != Vector3.zero)
                    m.obj.transform.rotation = Quaternion.LookRotation(dir);

                m.obj.transform.position = Vector3.MoveTowards(m.obj.transform.position, target, speed * Time.deltaTime);

                if (Vector3.Distance(m.obj.transform.position, target) < 0.1f)
                    m.index++;
            }

            yield return null;
        }

        Debug.Log("全オブジェクトの移動完了！");
    }
}
