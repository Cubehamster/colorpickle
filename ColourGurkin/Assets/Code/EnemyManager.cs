using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    private List<Enemy> enemies = new List<Enemy>();

    public float spawnTimeMin = .5f;
    public float spawnTimeMax = 2.0f;

    public int spawnAmountMin = 2;
    public int spawnAmountMax = 10;

    public float enemySpeedMin = 15.0f;
    public float enemySpeedMax = 50.0f;

    private void Awake()
    {
        Instance = this;
    }

    public void StartSpawning()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(spawnTimeMin, spawnTimeMax));

            for (int i = 0; i < Random.Range(spawnAmountMin, spawnAmountMax); ++i)
            {
                var enemy = new GameObject("Enemy");
                enemy.AddComponent<Enemy>();
                enemy.GetComponent<Enemy>().Speed = Random.Range(enemySpeedMin, enemySpeedMax);

                enemies.Add(enemy.GetComponent<Enemy>());
            }
        }
    }

    public void KillEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
        Destroy(enemy.gameObject);
    }

    public int Explosion(Vector2 position, float radius)
    {
        int hits = 0;
        for (int i = enemies.Count - 1; i >= 0; --i)
        {
            var enemy = enemies[i];

            float dx = position.x - enemy.RenderPosition.x;
            float dy = position.y - enemy.RenderPosition.y;
            float d = Mathf.Sqrt(dx * dx + dy * dy);

            if ((d - 20) < radius)
            {
                enemy.Explode();

                ++hits;

                enemies.RemoveAt(i);
            }
        }
        return hits;
    }

    public void Reset()
    {
        StopAllCoroutines();

        enemies.ForEach(e => Destroy(e.gameObject));
        enemies.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        var player = GameObject.Find("Player").GetComponent<Player>();
        if(Input.GetMouseButton(0) && !player.Exploding && !player.Dead)
        {
            foreach (var enemy in enemies)
            {
                float dx = player.RenderPosition.x - enemy.RenderPosition.x;
                float dy = player.RenderPosition.y - enemy.RenderPosition.y;
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                if((d - 20.0f) < player.Radius)
                {
                    player.Die();
                    Reset();
                    break;
                }
            }
        }
    }
}
