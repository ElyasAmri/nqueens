using System;
using System.Collections.Generic;
using System.Linq;
using static System.Linq.Enumerable;

namespace NQueens.Code
{
    public class Board
    {
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

            while (Check())
            {
                // calculate the conflicts in the column
                var conflicts = Range(0, size)
                    .Select((i, index) => (count: CountConflicts(i, c) - 3, index))
                    .ToList();

                // select the smallest conflicts
                var min = conflicts.Min(i => i.count);
                conflicts.RemoveAll(i => i.count != min);

                // randomly assigns the next position for the queen
                var nextPosition = random.Next(0, conflicts.Count);
                queens[c] = nextPosition;

                // cycles the next c
                if (++c == size) c = 0;
            }

            return enumeratedQueens;
        }

        // check if the board is in a resolved state
        bool Check()
        {
            var eq = enumeratedQueens;

            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    if (i == j) continue;

                    if (eq[i].x == eq[j].x ||
                        eq[i].y == eq[j].y ||
                        Math.Abs(eq[i].x - eq[i].y) == Math.Abs(eq[j].x - eq[j].y)
                       )
                        return false;
                }
            }

            return true;
        }

        // count conflicts for a cell
        // TODO: improve this mechanic
        int CountConflicts(int r, int c)
        {
            var eq = enumeratedQueens;

            // conflicts of row, column, and diagonal
            var rc = eq.Count(q => q.x == r);
            var cc = eq.Count(q => q.y == c);
            var dc = eq.Count(q => Math.Abs(q.x - q.y) == 0);

            return rc + cc + dc;
        }
    }
}