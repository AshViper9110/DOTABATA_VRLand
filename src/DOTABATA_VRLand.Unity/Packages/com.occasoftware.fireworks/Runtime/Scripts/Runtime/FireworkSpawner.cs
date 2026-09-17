using System.Collections.Generic;

using UnityEngine;

namespace OccaSoftware.Fireworks.Runtime
{
    public class FireworkSpawner : MonoBehaviour
    {
        public List<GameObject> visualEffects = new List<GameObject>();

        public float spawnRadius = 20f;
        public float spawnRate = 2f;

        float randomizedRate;
        float timeTracker;

        void Start()
        {
            timeTracker = Time.time;
            randomizedRate = spawnRate * Random.Range(1f, 2f);
        }

        void Update()
        {
            if (Time.time - timeTracker > randomizedRate)
            {
                Spawn();
                timeTracker = Time.time;
                randomizedRate = spawnRate * Random.Range(1f, 2f);
            }
        }

        void Spawn()
        {
            GameObject go = visualEffects[Random.Range(0, visualEffects.Count)];

            // XZ 平面でランダムに半径 spawnRadius の位置を作る
            Vector2 circle = Random.insideUnitCircle * spawnRadius;

            // スクリプトが付いているオブジェクトの周囲にスポーン
            Vector3 spawnPos = transform.position + new Vector3(circle.x, 0, circle.y);

            Instantiate(go, spawnPos, Quaternion.identity);
        }
    }
}
