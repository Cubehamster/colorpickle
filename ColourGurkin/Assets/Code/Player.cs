using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public AudioClip charging;
    public AudioClip explosion;

    public AudioSource chargeSource;
    public AudioSource explosionSource;

    private Texture2D visual;
    private float currentScale;

    private Vector2 position;
    public Vector2 RenderPosition => position;
    public float Radius => visual.width * displayScale * .5f;

    public bool Dead { get; private set; } = true;
    public bool Exploding { get; private set; } = false;

    private float startScale = 0.001f;
    private float scaleIncreasePerSecond = 0.011f;
    private float newScale = 1f;

    private float explosionScale = 3f;
    private float pointsScaleMultiplier = 1.0f;

    private int score;
    private float multiplier;
    private float multiplierBeforeDeath;

    private float scaleFactor;
    private bool playOnce = true;
    private bool explosionPlayOnce = false;

    private bool startingUp = false;
    private float startupScaleIncreaseMultiplier = 0.009f;

    private float sineAmplitude;
    private float sineFrequency = 1.2f;
    private float sineOffset;
    private float displayScale;

    private float timer;
    private float timeoffset;

    // Use this for initialization       
    void Awake ()
    {
        visual = Resources.Load<Texture2D>("circle");
    }

    private void Start()
    {
        currentScale = startScale;
        displayScale = startScale;
        chargeSource.playOnAwake = false;
        explosionSource.playOnAwake = false;        
    }

    private void OnGUI()
    {
        if ((Input.GetMouseButton(0) || Exploding) && !Dead)
        {
            GUI.DrawTexture(new Rect(RenderPosition.x - Radius, RenderPosition.y - Radius, Radius * 2, Radius * 2), visual);
            GUI.skin.label.fontSize = 60;
            GUI.Label(new Rect(20, 52, 200, 200), $"{multiplier}x");
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

        float beginScale = displayScale;
        float targetScale = displayScale * explosionScale;
        
        float p = .0f;

        while(p < 1.0f)
        {
            p += Time.deltaTime * 5.0f;
            displayScale = Mathf.Lerp(beginScale, targetScale, p);

            yield return 0;
        }

        int hits = EnemyManager.Instance.Explosion(RenderPosition, Radius);

        score += Mathf.RoundToInt(hits * multiplierBeforeDeath);

        Exploding = false;
        currentScale = startScale;
        displayScale = startScale;
    }

    private void SineOffset()
    {
        sineAmplitude = 0.075f * Mathf.Pow(currentScale,0.8f);
        sineFrequency = Mathf.Pow(1.48f, -0.23f * (timer - 1.74f));
        sineOffset = sineAmplitude * Mathf.Sin((timer - 1.74f) / sineFrequency * 1.0f * Mathf.PI);
    }

    public void Die()
    {
        Dead = true;
    }
    

    // Update is called once per frame
    void Update ()
    {
        timer = Time.time - timeoffset;

        SineOffset();

        if (Exploding) return;

        if (Input.GetMouseButtonDown(0))
        {

            if (playOnce)
            {
                timeoffset = Time.time;
                chargeSource.clip = charging;
                chargeSource.Play();
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
                currentScale = startupScaleIncreaseMultiplier * Mathf.Pow(2.718f, timer);
                displayScale = currentScale;
            }
            else
            {
                currentScale += scaleIncreasePerSecond * Mathf.Pow(Time.deltaTime, 0.87f);
                displayScale = currentScale + sineOffset;
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
                chargeSource.Stop();
                explosionPlayOnce = false;
                explosionSource.clip = explosion;
                explosionSource.Play();

            }
        }
        //multiplier = Mathf.Round(Mathf.Pow(currentScale / newScale, 2.74f) * pointsScaleMultiplier);
        multiplier = Mathf.Floor(-1f + Mathf.Pow(2.97039f, 0.14899f * (timer)) * 2.4f - 1.3f);
    }

    IEnumerator Startup()
    {
        yield return new WaitForSeconds(0.24f);
        startingUp = true;
        yield return new WaitForSeconds(1.62f);
        newScale = currentScale;
        startingUp = false;
    }
}
