using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class netPositiveScript : MonoBehaviour {

    public KMNeedyModule Needy;
    public KMAudio Audio;

    public GameObject WholeNet;
    public GameObject NetPivot;
    public GameObject[] Squares;
    public KMSelectable[] SquareSels;
    public Light[] SquareLights;
    public GameObject[] LightEncaps;
    public TextMesh[] TPTexts;

    const float SPACING = 0.0225f;
    bool[] state = { false, false, false, false, false, false, false };
    bool TwitchPlaysActive;
    string[] tpLetters = { "", "", "", "", "", "", "" };
    string[] possibleLetters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved = true;

    void Awake() {
        moduleId = moduleIdCounter++;
        Needy.OnNeedyActivation += OnNeedyActivation;
        Needy.OnNeedyDeactivation += OnNeedyDeactivation;
        Needy.OnTimerExpired += OnTimerExpired;

        foreach (KMSelectable sel in SquareSels) {
            sel.OnInteract += delegate () { SquarePress(sel); return false; };
        }
    }

    // Use this for initialization
    void Start() {
        moduleSolved = true;

        var scalar = transform.lossyScale.x; //standard light procedure: all lights must be scaled based on the scale of the bomb
        for (int l = 0; l < 7; l++) {
            SquareLights[l].range *= scalar;
            LightEncaps[l].SetActive(false);
        }
	}

    void OnNeedyActivation() {
        moduleSolved = false;
        for (int l = 0; l < 7; l++) {
            LightEncaps[l].SetActive(false);
            state[l] = false;
        }
        string[] NETS =
        {
            " y  |rGo |rW  |rBOg|  Yg| bRg|  w ",
            "  y  |  Go |yRW  |  BOg| rY  |  g  ",
            "  y  |  G  |yRWOy|  B  | rYo |  g  ",
            " y   |rG   |rWO  |  BYg|   Rg|   w ",
            " y   |rG   |rWO  |  BYg| wR  |  g  ",
            " y   |rG g |rWOYr|  B  | wRy |  g  ",
            " yy  |bRGo |  Wo | rB  | rYOw|  gg ",
            " yy  |bRGo |  W  | rBOg|   Yg|   r ",
            " yy  |bRGo |  W  | rBOg| rY  |  g  ",
            " yy  |bRG  |  WOy| rB  | rYo |  g  ",
            " yyy |bRGOb|  W  | rBo | rYo |  g  "
        };
        int chosenIx = Rnd.Range(0, NETS.Length);
        string[] chosenNet = NETS[chosenIx].Split('|');
        int spaceWidth = chosenIx == 0 ? 4 : 5;
        int spaceHeight = chosenIx == 0 ? 7 : 6;
        int extraY;
        int extraX;
        do {
            extraY = Rnd.Range(0, spaceHeight);
            extraX = Rnd.Range(0, spaceWidth);
        } while (!Regex.IsMatch(chosenNet[extraY][extraX].ToString(), @"[a-z]"));
        //Debug.LogFormat("extra {0}:{1}{2}", chosenIx, extraY, extraX);
        int matchY = -1; //shut up compiler i know what i am doing >:(
        int matchX = -1;
        int[] wrongsY = { -1, -1, -1, -1, -1 };
        int[] wrongsX = { -1, -1, -1, -1, -1 };
        int wrongsFound = 0;
        string[][] loggedNet = new string[spaceHeight][];
        for (int h = 0; h < spaceHeight; h++)
        {
            loggedNet[h] = new string[spaceWidth];
        }
        int minY = int.MaxValue;
        int maxY = int.MinValue;
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        if (TwitchPlaysActive) {
            possibleLetters = possibleLetters.Shuffle();
            for (int s = 0; s < 7; s++) {
                tpLetters[s] = possibleLetters[s];
                TPTexts[s].text = tpLetters[s];
            }
        }
        for (int r = 0; r < spaceHeight; r++) {
            for (int k = 0; k < spaceWidth; k++) {
                string sq = chosenNet[r][k].ToString();
                if (Regex.IsMatch(sq, @"[A-Z]")) {
                    if (chosenNet[extraY][extraX].ToString().ToUpper() == sq) {
                        matchY = r;
                        matchX = k;
                        //Debug.LogFormat("match {0}:{1}{2}", chosenIx, r, k);
                        loggedNet[r][k] = TwitchPlaysActive ? "{"+tpLetters[0]+"}" : "{}"; //note: I tried string interpolation here $"{{{tpLetters[0]}}}" but that's a C# 4.0 thing; feel free to submit a PR if you want that used as much as I do
                        //bound adjust
                            if (k < minX) { minX = k; }
                            if (k > maxX) { maxX = k; }
                            if (r < minY) { minY = r; }
                            if (r > maxY) { maxY = r; }
                    } else {
                        wrongsY[wrongsFound] = r;
                        wrongsX[wrongsFound] = k;
                        //Debug.LogFormat("wrong{0} {1}:{2}{3}", wrongsFound, chosenIx, r, k);
                        loggedNet[r][k] = TwitchPlaysActive ? "["+tpLetters[wrongsFound+2]+"]" : "[]";
                        wrongsFound++;
                        //bound adjust
                            if (k < minX) { minX = k; }
                            if (k > maxX) { maxX = k; }
                            if (r < minY) { minY = r; }
                            if (r > maxY) { maxY = r; }
                    }
                } else {
                    if (r == extraY && k == extraX) {
                        loggedNet[r][k] = TwitchPlaysActive ? "{"+tpLetters[1]+"}" : "{}";
                        //bound adjust
                            if (k < minX) { minX = k; }
                            if (k > maxX) { maxX = k; }
                            if (r < minY) { minY = r; }
                            if (r > maxY) { maxY = r; }
                    } else {
                        loggedNet[r][k] = TwitchPlaysActive ? "___" : "__";
                    }
                }
            }
        }
        Debug.LogFormat("[Net Positive #{0}] Given net:", moduleId);
        for (int r = spaceHeight - 1; r > -1; r--) { //the reason for this strange looking loop is because on TP if I didn't do this it'd be vertically flipped
            Debug.LogFormat("[Net Positive #{0}] {1}", moduleId, loggedNet[r].Join());
        }
        //Debug.LogFormat("xs {0},{1}", minX, maxX);
        //Debug.LogFormat("ys {0},{1}", minY, maxY);

        Squares[0].transform.localPosition = new Vector3(SPACING*(matchX-minX), 0.0148f, SPACING*(matchY-minY));
        Squares[1].transform.localPosition = new Vector3(SPACING*(extraX-minX), 0.0148f, SPACING*(extraY-minY));
        for (int b = 0; b < 5; b++) {
            Squares[b+2].transform.localPosition = new Vector3(SPACING*(wrongsX[b]-minX), 0.0148f, SPACING*(wrongsY[b]-minY));
        }

        NetPivot.transform.localPosition = new Vector3(-SPACING*(maxX-minX-1)/2-SPACING/2, 0f, (-SPACING*(maxY-minY-1)/2)-SPACING/2);
        if (!TwitchPlaysActive) {
            WholeNet.transform.localRotation = Quaternion.Euler(0f, (maxY - minY == 5) ? 90f + 180f*Rnd.Range(0, 2) : 90f*Rnd.Range(0, 4), 0f);
            WholeNet.transform.localScale = new Vector3(Rnd.Range(0, 1) == 0 ? 1f : -1f, 1f, Rnd.Range(0, 1) == 0 ? 1f : -1f);   
        }

        //i would *love* to be informed on why i can't put a void in a void like i can put a function inside of a function in js ._.
        /*
        void BoundAdjust(int x, int y) {
            if (x < minX) { minX = x; }
            if (x > maxX) { maxX = x; }
            if (y < minY) { minY = y; }
            if (y > maxY) { maxY = y; }
        }
        */
    }

    void SquarePress(KMSelectable S) {
        Audio.PlaySoundAtTransform("click", S.transform);
        S.AddInteractionPunch(0.2f);
        if (moduleSolved) { return; }
        for (int W = 0; W < 7; W++) {
            if (S == SquareSels[W]) {
                state[W] = !state[W];
                LightEncaps[W].SetActive(state[W]);
            }   
        }
    }

    void OnNeedyDeactivation() {
        moduleSolved = true;
        
    }

    void OnTimerExpired() {
        moduleSolved = true;
        for (int s = 0; s < 7; s++) {
            if (state[s] != s < 2) {
                Debug.LogFormat("[Net Positive #{0}] Strike!", moduleId);
                Debug.LogFormat("<Net Positive #{0}> Index {1} was {2}lit", moduleId, s, state[s] ? "" : "un");
                Needy.HandleStrike();
                break;
            }
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} XY [Toggles lights of the buttons with letters given]";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command) {
        if (moduleSolved) {
            yield return "sendtochaterror The needy isn't active right now!";
            yield break;
        }
        //make the command all caps to it isn't case-sensitive, and remove all spaces
        command = command.Split(' ').Join().ToUpper();
        //if any character isn't an english letter, return a 'you may only submit letters' error
        if (Regex.IsMatch(command, @"[^A-Z]")) {
            yield return "sendtochaterror The command is only allowed to have letters!";
            yield break;
        }
        //if letter is given that is not in tpLetters, return a 'there is no L letter' error
        for (int ch = 0; ch < command.Length; ch++) {
            if (Array.IndexOf(tpLetters, command[ch].ToString()) == -1) {
                yield return "sendtochaterror The command has a letter not in the net!";
                yield break;
            }
        }

        yield return null;
        yield return "strike";
        yield return "solve";

        //if neither of the above apply, then press every button whose letters were given in the command in that order
        for (int ch = 0; ch < command.Length; ch++) {
            SquareSels[Array.IndexOf(tpLetters, command[ch].ToString())].OnInteract();
            yield return new WaitForSeconds(.1f);
        }
    }

    private void TwitchHandleForcedSolve()
    {
        StartCoroutine(THFS());
    }

    private IEnumerator THFS() {
        for (int s = 0; s < 7; s++) {
            if (state[s] != s < 2) {
                SquareSels[s].OnInteract();
                yield return new WaitForSeconds(.1f);
            }
        }
    }
}