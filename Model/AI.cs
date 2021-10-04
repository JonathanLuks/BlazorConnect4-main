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
        private static double alpha = 1.0;
        private static double gamma = 0.9;
        private static double epsilon = 1.0;
        private static int iterations = 100;

        private double[][] qTable;

        private static CellColor color;
        private static GameBoard board;

        private double saReward = 0;
        private int state = 0;
        private int temprow = 0;
        double qValue = 0;
        int action = 0;

        public QAgent(GameBoard boardFromEngine, CellColor aiColor)
        {
            board = boardFromEngine;
            color = aiColor;

            qTable = new double[7][];
            for (int i = 0; i < 7; i++)
            {
                qTable[i] = new double[20];
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
                while (true)
                {
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

                            // return 1
                        }

                        else
                        {
                            saReward = 0;
                            qTable[state][temprow] = saReward;

                            Console.WriteLine("DRAW!!!");

                            // return 0
                        }

                        break;
                    }

                    else if (!temp && ge.active == false)
                    {
                        saReward = -1;
                        qTable[state][temprow] = saReward;

                        Console.WriteLine("Lose :(");

                        break;
                        // lose
                    }

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
            }

            ToFile("Data/Test-AI.bin");
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
        }

        public override int SelectMove(Cell[,] grid)
        {
            Random random = new Random();

            // choose action from e-greedy
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


                //double qValue = saReward + (gamma * nsReward);

                qValue = q + alpha * (saReward + gamma * nsReward - q); // Q-Learning
                //double value = q + alpha * (r + gamma * maxQ - q);


                Console.WriteLine("\tsaReward: " + saReward);
                //Debug.WriteLine("\tnsReward: " + nsReward);

                Console.WriteLine("\trow: " + row);
                qTable[action][row] = Math.Round(qValue, 2);

                Console.WriteLine("\tqValue: " + qValue);
                Console.WriteLine("\tResult: " + (int)Math.Round(qValue, 2));

                action = qTable[action].ToList().FindIndex(x => x == nsReward);
            }

            epsilon *= 0.9;

            Console.WriteLine("\taction: " + action);
            return action;
        }
    }
}