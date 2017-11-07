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

        // ある状態の時、次に取るべき行動を返す
        public int GetNextAction(int state, Random random = null)
        {
            // 10% でランダムに
            if (random is null)
            {
                random = new Random();
            }
            if (random.NextDouble() < 0.1)
            {
                return random.Next(actionCount);
            }

            // それ以外は最善と思っている手段で
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
            const int EpisodeNum = 1000; // 試行ごとのエピソード数
            const int N = 100; // N回回して統計をとる

            List<int> actionCounts = new List<int>();
            List<double> rewards = new List<double>();
            double[,] rewardRates = new double[EpisodeNum, N];

            // N回回してみる
            for (int c = 0; c < N; c++)
            {
                // 毎回変わればなんでもいい
                Random random = new Random(c + 10000);

                // very magic numbers
                QValues qValues = new QValues(10, 3, 0.1, 0.3);
                Problem1.initState();

                long allActionCount = 0; // 累計行動回数
                double sumOfRewards = 0; // 累計獲得報酬


                // エピソードを回す
                for (int i = 0; i < EpisodeNum; i++)
                {
                    // 初期化
                    Problem1.setState(0);
                    double reward = 0;
                    int actionCount = 0;

                    // 学習
                    while (!Problem1.getStateIsGoal())
                    {
                        int currentState = Problem1.getState();
                        int act = qValues.GetNextAction(currentState, random);
                        reward = Problem1.doAction(act.ToString());

                        int nextState = Problem1.getState();

                        //Console.WriteLine("Current State   ==> {0}", s);
                        //Console.WriteLine("Selected Action ==> {0}", act);
                        //Console.WriteLine("Next State is   ==> {0}", next);
                        qValues.Update(currentState, act, nextState, reward);
                        actionCount++;

                        allActionCount++;
                        sumOfRewards += reward;
                    }

                    // 最終エピソードにおける行動実行回数と獲得報酬を見る
                    if (i+1 == EpisodeNum)
                    {
                        actionCounts.Add(actionCount);
                        rewards.Add(reward);
                    }
                    rewardRates[i, c] = sumOfRewards / allActionCount;

                }
                //qValues.Show();
            }
            Console.WriteLine("Avg of actionCount: {0}", actionCounts.Average());
            Console.WriteLine("Avg of reward: {0}", rewards.Average());

            for (int i = 0; i < EpisodeNum; i++) 
            {
                double rewardRateAvg = 0;
                for (int c = 0; c < N; c++)
                {
                    rewardRateAvg += rewardRates[i, c];
                }
                rewardRateAvg /= N;
                Console.WriteLine("{0},", rewardRateAvg);
            }
        }
    }
}
