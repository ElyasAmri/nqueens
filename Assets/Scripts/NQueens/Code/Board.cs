using System;
using System.Collections.Generic;
using System.Linq;
using static System.Linq.Enumerable;

namespace NQueens.Code
{
    public class Board
    {
        const int MAX_ITERATIONS = 1000;
        
        readonly int size;
        readonly int?[] queens;

        List<(int x, int y)> enumeratedQueens => queens
            .Where(i => i.HasValue)
            .Select((v, i) => (v.Value, i))
            .ToList();

        public Board(int size)
        {
            this.size = size;
            queens = new int?[size];
        }

        public List<(int, int)> Solve()
        {
            var c = 0;
            var random = new Random();
            var runs = 0;

            do
            {
                // calculate the conflicts in the column
                var conflicts = Range(0, size)
                    .Select((i, index) => (count: CountConflicts(i, c), index))
                    .ToList();

                // select the smallest conflicts
                var min = conflicts.Min(i => i.count);
                conflicts.RemoveAll(i => i.count != min);

                // randomly assigns the next position for the queen
                var nextPosition = random.Next(0, conflicts.Count);
                queens[c] = conflicts[nextPosition].index;

                // cycles the next c
                if (++c == size) c = 0;
                runs++;
            } while (!Check() && runs < 50);

            return enumeratedQueens;
        }

        // display frame by frame solution of the problem
        public IEnumerator<Dictionary<(int x, int y), int>> StepSolving()
        {
            var c = 0;
            var random = new Random();
            var runs = 0;

            do
            {
                var conflicts = Range(0, size)
                    .Select((i, index) => (count: CountConflicts(i, c), index))
                    .ToList();
                var min = conflicts.Min(i => i.count);
                conflicts.RemoveAll(i => i.count != min);
                var nextPosition = random.Next(0, conflicts.Count);
                queens[c] = conflicts[nextPosition].index;
                if (++c == size) c = 0;

                // return a snapshot of the board
                yield return GetSnapshot();
                runs++;

                if (++runs > MAX_ITERATIONS)
                    throw new Exception("Unsolvable");
            } while (!Check());
        }

        Dictionary<(int x, int y), int> GetSnapshot()
        {
            var snapshot = new Dictionary<(int x, int y), int>();

            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
                snapshot[(i, j)] = CountConflicts(i, j);

            foreach (var q in enumeratedQueens)
                snapshot[q] *= -1;

            return snapshot;
        }

        // check if the board is in a resolved state
        bool Check()
        {
            var eq = enumeratedQueens;

            for (var i = 0; i < enumeratedQueens.Count; i++)
            {
                for (var j = 0; j < enumeratedQueens.Count; j++)
                {
                    if (i == j) continue;

                    var q1 = eq[i];
                    var q2 = eq[j];

                    if (q1.x == q2.x ||
                        q1.y == q2.y ||
                        q1.x - q1.y == q2.x - q2.y || q1.x + q1.y == q2.x + q2.y
                       )
                        return false;
                }
            }

            return enumeratedQueens.Count == size;
        }

        // count conflicts for a cell
        // TODO: improve this mechanic
        int CountConflicts(int r, int c)
        {
            var eq = enumeratedQueens;
            var include = eq.Remove((r, c));

            // conflicts of row, column, and diagonal
            var rc = eq.Count(q => q.x == r);
            var cc = eq.Count(q => q.y == c);
            var dc = eq.Count(q => q.x - q.y == r - c || q.x + q.y == r + c);

            return rc + cc + dc + (include ? 1 : 0);
        }
    }
}