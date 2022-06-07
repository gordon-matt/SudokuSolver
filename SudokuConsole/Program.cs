using Extenso;
using Extenso.Collections;

namespace SudokuConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Load data from file to byte[,]
            var data = new byte?[9,9];

            var fileContent = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.csv"));
            var rows = fileContent.ToLines().ToArray();

            for (int row = 0; row < 9; row++)
            {
                var values = rows[row].Split(',').ToListOf<byte>();
                for (int col = 0; col < 9; col++)
                {
                    byte val = values[col];
                    data[row, col] = val == 0 ? null : val;
                }
            }

            // Create Board with loaded data
            var board = new Board(data);

            Console.WriteLine("BEFORE Solve():");
            board.Display();

            bool solved = board.Solve();
            if (!solved)
            {
                Console.WriteLine("This board is not valid. It cannot be solved.");
            }
            else
            {
                Console.WriteLine("AFTER Solve():");
                board.Display();
            }

            Console.ReadLine();
        }
    }


    public class Board : List<Cell>
    {
        public Board(byte?[,] data)
        {
            // Iterate first dimension (rows)
            for (byte x = 0; x < data.GetLength(0); x += 1)
            {
                // Iterate second dimension (cols)
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
                Console.Write($"{cell.Value ?? 0} | ");

                if (cell.Column == 8)
                {
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
        }

        public bool Solve()
        {
            // Get an empty cell
            var cell = GetEmptyCell();
            if (cell == null)
            {
                return true;
            }

            // Fill it with a possible value
            FillCellValue(cell);

            // If there is a value, let's use recursion to find another empty block and continue..
            if (cell.Value.HasValue)
            {
                return Solve();
            }

            //Console.WriteLine($"Value not found for ({cell.Row},{cell.Column})…");

            // We need to go back and try a different number for one of the previously filled cells..
            do
            {
                // backtrack...
                cell.AttemptedValues.Clear();
                cell = GetPreviousCell(cell);

                if (cell == null)
                {
                    return false;
                }

                cell.AttemptedValues.Add(cell.Value.Value);
                cell.Value = null;
                FillCellValue(cell);
            } while (!cell.Value.HasValue);

            // If there is a value, let's use recursion to find another empty block and continue..
            if (cell.Value.HasValue)
            {
                return Solve();
            }

            return false;
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
                        //Console.WriteLine($"Attempting value {num} for ({cell.Row},{cell.Column})…");
                        //... let's try it..
                        cell.Value = num;
                        break;
                    }
                }
            }
        }

        private Cell GetEmptyCell()
        {
            return this
                .OrderBy(x => x.Row) // Order by row, then column.. so that we solve the board left-to-right, top-to-bottom
                .ThenBy(x => x.Column)
                .FirstOrDefault(x => !x.Value.HasValue);
        }

        private Cell GetPreviousCell(Cell cell)
        {
            // First try to get previous cell in same row
            var result = this
                .Where(x => x.Row == cell.Row && x.Column < cell.Column && !x.IsReadOnly)
                .OrderByDescending(x => x.Column)
                .FirstOrDefault();

            if (result != null)
            {
                return result;
            }

            // Then try last cell in previous row
            var cells = this.Where(x => x.Row == cell.Row - 1 && !x.IsReadOnly);
            if (!cells.Any())
            {
                return null;
            }

            byte col = cells.Max(x => x.Column);

            result = this
                .Where(x => x.Row == cell.Row - 1 && x.Column == col)
                .FirstOrDefault();

            return result;
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