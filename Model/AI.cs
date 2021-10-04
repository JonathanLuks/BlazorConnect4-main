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
        private static int iterations = 100;
        private double[][] qTable;
        private static CellColor color;
        private static GameEngine ge;
        private double saReward;
        private int action = 0;
        private string state;
        private Dictionary<string, double[]> states = new Dictionary<string, double[]>();
        public QAgent(GameEngine gameEngine, CellColor aiColor)
        {
            ge = gameEngine;
            color = aiColor;

            qTable = new double[42][];
            for (int i = 0; i < 42; i++)
            {
                qTable[i] = new double[7];
            }
            
            Random rd = new Random();

            for (int i = 0; i < 42; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    qTable[i][j] = 0;
                }
            }
        }

        public static QAgent ConstructFromFile(string FilePath, GameEngine gameEngine, CellColor aiColor)
        {
            ge = gameEngine;
            color = aiColor;
           
            return (QAgent)FromFile(FilePath);
        }

        public void TrainAgents(Cell[,] grid, GameEngine ge)
        {
            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine("Loop: " + i);
                while (true)
                {
                    Console.WriteLine("Our Color: " + color);
                    action = SelectMove(grid);

                    bool temp = ge.Play(action);
                    if (temp)
                    {
                        if (ge.message == ge.Player + " Wins" && ge.Player != (color == CellColor.Yellow ? CellColor.Red : CellColor.Yellow))
                        {
                            saReward = 1;
                        }

                        else
                        {
                            saReward = 0;
                        }
                        break;
                    }

                    else if(!temp && ge.active == false)
                    {
                        saReward = -1;
                        break;
                    }
                }

                string state = Hash(ge.Board.Grid);
                if (states.ContainsKey(state))
                {
                    double[] oldValues = states[state];
                    double maxVal = 0;
                    int indexForMaxVal = 0;

                    for (; indexForMaxVal < 7; indexForMaxVal++)
                    {
                        if (oldValues[indexForMaxVal] > maxVal)
                        {
                            maxVal = oldValues[indexForMaxVal];
                            break;
                        }
                    }
                    double currentQ = oldValues[action];
                    double r = saReward;
                }
                else
                {
                    //initializes new state with values of 0
                    for (int k = 0; i < 7; k++)
                        states[state][k] = 0;
                }

                ge.Board = new GameBoard();
            }

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

        private Tuple<double, int> GetReward(Cell[,] grid, int action)
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

            return Tuple.Create(qTable[state][action], row);
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
               //TODO
            }

            Console.WriteLine("\taction: " + action);
            return action;
        }      
    }
}
