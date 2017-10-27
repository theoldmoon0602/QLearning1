using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyQLProblems;
using System.IO;

namespace QLearning1
{
    class QValues
    {
        // Q値
        private List<List<double>> values;
        private int stateCount;
        private int actionCount;

        // ひでえ名前だ
        private double learningRate;
        private double discountRate;

        // 初期化（0で初期化）
        public QValues(int stateCount, int actionCount, double learningRate, double discountRate)
        {
            this.stateCount = stateCount;
            this.actionCount = actionCount;
            this.learningRate = learningRate;
            this.discountRate = discountRate;

            values = new List<List<double>>();
            for (int i = 0; i < stateCount; i++)
            {
                values.Add(Enumerable.Repeat<double>(0, actionCount).ToList());
            }
        }

        public void Update(int state, int action, int nextState, double reward)
        {
            values[state][action] =
                (1 - learningRate) * values[state][action] +
                learningRate * (reward + discountRate * values.ElementAt(nextState).Max());
        }

        public void Show()
        {
            Console.Write(new string(' ', 4) + '|');
            for (int i = 0; i < stateCount; i++)
            {
                Console.Write("{0,4} ", "s" + i);
            }
            Console.WriteLine("\n" + new string('-', 5 * stateCount));
            for (int i = 0; i < actionCount; i++)
            {
                Console.Write("{0,4}|", "a" + i);
                for (int j = 0; j < stateCount; j++)
                {
                    Console.Write("{0,4} ", (int)values[j][i]);
                }
                Console.WriteLine();
            }
        }

        public int GetNextAction(int state, Random random = null)
        {
            if (random is null)
            {
                random = new Random();
            }
            if (random.NextDouble() < 0.1)
            {
                return random.Next(actionCount);
            }

            var vs = values.ElementAt(state);

            int maxIndex = 0;
            double v = vs.ElementAt(0);
            for (int i = 0; i < vs.Count; i++)
            {
                if (vs.ElementAt(i) > v)
                {
                    v = vs.ElementAt(i);
                    maxIndex = i;
                }
            }

            return maxIndex;
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            // 統計をとるよ
            List<int> actionCountsOnLastEpisode = new List<int>();
            List<double> rewardsOnLastEpisode = new List<double>();

            // 試行ごとのエピソード数
            const int EpisodeNum = 1000;
            const int N = 100; // N回回して統計をとる

            int[] allActionCounts = new int[EpisodeNum];
            double[] allSumOfRewards = new double[EpisodeNum];


            // N回回してみる
            for (int c = 0; c < N; c++)
            {
                // 毎回変わればなんでもいい
                Random random = new Random(c+10000);

                int actionCounts = 0;
                double sumOfRewards = 0;

                // very magic numbers
                QValues qValues = new QValues(10, 3, 0.1, 0.3);
                Problem1.initState();


                // エピソードを回す
                for (int i = 0; i < EpisodeNum; i++)
                {
                    // 初期化
                    Problem1.setState(0);

                    int actionCount = 0;
                    double reward = 0;

                    // 学習
                    while (!Problem1.getStateIsGoal())
                    {
                        int s = Problem1.getState();
                        int act = qValues.GetNextAction(s, random);
                        reward = Problem1.doAction(act.ToString());

                        actionCount++;
                        sumOfRewards += reward;

                        int next = Problem1.getState();

                        //Console.WriteLine("Current State   ==> {0}", s);
                        //Console.WriteLine("Selected Action ==> {0}", act);
                        //Console.WriteLine("Next State is   ==> {0}", next);
                        qValues.Update(s, act, next, reward);

                        if (Problem1.getStateIsGoal()) { break; }
                    }
                    //Console.WriteLine("GOAL!!!\n\n");

                    actionCounts += actionCount;
                    sumOfRewards += reward;
                    if (i + 1 == EpisodeNum)
                    {
                        actionCountsOnLastEpisode.Add(actionCount);
                        rewardsOnLastEpisode.Add(reward);
                    }
                    if (c == 0) { 
                        allActionCounts[i] = actionCounts;
                        allSumOfRewards[i] = sumOfRewards;
                    }

                }
                //qValues.Show();
            }

            List<double> rewardRates = allActionCounts.Zip(allSumOfRewards, (count, reward) => reward / count).ToList();

            // 統計値を出力
            using (StreamWriter sw = new StreamWriter("output.csv"))
            {
                sw.Write("Average of action count on last episodes,");
                sw.Write(actionCountsOnLastEpisode.Average());
                sw.WriteLine(",");

                sw.Write("Average of reward on last episodes,");
                sw.Write(rewardsOnLastEpisode.Average());
                sw.WriteLine(",");

                sw.WriteLine("reward rate,");
                foreach (var rewardRate in rewardRates)
                {
                    sw.Write(rewardRate);
                    sw.WriteLine(",");
                }
            }
        }
    }
}
