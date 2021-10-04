using System;
using System.IO;
using BlazorConnect4.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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
        private static double alpha = 0.5;
        private static double gamma = 0.9;
        private static double epsilon = 0.5;
        private static int iterations = 10;
        private double[][] qTable;
        private static CellColor color;
        private static GameBoard board;
        private double saReward;
        private int state = 0;
        private int temprow = 0;


        public QAgent(GameBoard boardFromEngine, CellColor aiColor)
        {
            board = boardFromEngine;
            color = aiColor;

            qTable = new double[7][];
            for (int i = 0; i < 7; i++)
            {
                qTable[i] = new double[6];
            }

            Random rd = new Random();

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    qTable[i][j] = 0;//Math.Round(rd.NextDouble(), 2);
                }
            }
        }

        public QAgent(int difficulty, GameBoard boardFromEngine, CellColor aiColor)
        {
            board = boardFromEngine;
            color = aiColor;

            qTable = new double[7][];
            for (int i = 0; i < 7; i++)
            {
                qTable[i] = new double[6];
            }

            if (difficulty == 1)
            {

            }
        }

        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            // Eftersom generatorn inte var serialiserad.
            return temp;
        }

        public void TrainAgents(Cell[,] grid, GameEngine ge)
        {
            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine("Loop: " + i);
                while (true)
                {
                    Console.WriteLine("Our Color: " + color);
                    state = SelectMove(grid);
                    bool temp = ge.Play(state);
                    if (temp)
                    {
                        if (ge.message == ge.Player + " Wins" && ge.Player != (color == CellColor.Yellow ? CellColor.Red : CellColor.Yellow))
                        {

                            saReward = 1;
                            Console.WriteLine("win pog");
                            qTable[state][temprow] = saReward;
                            //int table = qTable.GetHashCode();
                            
                            Console.WriteLine("");

                            for (int col = 0; col <= 5; col++)
                            {
                                for (int row = 0; row <= 6; row++)
                                {
                                    Console.Write("\t" + qTable[row][col]);
                                }
                                Console.WriteLine("");
                            }

                            // return 1
                        }

                        else
                        {
                            saReward = 0;
                            qTable[state][temprow] = saReward;
                            // return 0
                        }
                        break;
                    }

                    else if(!temp && ge.active == false)
                    {
                        saReward = -1;
                        qTable[state][temprow] = saReward;
                        break;
                        // lose
                    }
                    /*
                    for (int col = 0; col <= 5; col++)
                    {
                        for (int row = 0; row <= 6; row++)
                        {
                            Console.Write("\t" + grid[row, col].Color);
                        }
                        Console.WriteLine("");
                    }

                    Console.WriteLine("");

                    for (int col = 0; col <= 5; col++)
                    {
                        for (int row = 0; row <= 6; row++)
                        {
                            Console.Write("\t" + qTable[row][col]);
                        }
                        Console.WriteLine("");
                    }
                    */
                    if (GoalReached(grid, state))
                        break;

                }
            }

            ToFile("Data/Test-AI.bin");

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

        private Tuple<double, int> GetReward(Cell[,] grid, int action)
        {
            //GameEngine ge = new GameEngine();
            int row = 0;

            for (int i = 5; i >= 0; i--)
            {
                if (grid[action, i].Color == CellColor.Blank)
                {
                    row = i;
                    break;
                }
            }

            return Tuple.Create(qTable[action][row], row);

            /*
            if (IsWin(action, row))
            {
                Console.WriteLine("WE WOOOOOOOOOOOOOOOOOOOOON POG!!!");
                return Tuple.Create((double)1, row);
            }
            else if (IsLoss(action, row))
            {
                Console.WriteLine("gg u suck NOOB");
                return Tuple.Create((double)-1, row);
            }
            else if (ge.IsDraw())
            {
                return Tuple.Create((double)2, row);
            }
            else if (grid[action, 0].Color != CellColor.Blank)
                return Tuple.Create(-0.1, row);
            else
                return Tuple.Create((double)0, row);
            */
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

        public bool GoalReached(Cell[,] grid, int currentState)
        {
            if (GetReward(grid, currentState).Item1 == 1)
                return true;
            else if (GetReward(grid, currentState).Item1 == -1)
                return true;
            else if (GetReward(grid, currentState).Item1 == 2)
                return true;
            else
                return false;
        }

        public override int SelectMove(Cell[,] grid)
        {
            Random random = new Random();

            //var validActions = GetValidActions(grid);
            // choose action from e-greedy
            // Temporary:

            int action = random.Next(7);

            if (random.NextDouble() < epsilon)
            {
                while (grid[action, 0].Color != CellColor.Blank)
                    action = random.Next(7);
            }

            else
            {
                var getTuple = GetReward(grid, action);
                saReward = getTuple.Item1; // Reward if a goal is reached
                int row = getTuple.Item2;
                temprow = row;

                double nsReward = qTable[action].Max(); // Next action's best reward
                double q = qTable[action][row]; // Current reward


                double qValue = saReward + (gamma * nsReward);

                //double qValue = q + alpha * (saReward + gamma * nsReward - q); // Q-Learning
                //double value = q + alpha * (r + gamma * maxQ - q);


                Console.WriteLine("\tsaReward: " + saReward);
                //Debug.WriteLine("\tnsReward: " + nsReward);

                Console.WriteLine("\trow: " + row);
                qTable[action][row] = Math.Round(qValue, 2);

                Console.WriteLine("\tqValue: " + qValue);
                Console.WriteLine("\tResult: " + (int)Math.Round(qValue, 2));
            }

            Console.WriteLine("\taction: " + action);
            return action;
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
            bool lose = false;
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
                lose = score == 4;
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
                lose = lose || score == 4;
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

                lose = lose || score == 4;
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

                lose = lose || score == 4;
                score = 0;
            }

            return lose;
        }
    }
}
