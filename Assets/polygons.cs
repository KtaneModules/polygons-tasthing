using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class polygons : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo bomb;

    public KMSelectable[] buttons;
    public Mesh[] shapes;
    public MeshFilter[] highlights;
    private bool[] conditions = new bool[35];
    private int trueshape;
    private int[] falseshapes = new int[3];
    private List <int> availablebuttons = new List <int>();
    private static readonly string[] posnames = new string[] { "top", "right", "bottom", "left" };
    private int startingday;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
      moduleId = moduleIdCounter++;
      foreach (KMSelectable button in buttons)
        button.OnInteract += delegate () { buttonPress(button); return false; };
    }

    void Start()
    {
      startingday = (int)DateTime.Now.DayOfWeek;
      getConditions();
      reset();
    }

    void reset()
    {
      trueshape = rnd.Range(0,35);
      var truthix = rnd.Range(0,2);
      var availableconditions = Enumerable.Range(0,35).ToList();
      if (truthix == 0)
      {
        trueshape = availableconditions.Where(x => conditions[x]).PickRandom();
        availableconditions.Remove(trueshape);
        for (int i = 0; i < 3; i++)
        {
          falseshapes[i] = availableconditions.Where(x => !conditions[x]).PickRandom();
          availableconditions.Remove(falseshapes[i]);
        }
      }
      else
      {
        trueshape = availableconditions.Where(x => !conditions[x]).PickRandom();
        availableconditions.Remove(trueshape);
        for (int i = 0; i < 3; i++)
        {
          falseshapes[i] = availableconditions.Where(x => conditions[x]).PickRandom();
          availableconditions.Remove(falseshapes[i]);
        }
      }
      availablebuttons = Enumerable.Range(0,4).ToList();
      availablebuttons.Shuffle();
      highlights[availablebuttons[0]].mesh = shapes[trueshape];
      var hclone = highlights[availablebuttons[0]].transform.Find("Highlight(Clone)");
      if (hclone != null)
        hclone.GetComponent<MeshFilter>().mesh = shapes[trueshape];
      for (int i = 0; i < 3; i++)
      {
        highlights[availablebuttons[i+1]].mesh = shapes[falseshapes[i]];
        var ihclone = highlights[availablebuttons[i+1]].transform.Find("Highlight(Clone)");
        if (ihclone != null)
          ihclone.GetComponent<MeshFilter>().mesh = shapes[falseshapes[i]];
      }
    }

    void buttonPress (KMSelectable button)
    {
      button.AddInteractionPunch(.5f);
      var ix = Array.IndexOf(buttons, button);
      if (ix != availablebuttons[0])
      {
        GetComponent<KMBombModule>().HandleStrike();
        Debug.LogFormat("[Polygons #{0}] You pressed the {1} shape. That was incorrect. Strike! Resetting...", moduleId, posnames[ix]);
        reset();
      }
      else
      {
        GetComponent<KMBombModule>().HandlePass();
        var rix = rnd.Range(0,100);
        if (rix == 0)
          Debug.LogFormat("[Polygons #{0}] You pressed the {1} shape. That correct incorrect. Module solved.", moduleId, posnames[ix]);
        else
          Debug.LogFormat("[Polygons #{0}] You pressed the {1} shape. That was correct. Module solved.", moduleId, posnames[ix]);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        for (int i = 0; i < 4; i++)
          buttons[i].gameObject.SetActive(false);
      }
    }

    void getConditions()
    {
      var ser = bomb.GetSerialNumber();
      if (bomb.GetSerialNumberLetters().Any(x => "AEIOU".Contains(x)))
        conditions[0] = true;
      if (bomb.GetSerialNumberLetters().Count(x => "AEIOU".Contains(x)) == 0)
        conditions[1] = true;
      if (bomb.GetSerialNumberNumbers().Last() % 2 == 0)
        conditions[2] = true;
      if (bomb.GetSerialNumberNumbers().Last() % 2 != 0)
        conditions[3] = true;
      if (bomb.GetOnIndicators().Any(ind => ser.Intersect(ind).Any()))
        conditions[4] = true;
      if (bomb.GetOffIndicators().Any(ind => ser.Intersect(ind).Any()))
        conditions[5] = true;
      if (bomb.GetPortPlates().Any(x => x.Length == 0))
        conditions[6] = true;
      if (bomb.GetPortCount(Port.Parallel) > 0)
        conditions[7] = true;
      if (bomb.GetPortCount(Port.Serial) > 0)
        conditions[8] = true;
      if (bomb.GetPortCount(Port.PS2) > 0)
        conditions[9] = true;
      if (bomb.GetPortCount(Port.RJ45) > 0)
        conditions[10] = true;
      if (bomb.GetPortCount(Port.StereoRCA) > 0)
        conditions[11] = true;
      if (bomb.GetPortCount(Port.DVI) > 0)
        conditions[12] = true;
      if (bomb.IsIndicatorOn("SND") || bomb.IsIndicatorOff("SND"))
        conditions[13] = true;
      if (bomb.IsIndicatorOn("CLR") || bomb.IsIndicatorOff("CLR"))
        conditions[14] = true;
      if (bomb.IsIndicatorOn("CAR") || bomb.IsIndicatorOff("CAR"))
        conditions[15] = true;
      if (bomb.IsIndicatorOn("IND") || bomb.IsIndicatorOff("IND"))
        conditions[16] = true;
      if (bomb.IsIndicatorOn("FRQ") || bomb.IsIndicatorOff("FRQ"))
        conditions[17] = true;
      if (bomb.IsIndicatorOn("SIG") || bomb.IsIndicatorOff("SIG"))
        conditions[18] = true;
      if (bomb.IsIndicatorOn("NSA") || bomb.IsIndicatorOff("NSA"))
        conditions[19] = true;
      if (bomb.IsIndicatorOn("MSA") || bomb.IsIndicatorOff("MSA"))
        conditions[20] = true;
      if (bomb.IsIndicatorOn("TRN") || bomb.IsIndicatorOff("TRN"))
        conditions[21] = true;
      if (bomb.IsIndicatorOn("BOB") || bomb.IsIndicatorOff("BOB"))
        conditions[22] = true;
      if (bomb.IsIndicatorOn("FRK") || bomb.IsIndicatorOff("FRK"))
        conditions[23] = true;
      if (bomb.IsIndicatorOn("NLL") || bomb.IsIndicatorOff("NLL"))
        conditions[24] = true;
      if (bomb.GetBatteryCount() % 2 == 0)
        conditions[25] = true;
      if (bomb.GetBatteryCount() % 2 != 0)
        conditions[26] = true;
      if (bomb.GetBatteryHolderCount() % 2 == 0)
        conditions[27] = true;
      if (bomb.GetBatteryHolderCount() % 2 != 0)
        conditions[28] = true;
      if (bomb.GetPortCount() % 2 == 0)
        conditions[29] = true;
      if (bomb.GetPortCount() % 2 != 0)
        conditions[30] = true;
      if ((bomb.GetOffIndicators().Count() + bomb.GetOnIndicators().Count()) % 2 == 0)
        conditions[31] = true;
      if ((bomb.GetOffIndicators().Count() + bomb.GetOnIndicators().Count()) % 2 != 0)
        conditions[32] = true;
      if (startingday == 0 || startingday == 6)
        conditions[33] = true;
      if (bomb.GetSolvableModuleNames().Any(mdl => mdl == "Blind Alley" || mdl == "Tap Code" || mdl == "A Mistake" || mdl == "Braille"))
        conditions[34] = true;
    }
}
