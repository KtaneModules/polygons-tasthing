using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    private static readonly string[] statusnames = new string[] { "true", "false" };
    private static readonly string[] shapenames = new string[] { "5 pointed star", "6 pointed star", "diamond", "down arrow", "down arrowhead", "down cross", "down kite", "equi triangle", "half trapezoid", "hexagon", "hourglass", "iso triangle", "left arrow", "left arrowhead", "left cross", "left kite", "long rectangle", "octagon", "parallelogram", "pentagon", "plus", "right arrow", "right arrowhead", "right cross", "right kite", "right triangle", "short trapezoid", "square", "T", "tall rectangle", "tall trapezoid", "up arrow", "up arrowhead", "up cross", "up kite" };
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
      Debug.LogFormat("[Polygons #{0}] You need to press the shape with the {1} condition.", moduleId, statusnames[truthix]);
      Debug.LogFormat("[Polygons #{0}] The correct shape is the {1}.", moduleId, shapenames[trueshape]);
      Debug.LogFormat("[Polygons #{0}] The incorrect shapes are the {1}, the {2}, and the {3}.", moduleId, shapenames[falseshapes[0]], shapenames[falseshapes[1]], shapenames[falseshapes[2]]);
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

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} cycle [Cycles through all of the shapes] | !{0} press <pos> [Presses the shape in the specified position] | Valid positions are top(t), right(r), bottom(b), and left(l)";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*cycle\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            for (int i = 0; i < 4; i++)
            {
                yield return "trycancel Shape cycling has been halted due to a request to cancel!";
                yield return new WaitForSeconds(.25f);
                var obj = highlights[i].gameObject;
                var hClone = obj.transform.Find("Highlight(Clone)");
                if (hClone != null)
                    obj = hClone.gameObject ?? obj;
                obj.SetActive(true);
                yield return new WaitForSeconds(2.25f);
                obj.SetActive(false);
                yield return new WaitForSeconds(.25f);
            }
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if(parameters.Length == 1)
                yield return "sendtochaterror Please specify the position of the shape you want to press!";
            else if (parameters.Length > 2)
                yield return "sendtochaterror Too many arguements!";
            else
            {
                if(parameters[1].EqualsIgnoreCase("top") || parameters[1].EqualsIgnoreCase("t"))
                {
                    yield return null;
                    buttons[0].OnInteract();
                }
                else if(parameters[1].EqualsIgnoreCase("right") || parameters[1].EqualsIgnoreCase("r"))
                {
                    yield return null;
                    buttons[1].OnInteract();
                }
                else if(parameters[1].EqualsIgnoreCase("bottom") || parameters[1].EqualsIgnoreCase("b"))
                {
                    yield return null;
                    buttons[2].OnInteract();
                }
                else if(parameters[1].EqualsIgnoreCase("left") || parameters[1].EqualsIgnoreCase("l"))
                {
                    yield return null;
                    buttons[3].OnInteract();
                }
                else
                    yield return "sendtochaterror The specified position '"+parameters[1]+"' is not a valid position!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        buttons[availablebuttons[0]].OnInteract();
        yield return true;
    }
}
