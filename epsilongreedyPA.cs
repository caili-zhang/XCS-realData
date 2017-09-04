using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
    class epsilongreedyPA : PredictionArray
    {
        public epsilongreedyPA(MatchSet M)
        {
            // 塩基が2進数(0, 1)
            if (Configuration.Type == "Binary")
            {
                this.PA = new double?[2] { null, null };
                double[] FSA = new double[2] { 0, 0 };

                foreach (Classifier C in M.CList)
                {
                    int index = C.A == '1' ? 0 : 1;

                    if (this.PA[index] == null)
                    {
                        this.PA[index] = C.P * C.F;
                    }
                    else
                    {
                        this.PA[index] += C.P * C.F;
                    }
                    FSA[index] += C.F;
                }

                for (int i = 0; i < this.PA.Length; i++)
                {
                    if (this.PA[i] != null)
                    {
                        if (FSA[i] != 0)
                        {
                            this.PA[i] /= FSA[i];
                        }
                    }
                }
            }
        }

        // 行動選択
        public override char SelectAction()
        {
            if (Configuration.Type == "Binary")
            {
                // ランダム
                if (Configuration.MT_P.NextDouble() < Configuration.P_explr)
                {

                    List<char> Ld = new List<char>();
                    for (int i = 0; i < this.PA.Length; i++)
                    {
                        if (this.PA[i] != null)
                        {
                            Ld.Add((i == 0) ? '0' : '1');   // 0, 1の二つしか記録されない?
                        }
                    }
                    return Ld[(int)(Configuration.MT_P.NextDouble() * Ld.Count)];
                }
                // ベスト
                else
                {
                    int Index = -1;
                    double MAX = -100;

                    // 最大値探し
                    for (int i = 0; i < this.PA.Length; i++)
                    {
                        if (this.PA[i] != null)
                        {
                            if (this.PA[i] > MAX)
                            {
                                Index = i;
                                MAX = (double)this.PA[i];
                            }
                            else if ((this.PA[i] == MAX) && (Configuration.MT_P.NextDouble() < 0.5))
                            {
                                Index = i;
                                MAX = (double)this.PA[i];
                            }
                        }
                    }
                    return (Index == 0) ? '0' : '1';
                }
            }
            return '0'; // 仮
        }

        // マルチステップ用
        public override double MAX()
        {
            double MAX = -100;
            for (int i = 0; i < this.PA.Length; i++)
            {
                if (MAX < this.PA[i])
                {
                    MAX = (double)this.PA[i];
                }
            }

            return MAX;
        }
    }
}