using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private Texture2D triangle;
    private Texture2D circle;

    private Vector2 position;
    private Vector2 direction;
    public float Speed { get; set; }

    public Vector2 RenderPosition => position;
    public float Radius => 25.0f * scale;

    private float scale = 1.0f;

    private void Awake()
    {
        triangle = Resources.Load<Texture2D>("triangle");
        circle = Resources.Load<Texture2D>("circle");
    }

    // Use this for initialization
    void Start()
    {
        int randomEdge = Random.Range(0, 4);
        
        if(randomEdge == 0)
        {
            position = new Vector2(Random.Range(0, Screen.width), -50);
            direction = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(.0f, 1.0f));
        }
        else if(randomEdge == 1)
        {
            position = new Vector2(Screen.width + 50.0f, Random.Range(.0f, Screen.height));
            direction = new Vector2(Random.Range(-1.0f, .0f), Random.Range(-1.0f, 1.0f));
        }
        else if(randomEdge == 2)
        {
            position = new Vector2(Random.Range(0, Screen.width), Screen.height + 50.0f);
            direction = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, .0f));
        }
        else if(randomEdge == 3)
        {
            position = new Vector2(-50.0f, Random.Range(.0f, Screen.height));
            direction = new Vector2(Random.Range(.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        }
    }

    public void Explode()
    {
        StartCoroutine(ShowExplode());
    }

    private IEnumerator ShowExplode()
    {
        float p = 0;

        float startScale = 1.0f;
        float targetScale = 2.0f;

        while(p < 1.0f)
        {
            p += Time.deltaTime * 5.0f;

            scale = Mathf.Lerp(startScale, targetScale, p);

            yield return 0;
        }

        Destroy(gameObject);
    }

    private void OnGUI()
    {
        if(Input.GetMouseButton(0))
        {
            var playerPos = GameObject.Find("Player").GetComponent<Player>().RenderPosition;

            float dx = playerPos.x - RenderPosition.x;
            float dy = playerPos.y - RenderPosition.y;
            float a = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg + 90.0f;

            GUIUtility.RotateAroundPivot(a, RenderPosition);

            GUI.DrawTexture(new Rect(RenderPosition.x - Radius, RenderPosition.y - Radius, Radius * 2, Radius * 2), triangle);

            GUI.matrix = Matrix4x4.identity;
        }
        else
        {
            GUI.DrawTexture(new Rect(RenderPosition.x - Radius, RenderPosition.y - Radius, Radius * 2, Radius * 2), circle);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (scale > 1.00001f) return;

        position += direction * Speed * Time.deltaTime;

        if(!(new Rect(-100, -100, Screen.width + 200, Screen.height + 200).Contains(position)))
        {
            EnemyManager.Instance.KillEnemy(this);
        }
    }
}
