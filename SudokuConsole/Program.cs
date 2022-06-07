namespace SudokuConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var data = new byte?[,]
            {
                { null, null, 3, 2, null, null, null, null, null },
                { null, 7, 4, null, null, null, 5, 3, null },
                { null, 6, null, null, 7, null, 9, null, 1 },
                { 6, null, 7, null, 5, 3, 8, 4, 9 },
                { 1, null, 8, 9, 2, 7, 3, 6, null },
                { 5, null, 9, 4, null, null, null, null, null },
                { null, 9, null, null, null, null, null, 1, null },
                { null, null, 6, null, 3, 1, null, null, null },
                { null, null, null, 6, null, 2, 4, null, 3 }
            };

            var board = new Board(data);

            Console.WriteLine("BEFORE Solve():");
            board.Display();

            board.Solve();

            Console.WriteLine("AFTER Solve():");
            board.Display();

            Console.ReadLine();
        }
    }


    public class Board : List<Cell>
    {
        public Board(byte?[,] data)
        {
            for (byte x = 0; x < data.GetLength(0); x += 1)
            {
                for (byte y = 0; y < data.GetLength(1); y += 1)
                {
                    byte? value = data[x, y];

                    Add(new Cell
                    {
                        Row = x,
                        Column = y,
                        Value = value,
                        IsReadOnly = value.HasValue
                    });
                }
            }
        }

        public void Display()
        {
            foreach (var cell in this.OrderBy(x => x.Row).ThenBy(x => x.Column))
            {
                Console.Write($"{cell.Value},");

                if (cell.Column == 8)
                {
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
        }

        public void Solve()
        {
            // Get all cells, grouped by row (as we want to iterate each row)
            var cellsByRow = this.GroupBy(x => x.Row);
            foreach (var row in cellsByRow)
            {
                // for each cell in row...
                for (int i = 0; i < 9; i++)
                {
                    var cell = row.ElementAt(i);
                    if (cell.IsReadOnly) // Already has a value..
                    {
                        continue;
                    }

                    do
                    {
                        FillCellValue(cell);

                        // If a valid number was not found for this cell, then it means we need to go back and try a different number
                        //  for one of the other cells..
                        if (!cell.Value.HasValue && i > 0) // we're going to do (i - 1), so make sure we're not at first cell in row.
                        {
                            Cell prevCell = null;
                            do
                            {
                                // backtrack...
                                for (int j = i - 1; j > 0; j--)
                                {
                                    prevCell = row.ElementAt(j);
                                    if (!prevCell.IsReadOnly)
                                    {
                                        prevCell.AttemptedValues.Add(prevCell.Value!.Value);
                                        prevCell.Value = null;
                                        FillCellValue(prevCell);

                                        if (prevCell.Value.HasValue)
                                        {
                                            break;
                                        }
                                    }
                                }
                            } while (!prevCell.Value.HasValue);
                        }
                    } while (!cell.Value.HasValue);
                }
            }
        }

        private void FillCellValue(Cell cell)
        {
            // Loop numbers 1-9..
            for (byte num = 1; num < 10; num++)
            {
                // If "num" is valid for this cell
                if (!ValueExistsInRow(cell.Row, num) &&
                    !ValueExistsInCol(cell.Column, num) &&
                    !ValueExistsInBlock(cell.BlockId, num))
                {
                    // If the cell has no value AND the "num" hasn't been tried before already..
                    if (!cell.Value.HasValue && !cell.AttemptedValues.Contains(num))
                    {
                        //... let's try it..
                        cell.Value = num;
                        break;
                    }
                }
            }
        }

        private bool ValueExistsInRow(byte row, byte val)
        {
            return this.Any(x => x.Row == row && x.Value == val);
        }

        private bool ValueExistsInCol(byte col, byte val)
        {
            return this.Any(x => x.Column == col && x.Value == val);
        }

        private bool ValueExistsInBlock(byte blockId, byte val)
        {
            return this.Any(x => x.BlockId == blockId && x.Value == val);
        }
    }

    public class Cell
    {
        public byte Row { get; init; }

        public byte Column { get; init; }

        public byte? Value { get; set; }

        /// <summary>
        /// We don't want to overwrite values from initial data set..
        /// </summary>
        public bool IsReadOnly { get; init; }

        public List<byte> AttemptedValues { get; set; } = new List<byte>();

        public byte Stack
        {
            get
            {
                if (Column >= 0 && Column < 3)
                {
                    return 1;
                }
                if (Column >= 3 && Column < 6)
                {
                    return 2;
                }
                if (Column >= 6 && Column < 9)
                {
                    return 3;
                }

                return 0;
            }
        }

        public byte Rank
        {
            get
            {
                if (Row >= 0 && Row < 3)
                {
                    return 1;
                }
                if (Row >= 3 && Row < 6)
                {
                    return 2;
                }
                if (Row >= 6 && Row < 9)
                {
                    return 3;
                }

                return 0;
            }
        }

        public byte BlockId
        {
            get
            {
                switch (Rank)
                {
                    case 1:
                        {
                            switch (Stack)
                            {
                                case 1: return 1;
                                case 2: return 2;
                                case 3: return 3;
                                default: return 0;
                            }
                        }
                    case 2:
                        {
                            switch (Stack)
                            {
                                case 1: return 4;
                                case 2: return 5;
                                case 3: return 6;
                                default: return 0;
                            }
                        }
                    case 3:
                        {
                            switch (Stack)
                            {
                                case 1: return 7;
                                case 2: return 8;
                                case 3: return 9;
                                default: return 0;
                            }
                        }
                    default: return 0;
                }
            }
        }

        public override string ToString() => $"({Row},{Column}): {Value}";
    }
}