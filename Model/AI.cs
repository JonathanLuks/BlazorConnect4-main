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
        private static double epsilon = 0.1;
        private static int iterations = 20000;
        private static GameEngine ge;
        private double saReward;
        private int action = 0;
        private string state;
        private Dictionary<string, double[]> states = new Dictionary<string, double[]>();
        private int wins = 0;
        private int loses = 0;
        private int draws = 0;

        public QAgent(GameEngine gameEngine)
        {
            ge = gameEngine;
        }


        public static QAgent ConstructFromFile(string FilePath, GameEngine gameEngine)
        {
            ge = gameEngine;
           
            return (QAgent)FromFile(FilePath);
        }


        public void TrainAgents(GameEngine ge)
        {
            for (int i = 0; i < iterations; i++)
            {
                //Console.WriteLine("Loop: " + i);
                while (true)
                {
                    if(ge.Player == CellColor.Red)
                        action = SelectMove(ge.Board.Grid);
                    else 
                    {
                        Random random = new Random();
                        action = random.Next(7);
                        while (ge.Board.Grid[action, 0].Color != CellColor.Blank)
                            action = random.Next(7);
                    }

                    //Console.WriteLine("Our Color: " + ge.Player);
                    //Console.WriteLine("Action: " + action);

                    bool temp = ge.Play(action);

                    state = Hash(ge.Board.Grid);

                    if (!states.ContainsKey(state))
                    {
                        //initializes new state with values of 0
                        double[] actionTable = new Double[7];
                        for (int k = 0; k < 7; k++)
                            actionTable[k] = 0;
                        states.Add(state, actionTable);
                        //Console.WriteLine("lagger in nytt varde!");
                    }

                    if (temp)
                    {
                        if (ge.message == ge.Player + " Wins" && ge.Player == CellColor.Red)
                        {
                            //Console.WriteLine("Win");
                            saReward = 1;
                            wins++;
                        }
                        else if (ge.message == ge.Player + " Wins" && ge.Player == CellColor.Yellow)
                        {
                            //Console.WriteLine("Lose");
                            saReward = -1;
                            loses++;
                        }
                        else
                        {
                            //Console.WriteLine("Draw");
                            saReward = -0.1;
                            draws++;
                        }
                        break;
                    }
                }

                double[] oldValues = states[state];
                double maxVal = 0;

                for (int k = 0; k < 7; k++)
                {
                    if (oldValues[k] > maxVal)
                    {
                        maxVal = oldValues[k];
                    }
                }

                double currentQ = oldValues[action];
                double r = saReward;

                double? maxQ = null;

                foreach (var nextState in states.Values)
                {
                    foreach (var result in nextState)
                    {
                        double val = result;

                        if (val > maxQ || !maxQ.HasValue)
                        {
                            maxQ = val;
                        }
                    }
                }

                double newQ = currentQ + alpha * (r + gamma * (double)maxQ - currentQ);

                states[state][action] = newQ;

                if (i + 1 != iterations)
                {
                    ge.Board = new GameBoard();
                    ge.Player = CellColor.Red;
                    ge.active = true;
                    ge.message = "Starting new game";
                }
                //Console.WriteLine("\n");
            }
            Console.WriteLine(draws);
            Console.WriteLine("\n");
            Console.WriteLine(wins);
            Console.WriteLine("\n");
            Console.WriteLine(loses);
            ToFile("Data/Test-AI.bin");
        }

        private string Hash(Cell[,] grid) 
        {
            string hashOfState = String.Empty;
            for (int col = 0; col <= 6; col++)
                for (int row = 0; row <= 5; row++)
                    hashOfState += ((int)grid[col,row].Color + 1);
            return hashOfState;
        }

        public override int SelectMove(Cell[,] grid)
        {
            Random random = new Random();
            int action = 0;

            if (random.NextDouble() < epsilon)
            {
                while (grid[action, 0].Color != CellColor.Blank)
                    action = random.Next(7);
            }
            else
            {
                string state = Hash(ge.Board.Grid);
                if (states.ContainsKey(state))
                {
                    double[] values = states[state];
                    double maxVal = Double.MinValue; //Min if there is only negative numbers
                    int indexForMaxVal = 0;

                    for (int i = 0; i < 7; i++)
                    {
                        if (values[i] > maxVal)
                        {
                            maxVal = values[i];
                            indexForMaxVal = i;
                        }
                    }
                    action = indexForMaxVal;
                }
                else
                {
                    action = random.Next(7);
                }
            }
            return action;
        }      
    }
}
