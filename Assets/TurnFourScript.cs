using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class TurnFourScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public GameObject[] tileobjs;
    public GameObject[] nodeobjs;
    public GameObject[] gearobjs;
    public KMSelectable[] tiles;
    public KMSelectable[] nodes;
    public KMSelectable[] gears;
    public Renderer[] tilerends;
    public Renderer[] nodeleds;
    public Material[] tmaterials;
    public Material[] onoff;
    public TextMesh cbtext;
    public TextMesh[] labels;

    private readonly int[,,] rotgrid = new int[9, 12, 3] {
    {{4, -2, 4}, {1, 3, -4}, {-3, 2, -1}, {3, -2, -3}, {4, 3, -2}, {4, -2, -1}, {-3, 4, -3}, {-4, 1, 4}, {-4, -2, 1}, {1, -4, -2}, {-3, 1, 3}, {4, 4, -3} },
    {{2, 2, 1}, {3, 2, -3}, {-2, -4, 1}, {-4, 2, 3}, {2, 3, -1}, {1, 4, 4}, {4, 2, -1}, {1, 3, -2}, {1, -4, 3}, {1, -4, 2}, {1, -4, -4}, {-4, -4, -2} },
    {{3, 3, -1}, {-4, -3, -1}, {3, -2, 3}, {1, 2, -4}, {-1, -4, -1}, {-2, 4, 3}, {2, -1, 2}, {-3, 4, -2}, {-4, -2, 1}, {-2, 1, -4}, {-1, 2, 3}, {-4, 2, -1} },
    {{-2, -4, 1}, {4, 2, 4}, {1, -3, -4}, {1, 2, -4}, {-1, 3, 1}, {-3, 2, -4}, {3, -1, -1}, {1, -3, 2}, {-4, -4, -3}, {4, 3, 4}, {2, 1, -3}, {1, -2, -4} },
    {{-4, -4, -2}, {-4, -2, -3}, {-3, -2, 3}, {3, 2, 2}, {-4, 1, 2}, {4, -3, -2}, {-4, -1, -2}, {-3, -1, -1}, {-3, -1, -2}, {4, 1, -2}, {2, -3, -1}, {-3, -1, -2} },
    {{-2, -3, -3}, {-3, -1, 2}, {-2, -4, 1}, {4, -2, 1}, {4, 1, 1}, {-3, 4, -3}, {1, 2, 1}, {-3, 4, -1}, {-2, -2, 1}, {2, 1, 4}, {2, 2, -4}, {-1, -3, -3} },
    {{-1, -2, 3}, {1, 1, -3}, {-1, 4, 2}, {2, -1, 2}, {-3, -2, -2}, {2, 3, 4}, {4, 3, 3}, {-3, -1, 4}, {1, -4, 3}, {-1, -4, -2}, {-1, 2, 1}, {3, 2, 2} },
    {{3, 1, -4}, {3, 3, -1}, {-4, -3, 1}, {1, 2, 1}, {-2, -2, 3}, {2, 4, 4}, {-1, 3, -1}, {-4, -2, 1}, {-1, -4, 3}, {-3, -3, 1}, {4, -2, 3}, {-3, -1, -4} },
    {{-2, -2, -4}, {-3, 2, 1}, {-4, -3, -3}, {3, -4, -2}, {2, 3, 3}, {2, -3, -1}, {-4, -2, -3}, {-1, -1, 2}, {2, -4, 3}, {-2, -3, -3}, {-2, -3, -2}, {1, 1, 3} } };
    private readonly int[][] cycles = new int[4][] { new int[4] { 0, 1, 4, 2 }, new int[4] { 1, 3, 6, 4 }, new int[4] { 2, 4, 7, 5 }, new int[4] { 4, 6, 8, 7 } };
    private int[][] states = new int[2][] { new int[9] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[9]};
    private int[] cols = new int[9] { 0, 1, 2, 3, 4, 5, 6, 7, 8};
    private int select;
    private List<int>[] rot =  new List<int>[2] { new List<int> { }, new List<int> { 0 } };
    private bool turnanim;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        module.OnActivate = Activate;
    }

    private void Activate()
    {
        cols.Shuffle();
        for (int i = 0; i < 9; i++)
            tilerends[i].material = tmaterials[cols[i]];
        Debug.LogFormat("[Turn Four #{0}] The tile colours are: {1}", moduleID, string.Join(" ", cols.Select((x, k) => (k + 1).ToString() + "=" + "ROYGCBPNE"[x].ToString()).ToArray()));
        int[] indcols = new int[6];
        int[] colindices = cols.Select((x, k) => Array.IndexOf(cols, k)).ToArray();
        switch(colindices[3])
        {
            case 0:
                indcols[0] = cols[8];
                indcols[1] = cols[Mathf.Max(colindices[0], colindices[1], colindices[2])];
                indcols[2] = colindices[5] == 8 ? 5 : cols[colindices[5] + 1];
                indcols[3] = colindices[4] + 1 > info.GetSerialNumberNumbers().Min() ? 4 : 0;
                indcols[4] = cols[4];
                indcols[5] = colindices[2] > colindices[8] ? 2 : 6;
                break;
            case 1:
                indcols[0] = cols[8 - colindices[4]];
                indcols[1] = info.GetSerialNumberNumbers().Contains(colindices[4] + 1) ? 4 : 7;
                indcols[2] = cols[2];
                indcols[3] = colindices[7] % 2 == 0 ? cols[5] : cols[6];
                indcols[4] = (colindices[0] + colindices[4]) % 2 == 0 ? cols[(colindices[0] + colindices[4]) / 2] : cols[Mathf.Max(colindices[0], colindices[4])];
                indcols[5] = cols[7];
                break;
            case 2:
                indcols[0] = cols[Mathf.Max(colindices[4], colindices[5])];
                indcols[1] = cols[1];
                indcols[2] = (colindices[5] + colindices[6]) % 2 == 0 ? cols[(colindices[5] + colindices[6]) / 2] : (colindices[5] % 2 == 1 ? 5 : 6);
                indcols[3] = cols[new int[3] { colindices[2], colindices[7], colindices[8]}.OrderBy(x => x).ToArray()[1]];
                indcols[4] = cols[6];
                indcols[5] = colindices[8] > 4 ? 1 : 0;
                break;
            case 3:
                indcols[0] = cols[0];
                indcols[1] = info.GetPortCount() == 0 ? 3 : (info.GetPortCount() > 8 ? cols[8] : cols[info.GetPortCount() - 1]);
                indcols[2] = (colindices[1] % 2 == colindices[4] % 2) ? ((colindices[1] % 2 == colindices[7] % 2) ? 5 : 7) : ((colindices[1] % 2 == colindices[7] % 2) ? 4 : 1);
                indcols[3] = cols[5];
                indcols[4] = (colindices[0] + colindices[6] < 8) ? cols[colindices[0] + colindices[6] + 1] : cols[Mathf.Abs(colindices[0] - colindices[6]) - 1];
                indcols[5] = colindices[2] % 2 == 0 ? 2 : cols[colindices[2] / 2]; 
                break;
            case 4:
                indcols[0] = cols[colindices.Where((x, k) => k / 3 == k % 3).OrderBy(x => x).ToArray()[1]];
                indcols[1] = cols[Mathf.Abs(colindices[1] - colindices[6]) - 1];
                indcols[2] = cols[3];
                indcols[3] = colindices[6] % 2 == 0 ? 6 : cols[colindices[6] / 2];
                indcols[4] = cols[8];
                indcols[5] = Mathf.Abs(colindices[4] - colindices[5]) % 3 == 0 ? cols[0] : cols[Mathf.Abs(colindices[4] - colindices[5]) - 1];
                break;
            case 5:
                indcols[0] = info.GetPortCount() == 0 ? 1 : ((colindices[6] + 1 == info.GetPortCount() || colindices[8] + 1 == info.GetPortCount()) ? 7 : 1);
                indcols[1] = cols[7];
                indcols[2] = cols[Mathf.Min(colindices[4], colindices[5], colindices[6])];
                indcols[3] = colindices[8] == 0 ? 8 : cols[colindices[8] - 1];
                indcols[4] = (colindices[2] + colindices[5] < 8) ? cols[colindices[2] + colindices[5] + 1] : cols[Mathf.Abs(colindices[2] - colindices[5]) - 1];
                indcols[5] = cols[4];
                break;
            case 6:
                indcols[0] = cols[2];
                indcols[1] = cols[Mathf.Max(colindices[5], colindices[6]) - 1];
                indcols[2] = cols[8 - colindices[0]];
                indcols[3] = cols[1];
                indcols[4] = cols[8 - Mathf.Max(colindices[7], colindices[1])];
                indcols[5] = (colindices[4] + colindices[8]) % 3 == 2 ? 4 : 8;
                break;
            case 7:
                indcols[0] = (colindices[1] + colindices[2]) % 2 == 0 ? cols[(colindices[1] + colindices[2]) / 2] : (colindices[1] % 2 == 0 ? 1 : 2);
                indcols[1] = cols[6];
                indcols[2] = (info.GetIndicators().Count() == colindices[5] + 1 || info.GetOffIndicators().Count() == colindices[5] + 1 || info.GetOnIndicators().Count() == colindices[5] + 1) ? 5 : cols[8 - colindices[5]];
                indcols[3] = cols[colindices.Where((x, k) => k > 5).OrderBy(x => x).ToArray()[1]];
                indcols[4] = colindices[0] % 3 == 2 ? cols[8] : (colindices[0] == 0 ? 0 : cols[colindices[0] - 1]);
                indcols[5] = cols[0];
                break;
            default:
                indcols[0] = cols[Mathf.Min(colindices[0], colindices[2], colindices[5]) + 1];
                indcols[1] = colindices[8] < 5 ? 6 : cols[8 - colindices[6]];
                indcols[2] = cols[5];
                indcols[3] = (info.GetBatteryCount() == colindices[7] + 1 || 2 * (info.GetBatteryCount() - info.GetBatteryHolderCount()) == colindices[7] + 1|| (2 * info.GetBatteryHolderCount()) - info.GetBatteryCount() == colindices[7] + 1) ? 7 : cols[7];
                indcols[4] = cols[3];
                indcols[5] = colindices[4] % 2 == 0 ? cols[4] : cols[8];
                break;
        }
        Debug.LogFormat("[Turn Four #{0}] The colour sequence is: {1}", moduleID, string.Join(" ", indcols.Select(x => "ROYGCBPNE"[x].ToString()).ToArray()));
        for(int i = 0; i < 6; i++)
        {
            int sn = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(info.GetSerialNumber()[i].ToString()) % 12;
            int[] cell = Enumerable.Range(0, 3).Select(x => rotgrid[indcols[i], sn, x]).ToArray();
            List<int[]> transformation = new List<int[]> { cell };
            if (i > 0)
            {
                int[] prev = new int[3];
                for (int j = 0; j < 3; j++) prev[j] = cell[j];
                transformation.Add(prev);
                switch (indcols[i - 1])
                {
                    case 0: transformation[1][0] *= -1; break;
                    case 1: transformation[1][1] = (int)Mathf.Sign(transformation[1][1]) * (((Mathf.Abs(transformation[1][1]) + 1) % 4) + 1); break;
                    case 2: transformation[1][2] = -(int)Mathf.Sign(transformation[1][2]) * (((Mathf.Abs(transformation[1][2]) + 1) % 4) + 1); break;
                    case 3: transformation[1][1] *= -1; break;
                    case 4: transformation[1][0] = -(int)Mathf.Sign(transformation[1][0]) * (((Mathf.Abs(transformation[1][0]) + 1) % 4) + 1); break;
                    case 5: transformation[1][2] = (int)Mathf.Sign(transformation[1][2]) * (((Mathf.Abs(transformation[1][2]) + 1) % 4) + 1); break;
                    case 6: transformation[1][2] *= -1; break;
                    case 7: transformation[1][0] = (int)Mathf.Sign(transformation[1][0]) * (((Mathf.Abs(transformation[1][0]) + 1) % 4) + 1); break;
                    default: transformation[1][1] = -(int)Mathf.Sign(transformation[1][1]) * (((Mathf.Abs(transformation[1][1]) + 1) % 4) + 1); break;
                }
            }
            if(i > 1)
            {
                int[] prev = new int[3];
                for (int j = 0; j < 3; j++) prev[j] = transformation[1][j];
                transformation.Add(prev);
                switch(indcols[i - 2])
                {
                    case 0: int s1 = transformation[2][0]; transformation[2][0] = transformation[2][1]; transformation[2][1] = s1; break;
                    case 1: transformation[2][2] = (int)Mathf.Sign(transformation[2][2]) * (((Mathf.Abs(transformation[2][2]) + 2) % 4) + 1); break;
                    case 2: int s2 = transformation[2][1]; transformation[2][1] = transformation[2][2]; transformation[2][2] = s2; break;
                    case 3: transformation[2][0] = (int)Mathf.Sign(transformation[2][0]) * ((Mathf.Abs(transformation[2][0]) % 4) + 1); break;
                    case 4: int s3 = transformation[2][0]; transformation[2][0] = transformation[2][2]; transformation[2][2] = s3; break;
                    case 5: transformation[2][1] = (int)Mathf.Sign(transformation[2][1]) * ((Mathf.Abs(transformation[2][1]) % 4) + 1); break;
                    case 6: transformation[2][0] = (int)Mathf.Sign(transformation[2][0]) * (((Mathf.Abs(transformation[2][0]) + 2) % 4) + 1); break;
                    case 7: transformation[2][2] = (int)Mathf.Sign(transformation[2][2]) * ((Mathf.Abs(transformation[2][2]) % 4) + 1); break;
                    default: transformation[2][1] = (int)Mathf.Sign(transformation[2][1]) * (((Mathf.Abs(transformation[2][1]) + 2) % 4) + 1); break;
                }
            }
            if (i > 2)
            {
                int[] prev = new int[3];
                for (int j = 0; j < 3; j++) prev[j] = transformation[2][j];
                transformation.Add(prev);
                switch (indcols[i - 3])
                {
                    case 0: transformation[3] = Enumerable.Range(0, 3).Select(x => -(int)Mathf.Sign(transformation[3][x]) * (((Mathf.Abs(transformation[3][x]) + 1) % 4) + 1)).ToArray(); break;
                    case 1: transformation[3] = Enumerable.Range(0, 3).Select(x => (int)Mathf.Sign(transformation[3][x]) * Mathf.Abs(transformation[3][(x + 1) % 3])).ToArray(); break;
                    case 2: transformation[3] = Enumerable.Range(0, 3).Select(x => (int)Mathf.Sign(transformation[3][x]) * Mathf.Abs(transformation[3][(x + 2) % 3])).ToArray(); break;
                    case 3: transformation[3] = transformation[3].Select(x => -x).ToArray(); break;
                    case 4: transformation[3] = Enumerable.Range(0, 3).Select(x => (int)Mathf.Sign(transformation[3][x]) * (((Mathf.Abs(transformation[3][x]) + 1) % 4) + 1)).ToArray(); break;
                    case 5: transformation[3] = Enumerable.Range(0, 3).Select(x => (int)Mathf.Sign(transformation[3][(x + 1) % 3]) * Mathf.Abs(transformation[3][x])).ToArray(); break;
                    case 6: transformation[3] = Enumerable.Range(0, 3).Select(x => (int)Mathf.Sign(transformation[3][(x + 2) % 3]) * Mathf.Abs(transformation[3][x])).ToArray(); break;
                    case 7: transformation[3] = Enumerable.Range(0, 3).Select(x => transformation[3][(x + 1) % 3]).ToArray(); break;
                    default: transformation[3] = Enumerable.Range(0, 3).Select(x => transformation[3][(x + 2) % 3]).ToArray(); break;
                }
            }
            if(i > 3)
            {
                int[] prev = new int[3];
                for (int j = 0; j < 3; j++) prev[j] = transformation[3][j];
                transformation.Add(prev);
                switch (indcols[i - 4])
                {
                    case 0: transformation[4][2] = rot[0].Where((x, k) => k > rot[0].Count() - 3 && x > 0).Count() % 3 == 0 ? -transformation[4][2] : (int)Mathf.Sign(transformation[4][2]) * ((Mathf.Abs(transformation[4][2] + 1) % 4) + 1); break;
                    case 1: transformation[4] = transformation[4].Select(x => (int)Mathf.Sign(x) * (((Mathf.Abs(x) + (x < 0 ? 0 : 2)) % 4) + 1)).ToArray(); break;
                    case 2: transformation[4] = transformation[4].Select(x => (int)Mathf.Sign(x) * ((Mathf.Abs(x) % 4) + 1)).ToArray(); break;
                    case 3: transformation[4][1] = rot[0].Where((x, k) => k > rot[0].Count() - 3 && x > 0).Count() % 2 == 1 ? -transformation[4][1] : (int)Mathf.Sign(transformation[4][1]) * ((Mathf.Abs(transformation[4][1] + 1) % 4) + 1); break;
                    case 4: transformation[4] = transformation[4].Select(x => -(int)Mathf.Sign(x) * (((Mathf.Abs(x) + 1) % 4) + 1)).ToArray(); break;
                    case 5: transformation[4] = transformation[4].Select(x => (int)Mathf.Sign(x) * (((Mathf.Abs(x) + (x < 0 ? 2 : 0)) % 4) + 1)).ToArray(); break;
                    case 6: transformation[4] = transformation[4].Select(x => (int)Mathf.Sign(x) * (((Mathf.Abs(x) + 2) % 4) + 1)).ToArray(); break;
                    case 7: transformation[4][0] = rot[0].Where((x, k) => k > rot[0].Count() - 3 && x > 0).Count() > 1 ? -transformation[4][0] : (int)Mathf.Sign(transformation[4][0]) * ((Mathf.Abs(transformation[4][0] + 1) % 4) + 1); break;
                    default: transformation[4] = transformation[4].Select((x, k) => (int)Mathf.Sign(x) * (((Mathf.Abs(x) + (rot[0][rot[0].Count() - 3 + k] < 0 ? 2 : 0)) % 4) + 1)).ToArray(); break;
                }
            }
            if (i == 5)
            {
                int[] prev = new int[3];
                for (int j = 0; j < 3; j++) prev[j] = transformation[4][j];
                transformation.Add(prev);
                switch (indcols[0])
                {
                    case 0: transformation[5] = transformation[5].Select(x => rot[0].Count(q => q > 0) > 7 ? -Mathf.Abs(x) : Mathf.Abs(x)).ToArray(); break;
                    case 1: transformation[5] = transformation[5].Select(x => rot[0].Count(q => q > 0) % 2 == 0 ? -x : (int)Mathf.Sign(x) * (((Mathf.Abs(x) + 1) % 4) + 1)).ToArray(); break;
                    case 2: transformation[5] = Enumerable.Range(0, 3).Select(x => rot[0].Count(q => q > 0) > 7 ? transformation[5][(x + 1) % 3] : transformation[5][(x + 2) % 3]).ToArray(); break;
                    case 3: transformation[5] = transformation[5].Select(x => (int)Mathf.Sign(x) * (rot[0].Count(q => q > 0) % 2 == 0 ? (Mathf.Abs(x) % 4) + 1 : (((Mathf.Abs(x) + 2) % 4) + 1))).ToArray(); break;
                    case 4: transformation[5] = Enumerable.Range(0, 3).Select(x => (int)Mathf.Sign(x) * (rot[0].Distinct().Count() < 8 ? transformation[5][(x + 1) % 3] : ((Mathf.Abs(transformation[5][x]) % 4) + 1))).ToArray(); break;
                    case 5: transformation[5] = transformation[5].Select(x => (int)Mathf.Sign(x) * (rot[0].Count(q => q > 0) > 7 ? (Mathf.Abs(x) % 4) + 1 : (((Mathf.Abs(x) + 2) % 4) + 1))).ToArray(); break;
                    case 6: transformation[5] = Enumerable.Range(0, 3).Select(x => rot[0].Count(q => q > 0) % 2 == 0 ? transformation[5][(x + 1) % 3] : transformation[5][(x + 2) % 3]).ToArray(); break;
                    case 7: transformation[5] = transformation[5].Select(x => rot[0].Count(q => q > 0) > 7 ? -x : (int)Mathf.Sign(x) * (((Mathf.Abs(x) + 1) % 4) + 1)).ToArray(); break;
                    default: transformation[5] = Enumerable.Range(0, 3).Select(x => (int)Mathf.Sign(x) * (rot[0].Distinct().Count() < 8 ? transformation[5][(x + 2) % 3] : (((Mathf.Abs(transformation[5][x]) + 2) % 4) + 1))).ToArray(); break;
                }
            }
            Debug.LogFormat("[Turn Four #{0}] Steps {1} - {2}: {3}", moduleID, (3 * i) + 1, (3 * i) + 3, string.Join(" \u2192 ", transformation.Select((x, k) => (i == k ? "[" : "(") + string.Join(", ", x.Select(y => "XURDL"[Mathf.Abs(y)].ToString() + (y < 0 ? "'" : "")).ToArray()) + (i == k ? "]" : ")")).ToArray()));
            for (int j = 0; j < 3; j++)
                rot[0].Add(transformation[i][j]);
        }
        Debug.LogFormat("[Turn Four #{0}] The complete sequence is: [{1}]", moduleID, string.Join(", ", rot[0].Select(x => "URDL"[Mathf.Abs(x) - 1].ToString() + (x < 0 ? "'" : "")).ToArray()));
        List<string> spin = new List<string> { "#" };
        for(int i = 0; i < 18; i++)
        {
            if (Mathf.Abs(rot[0][i]) != "URDL".IndexOf(spin.Last()[0].ToString()) + 1)
                spin.Add("URDL"[Mathf.Abs(rot[0][i]) - 1].ToString() + (rot[0][i] < 0 ? "'" : ""));
            else if(rot[0][i] < 0)
            {
                if (spin.Last().Length == 1)
                    spin.RemoveAt(spin.Count() - 1);
                else if (spin.Last()[1] == '\'')
                    spin[spin.Count() - 1] = spin.Last()[0].ToString() + "2";
                else
                    spin[spin.Count() - 1] = spin.Last()[0].ToString();
            }
            else
            {
                if (spin.Last().Length == 1)
                    spin[spin.Count() - 1] = spin.Last()[0].ToString() + "2";
                else if (spin.Last()[1] == '\'')
                    spin.RemoveAt(spin.Count() - 1);
                else
                    spin[spin.Count() - 1] = spin.Last()[0].ToString() + "'";
            }
            select = new int[4] { 0, 2, 3, 1 }[Mathf.Abs(rot[0][i]) - 1];
            for(int k = 0; k < 4; k++)
                tileobjs[states[0][cycles[select][k]]].transform.RotateAround(nodeobjs[select].transform.position, transform.up, rot[0][i] < 0 ? -90 : 90);
            Rot(-(int)Mathf.Sign(rot[0][i]) * (select + 1));
        }
        select = 0;
        spin.RemoveAt(0);
        Debug.LogFormat("[Turn Four #{0}] Condensed: [{1}]", moduleID, string.Join(", ", spin.ToArray()));
        spin.Reverse();
        spin = spin.Select(x => x.Length == 1 ? x + "'" : (x[1] == '\'' ? x[0].ToString() : x)).ToList();
        Debug.LogFormat("[Turn Four #{0}] Solution: [{1}]", moduleID, string.Join(", ", spin.ToArray()));
        foreach (KMSelectable tile in tiles)
        {
            int b = Array.IndexOf(tiles, tile);
            tile.OnHighlight = delegate () { if(!moduleSolved) cbtext.text = "ROYGCBPNE"[cols[b]].ToString(); };
            tile.OnHighlightEnded = delegate () { cbtext.text = string.Empty; };
        }
        foreach(KMSelectable node in nodes)
        {
            int b = Array.IndexOf(nodes, node);
            node.OnInteract = delegate ()
            {
                node.AddInteractionPunch();
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, node.transform);
                for (int i = 0; i < 4; i++)
                    nodeleds[i].material = onoff[1];
                nodeleds[b].material = onoff[0];
                select = b;
                return false;
            };
        }
        foreach(KMSelectable gear in gears)
        {
            int b = Array.IndexOf(gears, gear);
            gear.OnInteract = delegate () { Crank(b); return false; };
        }
    }

    private void Crank(int b)
    {
        if(!turnanim)
        {
            turnanim = true;
            int s = ((2 * b) - 1) * (select + 1);
            if (rot[1].Last() + s == 0)
            {
                StartCoroutine(Spin(s));
                rot[1].RemoveAt(rot[1].Count() - 1);
                if (rot[1].Count() > 1 && rot[1].Last() == rot[1][rot[1].Count() - 2] && rot[1].Last() == rot[1][rot[1].Count() - 3])
                    Audio.PlaySoundAtTransform("Crank2", transform);
                else
                    Audio.PlaySoundAtTransform("Crank1", transform);
            }
            else
            {
                rot[1].Add(s);
                if (rot[1].Last() != rot[1][rot[1].Count() - 2])
                {
                    Audio.PlaySoundAtTransform("Crank1", transform);
                    StartCoroutine(Spin(s));
                }
                else if (rot[1].Last() != rot[1][rot[1].Count() - 3])
                {
                    Audio.PlaySoundAtTransform("Crank2", transform);
                    StartCoroutine(Spin(s));
                }
                else if (rot[1].Last() != rot[1][rot[1].Count() - 4])
                {
                    Audio.PlaySoundAtTransform("Crank3", transform);
                    StartCoroutine(Spin(s));
                }
                else
                {
                    if (!moduleSolved && !info.GetFormattedTime().Contains((Array.IndexOf(cols, 0) + 1).ToString()))
                        module.HandleStrike();
                    Debug.LogFormat("[Turn Four #{0}] Module reset at {1}", moduleID, info.GetFormattedTime());
                    StartCoroutine(Sprung());
                    StartCoroutine(GearReset(false));
                }
            }
        }
    }

    private void Rot(int c)
    {
        if(c < 0)
        {
            int f = states[0][cycles[Mathf.Abs(c) - 1][0]];
            for(int i = 0; i < 3; i++)
                states[0][cycles[Mathf.Abs(c) - 1][i]] = states[0][cycles[Mathf.Abs(c) - 1][i + 1]];
            states[0][cycles[Mathf.Abs(c) - 1][3]] = f;
            for(int i = 0; i < 4; i++)
                states[1][states[0][cycles[Mathf.Abs(c) - 1][i]]] = (states[1][states[0][cycles[Mathf.Abs(c) - 1][i]]] + 1) % 4;
        }
        else
        {
            int f = states[0][cycles[Mathf.Abs(c) - 1][3]];
            for(int i = 3; i > 0; i--)
                states[0][cycles[Mathf.Abs(c) - 1][i]] = states[0][cycles[Mathf.Abs(c) - 1][i - 1]];
            states[0][cycles[Mathf.Abs(c) - 1][0]] = f;
            for(int i = 0; i < 4; i++)
                states[1][states[0][cycles[Mathf.Abs(c) - 1][i]]] = (states[1][states[0][cycles[Mathf.Abs(c) - 1][i]]] + 3) % 4;
        }
    }

    private IEnumerator Spin(int c)
    {
        for (int j = 0; j < 10; j++)
        {
            for (int k = 0; k < 4; k++)
                tileobjs[states[0][cycles[select][k]]].transform.RotateAround(nodeobjs[select].transform.position, transform.up, c < 0 ? 9 : -9);
            for (int k = 0; k < 2; k++)
                gearobjs[k].transform.Rotate(0, 0, c < 0 ? -9 : 9);
            yield return new WaitForSeconds(0.05f);
        }
        Rot(c);
        turnanim = false;
        if (!moduleSolved && states[0].SequenceEqual(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }) && states[1].All(x => x == 0))
        {
            moduleSolved = true;
            module.HandlePass();
            rot[1] = new List<int> { 0 };
            Audio.PlaySoundAtTransform("Solve", transform);
            for (int i = 0; i < 9; i++) {
                tilerends[i].material = tmaterials[9];
                labels[2 * i].color = new Color(0, 1, 0);
                labels[(2 * i) + 1].color = new Color(0, 1, 0);
            }
        }
    }

    private IEnumerator GearReset(bool s)
    {
        while (turnanim)
        {
            for (int i = 0; i < 2; i++)
                gearobjs[i].transform.Rotate(0, 0, (i == 0 ^ s) ? -4.5f : 4.5f);
            yield return new WaitForSeconds(0.025f);
        }
    }

    private IEnumerator Sprung()
    {
        Audio.PlaySoundAtTransform("Sprung", transform);
        for(int i = rot[1].Count() - 2; i > 0; i--)
        {
            nodeleds[select].material = onoff[1];
            select = Mathf.Abs(rot[1][i]) - 1;
            nodeleds[select].material = onoff[0];
            for(int j = 0; j < 2; j++)
            {
                for(int k = 0; k < 4; k++)
                    tileobjs[states[0][cycles[select][k]]].transform.RotateAround(nodeobjs[select].transform.position, transform.up, rot[1][i] < 0 ? -45 : 45);
                yield return new WaitForSeconds(0.05f);
            }
            Rot(-rot[1][i]);
        }
        rot[1] = new List<int> { 0 };
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
        turnanim = false;
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} <U/L/R/D/U'/L'/R'/D'> [Rotates tiles. Separate with spaces.] | !{0} reset";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (command.ToLowerInvariant() == "reset")
        {
            StartCoroutine(GearReset(false));
            StartCoroutine(Sprung());
            yield break;
        }
        string[] commands = command.ToUpperInvariant().Split(' ');
        List<int> nums = new List<int> { };
        for (int i = 0; i < rot[1].Count(); i++)
            nums.Add(rot[1][i]);
        for (int i = 0; i < commands.Length; i++)
        {
            int num = Array.IndexOf(new string[] { "D'", "R'", "L'", "U'", "X", "U", "L", "R", "D" }, commands[i]) - 4;
            if (num < -4 || num == 0)
            {
                yield return "sendtochaterror " + commands[i] + " is an invalid rotation.";
                yield break;
            }
            if (num + nums.Last() == 0)
                nums.RemoveAt(nums.Count() - 1);
            else
                nums.Add(num);
            if(nums.Count() > 3 && nums.Where((x, k) => k > nums.Count() - 5).Distinct().Count() < 2)
            {
                yield return "sendtochaterror This sequence of commands would trigger the reset mechanism.";
                yield break;
            }
        }
        nums.RemoveRange(0, rot[1].Count());
        for(int i = 0; i < nums.Count(); i++)
        {
            yield return null;
            nodes[Mathf.Abs(nums[i]) - 1].OnInteract();
            gears[nums[i] < 0 ? 1 : 0].OnInteract();
            while (turnanim)
                yield return true;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        turnanim = true;
        StartCoroutine(GearReset(false));
        StartCoroutine(Sprung());
        while (turnanim)
            yield return true;
        turnanim = true;
        StartCoroutine(GearReset(true));
        rot[0] = rot[0].Select(x => (int)Mathf.Sign(x) * new int[] { 1, 3, 4, 2 }[Mathf.Abs(x) - 1]).ToList();
        for (int i = rot[0].Count() - 1; i > -1; i--)
        {
            nodeleds[select].material = onoff[1];
            select = Mathf.Abs(rot[0][i]) - 1;
            nodeleds[select].material = onoff[0];
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 4; k++)
                    tileobjs[states[0][cycles[select][k]]].transform.RotateAround(nodeobjs[select].transform.position, transform.up, rot[0][i] < 0 ? 45 : -45);
                yield return new WaitForSeconds(0.05f);
            }
            Rot(rot[0][i]);
        }
        turnanim = false;
        moduleSolved = true;
        module.HandlePass();
        rot[1] = new List<int> { 0 };
        Audio.PlaySoundAtTransform("Solve", transform);
        for (int i = 0; i < 9; i++)
        {
            tilerends[i].material = tmaterials[9];
            labels[2 * i].color = new Color(0, 1, 0);
            labels[(2 * i) + 1].color = new Color(0, 1, 0);
        }
    }
}
