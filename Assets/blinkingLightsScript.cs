using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class blinkingLightsScript : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable[] arrows;
    public KMSelectable submit, play;
    public GameObject line;
    public GameObject codeLight;
    public TextMesh screenText;

    private float[] freqs = new float[] { 3.505f, 3.515f, 3.522f, 3.532f, 3.535f, 3.542f, 3.545f, 3.552f, 3.555f, 3.565f, 3.572f, 3.575f, 3.582f, 3.592f, 3.595f, 3.600f };
    private float[] linePos = new float[] { -0.2837f, -0.2206f, -0.1719f, -0.1079f, -0.0925f, -0.0426f, -0.0257f, 0.0188f, 0.0342f, 0.099f, 0.1485f, 0.1639f, 0.2117f, 0.2749f, 0.2936f, 0.3242f };

    private string[] musicNames = new string[]
    {
        "New Super Mario Bros. - Castle Theme",//BPM 124
        "Better Call Saul Intro",//BPM 86
        "Franz Schubert - Serenade",//BPM 132
        "Keep Talking and Nobody Explodes OST - SMILEYFACE",//BPM 110
        "Plants Vs. Zombies OST - Watery Graves (Horde)",//BPM 107
        "Cass Elliot - Make Your Own Kind Of Music",//BPM 120
        "Michael Jackson - Earth Song",//BPM 138
        "Maon Kurosaki - DEAD OR LIE",//BPM 188
        "La Marseillaise (French National Anthem)",//BPM 115
        "Dave James & Keith Beauvais - Class Act",//BPM 172
        "Rhythm Heaven Fever OST - Exhibition Match",//BPM 120
        "Lost OST - Hollywood And Vines",//BPM 110
        "TLoZ: A Link To The Past - Hyrule Castle",//BPM 132
        "TLoZ: Spirit Tracks OST - Realm Overworld",//BPM 138
        "Jamiroquai - Virtual Insanity",//BPM 92
        "Mii Channel Theme"//BPM 114
    };
    private int correctClip = 0;
    private int playPresses = 0;

    private int selected = 0;
    bool isAnimating, isPlaying;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleActivated, inputMode, moduleSolved; // Some helpful booleans

    void Awake()
    {
        moduleId = moduleIdCounter++;

        for (int i = 0; i < arrows.Length; i++)
        {
            int j = i;
            arrows[i].OnInteract += () => { optionSwitch(j); return false; };
        }
        submit.OnInteract += () => { submitOption(); return false; };
        play.OnInteract += () => { playCode(); return false; };

        //module.OnActivate += SomeFunctionAfterBombActivates; 
    }

    void Start()
    {
        float scalar = transform.lossyScale.x;
        codeLight.GetComponent<Light>().range *= scalar;
        codeLight.SetActive(false);

        correctClip = UnityEngine.Random.Range(0, musicNames.Length);
        Debug.LogFormat("[Blinking Lights #{0}]: Module initiated, selected note sequence: {1} ", moduleId, musicNames[correctClip]);
        Debug.LogFormat("[Blinking Lights #{0}]: The starting frequency is {1}.", moduleId, freqs[correctClip].ToString("0.000") + " MHz");

    }

    void optionSwitch(int k)
    {
        if (moduleSolved) { return; }
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (k == 0)//Left
        {
            if (selected != 0)
                selected--;
        }
        else //Right
        {
            if (selected != 15)
                selected++;
        }
        screenText.text = freqs[selected].ToString("0.000") + " MHz";
        if (isAnimating)
            StopCoroutine("lineMove");
        StartCoroutine("lineMove");
    }

    void playCode()
    {
        if (moduleSolved || isPlaying) { return; }
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        playPresses++;
        Debug.LogFormat("<Blinking Lights #{0}>: Play button pressed, sequence played for time #{1}", moduleId, playPresses);
        StartCoroutine("codeFlash");
    }

    void submitOption()
    {
        if (moduleSolved) { return; }
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("[Blinking Lights #{0}]: Submit button pressed, with {1} being the number of times the sequence is played.", moduleId, playPresses);
        Debug.LogFormat("[Blinking Lights #{0}]: The correct frequency to submit is {1}.", moduleId, freqs[(correctClip + playPresses) % freqs.Length].ToString("0.000") + " MHz");
        if (selected == (correctClip + playPresses) % freqs.Length)
        {
            Debug.LogFormat("[Blinking Lights #{0}]: Correct frequency submitted! Module solved!", moduleId);
            module.HandlePass();
            moduleSolved = true;
            if (isPlaying)
            {
                StopCoroutine("codeFlash");
                codeLight.SetActive(false);
            }
        }
        else
        {
            Debug.LogFormat("[Blinking Lights #{0}]: Wrong frequency submitted ({1})! Strike!", moduleId, freqs[selected].ToString("0.000") + " MHz");
            module.HandleStrike();
            playPresses = 0;
            Debug.LogFormat("[Blinking Lights #{0}]: Play button presses are now reset back to zero.", moduleId);
        }
    }

    IEnumerator lineMove()
    {
        isAnimating = true;
        float x = line.transform.localPosition.x;
        float current = x;
        while (current < linePos[selected])
        {
            current += 0.002f;
            line.transform.localPosition = new Vector3(current, 0.55f, 0.15125f);
            yield return null;
        }
        while (current > linePos[selected])
        {
            current -= 0.002f;
            line.transform.localPosition = new Vector3(current, 0.55f, 0.15125f);
            yield return null;
        }
        isAnimating = false;
    }

    private int[] bpm = new int[] { 124, 86, 132, 110, 107, 120, 138, 188, 115, 172, 120, 110, 132, 138, 94, 114 };
    private float[][] beatSeq = new float[][]//Note to self, time period in seconds per beat = 60 / BPM
    {
        new float[]{1.5f, 0.5f, 0.5f, 0.5f, 2f, 1f, 2f, 1f, 3f, 1.5f, 0.5f, 0.5f, 0.5f, 2f, 1f, 2f, 1f, 2f},
        new float[]{2.5f, 0.5f, 0.25f, 0.25f, 0.5f, 2.5f, 0.5f, 0.25f, 0.75f, 2f},
        new float[]{0.67f, 0.67f, 0.66f, 3f, 1f, 0.67f, 0.67f, 0.66f, 3f, 1f, 3f, 1f, 0.67f, 0.67f, 0.66f, 4f},
        new float[]{0.67f, 0.67f, 0.66f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.67f, 0.67f, 0.66f},
        new float[]{0.5f, 2.5f, 0.5f, 0.5f, 0.5f, 2.5f, 0.5f, 0.5f, 0.5f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f, 0.23f, 0.44f, 0.67f, 2.66f},
        new float[]{2f, 0.5f, 1f, 1f, 0.5f, 1f, 1f, 2f, 0.5f, 1f, 1f, 0.5f, 1f, 1f},
        new float[]{1f, 1f, 2f, 0.5f, 0.5f, 1f, 1f, 0.5f, 1.5f, 4f, 1f, 1f, 2f, 0.5f, 0.5f, 1f, 1f, 0.5f, 1.5f, 4f},
        new float[]{1f, 1f, 0.5f, 0.5f, 1f, 1f, 1f, 1.5f, 0.5f, 1f, 1f, 2.5f, 1.5f, 1f, 1.5f, 1.5f, 1f, 1.5f, 1.5f, 1.5f, 1f, 1f, 1f, 2f, 2f},
        new float[]{0.75f, 0.25f, 1f, 1f, 1f, 1f, 1.75f, 0.25f, 0.75f, 0.25f, 0.75f, 0.25f, 1f, 2f, 0.75f, 0.25f, 3f},
        new float[]{1f, 0.5f, 0.5f, 1f, 0.5f, 0.5f, 0.5f, 0.5f, 1f, 0.5f, 0.5f, 0.5f, 0.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0.5f, 1f, 0.5f, 0.5f, 0.5f, 0.5f, 1f, 0.5f, 0.5f, 0.5f, 0.5f, 1f, 1f, 1f, 0.34f, 0.33f, 0.33f, 1f, 1f, 1f, 1f},
        new float[]{1f, 0.75f, 1.25f, 0.5f, 1f, 1.5f, 1f, 0.75f, 1.25f, 0.5f, 1f, 1.5f},
        new float[]{0.5f, 1.5f, 0.25f, 0.25f, 0.5f, 2f, 2f, 0.25f, 0.25f, 0.5f, 0.5f, 5f, 0.5f, 1f},
        new float[]{2f, 3.5f, 0.5f, 0.5f, 1f, 0.25f, 0.25f, 2f, 1.5f, 1f, 1f, 0.25f, 0.25f, 0.5f, 0.5f, 0.25f, 0.25f, 0.25f, 0.25f},
        new float[]{1f, 1f, 2f, 0.67f, 0.67f, 0.66f, 2f, 1f, 1f, 2.67f, 0.67f, 0.66f, 2f, 1f, 1f, 2f, 0.67f, 0.67f, 0.66f, 2f, 1f, 1f, 4f},
        new float[]{0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.5f, 0.75f, 0.25f, 0.5f, 0.75f, 0.5f, 0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.5f, 0.5f, 0.75f, 0.25f, 0.75f, 0.5f},
        new float[]{0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 1.5f, 0.5f, 1f},
    };

    IEnumerator codeFlash()
    {
        isPlaying = true;
        for (int i = 0; i < beatSeq[correctClip].Length; i++)
        {
            if (correctClip == 5 && i == 0)
                yield return new WaitForSeconds(60f / bpm[correctClip] * 1f);
            if (correctClip == 7 && i == 0)
                yield return new WaitForSeconds(60f / bpm[correctClip] * 1f);
            if (correctClip == 7 && i == 0)
                yield return new WaitForSeconds(60f / bpm[correctClip] * 0.75f);

            codeLight.SetActive(true);
            yield return new WaitForSeconds(60f / bpm[correctClip] * 0.2f);
            //yield return new WaitForSeconds(60f / bpm[correctClip] * (beatSeq[correctClip][i] - 0.2f));
            codeLight.SetActive(false);
            yield return new WaitForSeconds(60f / bpm[correctClip] * (beatSeq[correctClip][i] - 0.2f));
            //yield return new WaitForSeconds(60f / bpm[correctClip] * 0.2f);

            if (correctClip == 3 && i == 4)
                yield return new WaitForSeconds(60f / bpm[correctClip] * 3.5f);
            if (correctClip == 4 && i == 8)
                yield return new WaitForSeconds(60f / bpm[correctClip] * 0.25f);
            if (correctClip == 5 && i == 6)
                yield return new WaitForSeconds(60f / bpm[correctClip] * 1f);
            if (correctClip == 6 && i == 9)
                yield return new WaitForSeconds(60f / bpm[correctClip] * 2f);
            if (correctClip == 7 && i == 1)
                yield return new WaitForSeconds(60f / bpm[correctClip] * 0.5f);
            if (correctClip == 10 && (i == 2 || i == 5 || i == 8 || i == 11))
                yield return new WaitForSeconds(60f / bpm[correctClip] * 1f);
            if (correctClip == 14 && (i == 1 || i == 3 || i == 13 || i == 15))
                yield return new WaitForSeconds(60f / bpm[correctClip] * 1f);
            if (correctClip == 15 && (i == 0 || i == 2 || i == 3 || i == 12 || i == 13))
                yield return new WaitForSeconds(60f / bpm[correctClip] * 0.5f);
            if (correctClip == 15 && i == 7)
                yield return new WaitForSeconds(60f / bpm[correctClip] * 2f);

        }
        isPlaying = false;
    }
    
    /*Twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        Match m = Regex.Match(command, @"^()$");
        yield return null;
    }
    */

    /*Force Solve Handler
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
    */
}