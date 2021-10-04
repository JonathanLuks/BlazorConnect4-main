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
        private double[][] qTable;
        private static CellColor color;
        private static GameEngine ge;
        private int state = 0;

        public QAgent(GameEngine gameEngine, CellColor aiColor)
        {
            ge = gameEngine;
            color = aiColor;

            qTable = new double[7][];
            for (int i = 0; i < 7; i++)
            {
                qTable[i] = new double[6];
            }
        }

        public static QAgent ConstructFromFile(string FilePath, GameEngine gameEngine, CellColor aiColor)
        {
            ge = gameEngine;
            color = aiColor;
            
            return (QAgent)FromFile(FilePath);
        }

        public void TrainAgents()
        {
            int state = 0;
            for (int i = 0; i < 100; i++)
            {
                Debug.WriteLine("Loop: " + i);
                while (true)
                {
                    state = SelectMove(ge.Board.Grid);
                    if (ge.Play(state))
                    { 
                        break;
                    }

                    for (int col = 0; col <= 5; col++)
                    {
                        for (int row = 0; row <= 6; row++)
                        {
                            Debug.Write("\t" + ge.Board.Grid[row, col].Color);
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

                    if (GoalReached(ge.Board.Grid, state))
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
            int row = 0;

            for (int i = 5; i >= 0; i--)
            {
                if (grid[action, i].Color == CellColor.Blank)
                {
                    row = i;
                    break;
                }
            }

            if (ge.IsWin(action, row))
                return 1;
            /*else if (IsLoss(action, row))
                return -1;*/
            else if (ge.Board.Grid[action, 0].Color == CellColor.Blank)
                return 0;
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
            GetReward(grid, currentState);
            return currentState == 1;
        }

        public override int SelectMove(Cell[,] grid)
        {
            Random random = new Random();

            int[] validActions = GetValidActions(grid);
            int action = validActions[random.Next() % validActions.Length];

            
            if (random.NextDouble() < epsilon)
            {
                while (grid[action, 0].Color != CellColor.Blank)
                    action = random.Next(7);
            }
            else
            {
                double saReward = GetReward(grid, action);
                double nsReward = qTable[action].Max();
                double qState = saReward + (gamma * nsReward);
                Debug.WriteLine("qstate: " + qState);
                Debug.WriteLine("action: " + action);
                Debug.WriteLine("saReward: " + saReward);
                Debug.WriteLine("nsReward: " + nsReward);
                qTable[action][grid.GetLength(1) - 1] = qState;
            }

            return action;
        }
    }
}
