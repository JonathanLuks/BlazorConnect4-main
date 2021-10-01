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

        //private List<QState> states { get; set; }
        private HashSet<string> EndStates { get; set; }

        public QAgent()
        {
            
        }

        public QAgent(int difficulty)
        {
            if (difficulty == 1)
            {
                // Load Easy-AI file
                if (File.Exists("Data/Easy-AI"))
                {
                    FromFile("Data/Easy-AI");
                }
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

        private static Cell GetReward(int currentState, int action, Cell[,] grid)
        {
            /*
             if (IsWin(currentState, action))
                return 1
             else if (lost)
                return -1
             else if NotValidMove
                return -0.1
             else
                return 0
            */
            return grid[currentState, action];
        }

        private int[] GetValidActions(int currentState, Cell[,] grid)
        {
            List<int> validActions = new List<int>();

            //return Board.Grid[col, 0].Color == CellColor.Blank;

            for (int i = 0; i < 7; i++)
            {
                if (grid[currentState, i].Color == CellColor.Blank)
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
