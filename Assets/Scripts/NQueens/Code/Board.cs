using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Linq.Enumerable;
using Random = System.Random;

namespace NQueens.Code
{
    public class Board
    {
        const int MAX_ITERATIONS = 100000;

        readonly int size;
        readonly int?[] queens;
        readonly int[,] conflictsCounter;
        readonly int[] row;

        int currentColumn;
        int runs;
        readonly Random random = new();

        List<(int x, int y)> enumeratedQueens => queens
            .Where(i => i.HasValue)
            .Select((v, i) => (v.Value, i))
            .ToList();

        public Board(int size)
        {
            this.size = size;
            queens = new int?[size];
            conflictsCounter = new int[size, size];
            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
                conflictsCounter[i, j] = 1;
            row = Range(0, size).ToArray();
        }

        public List<(int, int)> Solve()
        {
            do
            {
                Iteration();

                if (runs > MAX_ITERATIONS)
                    throw new Exception("Unsolvable");
            } while (!Check());

            Debug.Log($"Took {runs} iterations");

            return enumeratedQueens;
        }

        // display frame by frame solution of the problem
        public IEnumerator<Dictionary<(int x, int y), int>> StepSolving()
        {
            do
            {
                if (Iteration())
                {
                    // return a snapshot of the board
                    yield return GetSnapshot();                    
                }
            } while (!Check());

            Debug.Log($"Took {runs} iterations");
        }

        bool Iteration()
        {
            var q = queens[currentColumn];
            if (q.HasValue && conflictsCounter[q.Value, currentColumn] == 1)
            {
                if (++currentColumn == size) currentColumn = 0;
                return false;
            }
            
            // calculate the conflicts in the column
            var conflicts = row
                .Select((i, index) => (count: conflictsCounter[i, currentColumn], index))
                .ToList();

            // select the smallest conflicts
            var min = conflicts.Min(i => i.count);
            conflicts.RemoveAll(i => i.count != min);

            // randomly assigns the next position for the queen
            var nextPosition = random.Next(0, conflicts.Count);
            MoveQueen(currentColumn, conflicts[nextPosition].index);

            // cycle the column
            if (++currentColumn == size) currentColumn = 0;
            ++runs;
            return true;
        }

        Dictionary<(int x, int y), int> GetSnapshot()
        {
            var snapshot = new Dictionary<(int x, int y), int>();

            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
                snapshot[(i, j)] = conflictsCounter[i, j];

            foreach (var q in enumeratedQueens)
                snapshot[q] *= -1;

            return snapshot;
        }

        // check if the board is in a resolved state
        bool Check()
        {
            var eq = enumeratedQueens;

            if (eq.Count != size) return false; 

            foreach (var (x, y) in eq)
            {
                if (conflictsCounter[x, y] != 1)
                    return false;
            }

            return true;
        }

        void MoveQueen(int c, int newR)
        {
            if (!queens[c].HasValue) goto skipSubtraction;
            var r = queens[c].Value;
            if (r == newR) return;

            // decrement the old row
            ShiftRow(r, -1);
            // decrement the positive diagonals we are moving from
            ShiftPositiveDiagonal(r, c, -1);
            ShiftNegativeDiagonal(r, c, -1);
            // offset the overlap
            conflictsCounter[r, c] += 3;

            skipSubtraction:

            // increment the new row
            ShiftRow(newR, 1);
            // increment the diagonals we are moving to
            ShiftPositiveDiagonal(newR, c, 1);
            ShiftNegativeDiagonal(newR, c, 1);
            // offset the overlap
            conflictsCounter[newR, c] -= 3;

            queens[c] = newR;
        }

        void ShiftRow(int r, int v)
        {
            for (var i = 0; i < size; i++)
            {
                conflictsCounter[r, i] += v;
            }
        }

        void ShiftPositiveDiagonal(int r, int c, int v)
        {
            var o = r - c;
            switch (o)
            {
                case 0:
                    for (var i = 0; i < size; i++)
                        conflictsCounter[i, i] += v;
                    break;
                case > 0:
                    for (var i = 0; i < size - o; i++)
                        conflictsCounter[i + o, i] += v;
                    break;
                case < 0:
                    for (var i = 0; i < size + o; i++)
                        conflictsCounter[i, i - o] += v;
                    break;
            }
        }

        void ShiftNegativeDiagonal(int r, int c, int v)
        {
            var asize = size - 1;
            var o = r - (asize - c);
            switch (o)
            {
                case 0:
                    for (var i = 0; i < size; i++)
                        conflictsCounter[i, asize - i] += v;
                    break;
                case > 0:
                    for (var i = 0; i < size - o; i++)
                        conflictsCounter[i + o, asize - i] += v;
                    break;
                case < 0:
                    for (var i = 0; i < size + o; i++)
                        conflictsCounter[i, asize - (i - o)] += v;
                    break;
            }
        }
    }
}