using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NQueens.Code;
using TMPro;

public class MainScript : MonoBehaviour
{
    [SerializeField] int size = 8;
    [SerializeField] SolveOption solveOption = SolveOption.Auto;
    [SerializeField] float frameDelay = 0;
    [SerializeField] KeyCode stepKey = KeyCode.Space;

    [SerializeField] GameObject queenPrefab;
    [SerializeField] GameObject squarePrefab;
    [SerializeField] GameObject conflictPrefab;

    Board board;
    float sizeFactor = 1;
    Vector3 offsetFactor = Vector3.zero;
    List<GameObject> currentQueens = new();
    Dictionary<(int, int), TextMeshProUGUI> currentConflicts = new();
    Transform squaresParent;
    Transform conflictsParent;
    Transform queensParent;


    void Start()
    {
        Setup();
        GenerateField();
        Solve();
    }

    void Setup()
    {
        squaresParent = new GameObject("Squares").transform;
        squaresParent.SetParent(transform);
        queensParent = new GameObject("Queens").transform;
        queensParent.SetParent(transform);
        conflictsParent = new GameObject("Conflicts Counts").transform;
        conflictsParent.SetParent(transform);

        for (int i = 0; i < size; i++)
        for (int j = 0; j < size; j++)
        {
            currentConflicts[(i, j)] = Instantiate(conflictPrefab, new Vector3(i, j) * sizeFactor + offsetFactor,
                Quaternion.identity, conflictsParent).GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    void GenerateField()
    {
        for (var i = 0; i < size; i++)
        for (var j = 0; j < size; j++)
            Instantiate(squarePrefab, new Vector3(i, j) * sizeFactor + offsetFactor, Quaternion.identity, squaresParent);
    }

    void Solve()
    {
        board = new Board(size);
        if (solveOption == SolveOption.Auto)
        {
            var solution = board.Solve();
            PutQueens(solution);
        }
        else
        {
            StartCoroutine(FrameSolver());
        }
    }

    void PutQueens(List<(int, int)> queens)
    {
        currentQueens.ForEach(Destroy);

        foreach (var (x, y) in queens)
        {
            var q = Instantiate(queenPrefab, new Vector3(x, y) * sizeFactor + offsetFactor, Quaternion.identity, queensParent);
            currentQueens.Add(q);
        }
    }

    IEnumerator FrameSolver()
    {
        var enumerator = board.StepSolving();
        object wait = solveOption == SolveOption.StepByStep 
            ? new WaitForSeconds(frameDelay) 
            : new WaitUntil(() => Input.GetKeyDown(stepKey));
        while (enumerator.MoveNext())
        {
            var queens = enumerator.Current?
                .Where(kv => kv.Value == -1)
                .Select(kv => kv.Key)
                .ToList();

            var conflicts = enumerator.Current?
                .Where(kv => kv.Value > 0)
                .ToDictionary(p => p.Key, p => p.Value);
            
            PutQueens(queens);
            PutConflicts(conflicts);
            yield return wait;
        }

        Debug.Log("Done");
    }

    void PutConflicts(Dictionary<(int x, int y), int> conflicts)
    {
        foreach (var (k, v) in conflicts)
        {
            currentConflicts[k].text = v.ToString();
        }
    }
}

public enum SolveOption
{
    Auto,
    StepByStep,
    WaitForInput,
}