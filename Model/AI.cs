using System;
using System.IO;
using BlazorConnect4.Model;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BlazorConnect4.AIModels
{
    [Serializable]
    public abstract class AI
    {
        // Funktion för att bestämma vilken handling som ska genomföras.
        public abstract int SelectMove(Cell[,] grid);

        // Funktion för att skriva till fil.
        public virtual void ToFile(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(stream, this);
            }
        }

        // Funktion för att att läsa från fil.
        protected static AI FromFile(string fileName)
        {
            AI returnAI;
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                returnAI = (AI)bformatter.Deserialize(stream);
            }
            return returnAI;
        }

    }


    [Serializable]
    public class RandomAI : AI
    {
        [NonSerialized] Random generator;

        public RandomAI()
        {
            generator = new Random();
        }

        public override int SelectMove(Cell[,] grid)
        {
            return generator.Next(7);
        }

        public static RandomAI ConstructFromFile(string fileName)
        {
            RandomAI temp = (RandomAI)(AI.FromFile(fileName));
            // Eftersom generatorn inte var serialiserad.
            temp.generator = new Random();
            return temp;
        }
    }


    [Serializable]
    public class QAgent : AI
    {
        private static double alpha = 0.1;
        private static double gamma = 0.9;
        private static double epsilon = 0.5;
        private static int iterations = 100;
        private static CellColor color;
        private static GameBoard board;

        //private List<QState> states { get; set; }
        private HashSet<string> EndStates { get; set; }

        public QAgent(GameBoard boardFromEngine, CellColor aiColor)
        {
            board = boardFromEngine;
            color = aiColor;
        }

        public QAgent(int difficulty, GameBoard boardFromEngine, CellColor aiColor)
        {
            board = boardFromEngine;
            color = aiColor;

            if (difficulty == 1)
            {
                // Load Easy-AI file
                FromFile("Data/Easy-AI.bin");
            }
            else if (difficulty == 2)
            {
                // Load Easy-AI file
                FromFile("Data/Medium-AI.bin");
            }
            else if (difficulty == 3)
            {
                // Load Easy-AI file
                FromFile("Data/Hard-AI.bin");
            }
        }

        public static void TrainAgents()
        {
            Random random = new Random();

            for (int i = 0; i < iterations; i++)
            {
                int startState = random.Next(6);
                while (true)
                {
                    //startState = 
                    break;
                }
            }

            /* Calculate the QValue for the current State:

            loop states in episodes:
                loop actions in states:

                    double q = QEstimated;
                    double r = GetReward;
                    double maxQ = MaxQ(nextStateName);
             
                    double value = q + alpha * (r + gamma * maxQ - q);
                    QValue = value;
             */
        }

        private static double GetReward(int action, Cell[,] grid)
        {
            int row = 0;

            for (int i = 0; i < 7; i++) 
            {
                if (grid[i, action].Color == CellColor.Blank)
                {
                    row = i;
                    break;
                }
            }

            if (IsWin(action, row))
                return 1;
            else if (IsLoss(action,row))
                return -1;
            else if (board.Grid[action, 0].Color != CellColor.Blank)
                return -0.1;
            else
                return 0;
        }

        private static bool IsWin(int action, int row) 
        {
            bool win = false;
            int score = 0;

            if (row < 3)
            {
                for (int i = row; i <= row + 3; i++)
                {
                    if (board.Grid[action, i].Color == color)
                    {
                        score++;
                    }
                }
                win = score == 4;
                score = 0;
            }

            int left = Math.Max(action - 3, 0);

            for (int i = left; i <= action; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i + j <= 6 && board.Grid[i + j, row].Color == color)
                    {
                        score++;
                    }
                }
                win = win || score == 4;
                score = 0;
            }

            int colpos;
            int rowpos;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    colpos = action - i + j;
                    rowpos = row - i + j;
                    if (0 <= colpos && colpos <= 6 &&
                        0 <= rowpos && rowpos < 6 &&
                        board.Grid[colpos, rowpos].Color == color)
                    {
                        score++;
                    }
                }

                win = win || score == 4;
                score = 0;
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    colpos = action + i - j;
                    rowpos = row - i + j;
                    if (0 <= colpos && colpos <= 6 &&
                        0 <= rowpos && rowpos < 6 &&
                        board.Grid[colpos, rowpos].Color == color)
                    {
                        score++;
                    }
                }

                win = win || score == 4;
                score = 0;
            }

            return win;
        }

        private static bool IsLoss(int action, int row) 
        {
            CellColor otherPlayer = color == CellColor.Yellow ? CellColor.Red : CellColor.Yellow;
            bool win = false;
            int score = 0;

            if (row < 3)
            {
                for (int i = row; i <= row + 3; i++)
                {
                    if (board.Grid[action, i].Color == otherPlayer)
                    {
                        score++;
                    }
                }
                win = score == 4;
                score = 0;
            }

            int left = Math.Max(action - 3, 0);

            for (int i = left; i <= action; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i + j <= 6 && board.Grid[i + j, row].Color == otherPlayer)
                    {
                        score++;
                    }
                }
                win = win || score == 4;
                score = 0;
            }

            int colpos;
            int rowpos;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    colpos = action - i + j;
                    rowpos = row - i + j;
                    if (0 <= colpos && colpos <= 6 &&
                        0 <= rowpos && rowpos < 6 &&
                        board.Grid[colpos, rowpos].Color == otherPlayer)
                    {
                        score++;
                    }
                }

                win = win || score == 4;
                score = 0;
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    colpos = action + i - j;
                    rowpos = row - i + j;
                    if (0 <= colpos && colpos <= 6 &&
                        0 <= rowpos && rowpos < 6 &&
                        board.Grid[colpos, rowpos].Color == otherPlayer)
                    {
                        score++;
                    }
                }

                win = win || score == 4;
                score = 0;
            }

            return win;
        }

        private int[] GetValidActions(Cell[,] grid)
        {
            List<int> validActions = new List<int>();

            //return Board.Grid[col, 0].Color == CellColor.Blank;

            for (int i = 0; i < 7; i++)
            {
                if (grid[i, 0].Color == CellColor.Blank)
                {
                    validActions.Add(i);
                }
            }

            return validActions.ToArray();
        }

        public bool GoalReached(int currentState)
        {
            return currentState == 1;   
        }

        public override int SelectMove(Cell[,] grid)
        {
            throw new NotImplementedException();

            //var validActions = GetValidActions(currentState)
        }
    }
}
