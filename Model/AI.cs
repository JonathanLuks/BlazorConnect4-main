﻿using System;
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
        private static double alpha = 0.1;
        private static double gamma = 0.9;
        private static double epsilon = 0.0;
        private static int iterations = 1;
        private double[][] qTable;
        private static CellColor color;
        private static GameBoard board;


        public QAgent(GameBoard boardFromEngine, CellColor aiColor)
        {
            board = boardFromEngine;
            color = aiColor;

            qTable = new double[7][];
            for (int i = 0; i < 7; i++)
            {
                qTable[i] = new double[6];
            }

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    qTable[j][i] = 0.5;
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
            int state = 0;
            for (int i = 0; i < iterations; i++)
            {
                Debug.WriteLine("Loop: " + i);
                while (true)
                {
                    Debug.WriteLine("Our Color: " + color);
                    state = SelectMove(grid);
                    ge.Play(state);

                    for (int col = 0; col <= 5; col++)
                    {
                        for (int row = 0; row <= 6; row++)
                        {
                            Debug.Write("\t" + grid[row, col].Color);
                        }
                        Debug.WriteLine("");
                    }

                    for (int col = 0; col <= 5; col++)
                    {
                        for (int row = 0; row <= 6; row++)
                        {
                            Debug.Write("\t" + qTable[row][col]);
                        }
                        Debug.WriteLine("");
                    }

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

        private double GetReward(Cell[,] grid, int action)
        {
            GameEngine ge = new GameEngine();
            int row = 0;

            for (int i = 5; i >= 0; i--)
            {
                if (grid[action, i].Color == CellColor.Blank)
                {
                    row = i;
                    break;
                }
            }


            if (IsWin(action, row))
            {
                Debug.WriteLine("WE WOOOOOOOOOOOOOOOOOOOOON POG!!!");
                return 1;
            }
            else if (IsLoss(action, row))
            {
                Debug.WriteLine("gg u suck NOOB");
                return -1;
            }
            else if (ge.IsDraw())
            {
                return 2;
            }
            else if (grid[action, 0].Color != CellColor.Blank)
                return -0.1;
            else
                return 0;
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
            if (GetReward(grid, currentState) == 1)
                return true;
            else if (GetReward(grid, currentState) == -1)
                return true;
            else if (GetReward(grid, currentState) == 2)
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

            int row = 0;


            if (random.NextDouble() < epsilon)
            {
                while (grid[action, 0].Color != CellColor.Blank)
                    action = random.Next(7);
            }
            double saReward = GetReward(grid, action);
            double nsReward = qTable[action].Max();
            double qState = saReward + (gamma * nsReward);
            Debug.WriteLine("\tqstate: " + qState);
            Debug.WriteLine("\taction: " + action);
            //Debug.WriteLine("\tsaReward: " + saReward);
            //Debug.WriteLine("\tnsReward: " + nsReward);
            qTable[action][row] = qState;


            Debug.WriteLine("\tResult: " + (int)Math.Round(qState));

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
