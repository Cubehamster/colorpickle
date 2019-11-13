using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public AudioClip charging;
    public AudioClip explosion;

    private Texture2D visual;
    private float currentScale;

    private Vector2 position;
    public Vector2 RenderPosition => position;
    public float Radius => visual.width * currentScale * .5f;

    public bool Dead { get; private set; } = true;
    public bool Exploding { get; private set; } = false;

    public float startScale = .001f;
    public float scaleIncreasePerSecond = .005f;
    private float newScale = 1f;

    public float explosionScale = 1.0f;
    public float pointsScaleMultiplier = 1.0f;

    private int score;
    private float multiplier;
    private float multiplierBeforeDeath;

    private float scaleFactor;
    private bool playOnce = true;
    private bool explosionPlayOnce = false;

    private bool startingUp = false;
    public float startupScaleIncreaseMultiplier = 0.024f;

    public float sineAmplitude;
    public float sineFrequency = 1.2f;
    private float sineOffset;

    // Use this for initialization       
    void Awake ()
    {
        visual = Resources.Load<Texture2D>("circle");
    }

    private void Start()
    {
        currentScale = startScale;
        GetComponent<AudioSource>().playOnAwake = false;
    }

    private void OnGUI()
    {
        if ((Input.GetMouseButton(0) || Exploding) && !Dead)
        {
            GUI.DrawTexture(new Rect(RenderPosition.x - Radius, RenderPosition.y - Radius, Radius * 2, Radius * 2), visual);
            GUI.skin.label.fontSize = 60;
            GUI.Label(new Rect(20, 52, 200, 200), $" {multiplier}x");
        }
        else
        {
            GUI.skin.label.fontSize = 60;
            GUI.Label(new Rect(20, 52, 200, 200), $"{multiplierBeforeDeath}x");
        }

        GUI.skin.label.fontSize = 32;

        GUI.Label(new Rect(20, 20, 200, 200), $"Score: {score}");


        if (Dead) GUI.Label(new Rect(Screen.width * .5f - 200.0f, Screen.height * .5f - 15.0f, 400, 200), "Click and hold... Release!");
    }

    private IEnumerator Explode()
    {
        yield return new WaitForSeconds(0.08f);
        Exploding = true;
        multiplierBeforeDeath = multiplier;
        scaleFactor = currentScale / startScale;
        float beginScale = currentScale;
        float targetScale = currentScale * Mathf.Pow(scaleFactor,0.8f) * explosionScale;
        float p = .0f;

        while(p < 1.0f)
        {
            p += Time.deltaTime * 5.0f;
            currentScale = Mathf.Lerp(beginScale, targetScale, p);

            yield return 0;
        }

        int hits = EnemyManager.Instance.Explosion(RenderPosition, Radius);

        score += Mathf.RoundToInt(hits * multiplierBeforeDeath);

        Exploding = false;
    }

    private void SineOffset()
    {
        sineOffset = sineAmplitude * Mathf.Sin((Time.time * sineFrequency) * 2.0f * Mathf.PI);
    }

    public void Die()
    {
        Dead = true;
    }

    // Update is called once per frame
    void Update ()
    {
        SineOffset();
        Debug.Log(sineOffset);

        if (Exploding) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (playOnce)
            {
                GetComponent<AudioSource>().clip = charging;
                GetComponent<AudioSource>().Play();
                currentScale = startScale;
                StartCoroutine(Startup());
                playOnce = false;
                explosionPlayOnce = true;
            }
            if (Dead)
            {
                Dead = false;
                score = 0;

                EnemyManager.Instance.StartSpawning();
            }
        }
        else if (Input.GetMouseButton(0) && !Dead)
        {
            if (startingUp)
            {
                currentScale += scaleIncreasePerSecond * startupScaleIncreaseMultiplier * Mathf.Pow(Time.deltaTime, 0.4f);
            }
            else
            {
                currentScale += scaleIncreasePerSecond * Mathf.Pow(Time.deltaTime, 0.89f) + sineOffset;
            }


            Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

            position = new Vector2(mousePos.x, mousePos.y);
        }
        else if (Input.GetMouseButtonUp(0) && !Dead)
        {
            StartCoroutine(Explode());
        }
        else
        {
            playOnce = true;
            if (explosionPlayOnce)
            {
                GetComponent<AudioSource>().Stop();
                explosionPlayOnce = false;
                GetComponent<AudioSource>().clip = explosion;
                GetComponent<AudioSource>().Play();

            }
        }
        multiplier = Mathf.Round(Mathf.Pow(currentScale/newScale, 2.74f) * pointsScaleMultiplier);
    }

    IEnumerator Startup()
    {
        yield return new WaitForSeconds(0.19f);
        startingUp = true;
        yield return new WaitForSeconds(1.67f);
        newScale = currentScale;
        startingUp = false;
    }
}
