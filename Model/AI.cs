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
        private double alpha = 0.1;
        private double gamma = 0.9;
        private int episodes = 100;

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
            else if (difficulty == 2)
            {
                // Load Medium-AI file
                if (File.Exists("Data/Medium-AI"))
                {
                    FromFile("Data/Medium-AI");
                }
            }
            else if (difficulty == 3)
            {
                // Load Hard-AI file
                if (File.Exists("Data/Hard-AI"))
                {
                    FromFile("Data/Hard-AI");
                }
            }
        }

        public static void TrainAgents()
        {
            /* Calculate the QValue for the current State:
            double q = QEstimated;
            double r = GetReward;
            double maxQ = MaxQ(nextStateName);
             
            double value = q + alpha * (r + gamma * maxQ - q);
            QValue = value;
             */
        }

        private static void GetReward()
        {
            /*
             if (won)
                return 1
             else if (lost)
                return -1
             else if NotValidMove
                return -0.1
             else
                return 0
             */
        }

        public override int SelectMove(Cell[,] grid)
        {
            throw new NotImplementedException();
        }
    }
}
