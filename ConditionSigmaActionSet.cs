using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace XCS
{
    class ConditionSigmaActionSet : ActionSet
    {
        public ConditionSigmaActionSet(List<Classifier> actSet)
        {
            this.CList = actSet;

        }

        public override void Show()
        {
            StreamWriter sw = new StreamWriter("./ActionSet_" + Configuration.T + ".csv", true, System.Text.Encoding.GetEncoding("shift_jis"));
            if (Configuration.ASName != "CS" && Configuration.ASName != "MaxCS" && Configuration.ASName != "Max" && Configuration.ASName != "Updatee0CS")
            {
                sw.WriteLine("state,action,prediction,epsilon,fitness,numerosity,experience,timestamp,actionsetsize,accuracy,epsilon_0,selectTime,mean,std,generateTime,generality");
                foreach (Classifier C in this.CList)
                {


                    sw.WriteLine(C.C.state + "," /*+ C.A + ","*/ + C.P + "," + C.Epsilon + "," + C.F + "," + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.M + "," + Math.Sqrt(C.S / (C.St - 1)) + "," + C.GenerateTime + "," + C.C.Generality);
                }
            }
            else
            {
                sw.WriteLine("time,state,action,prediction,epsilon,fitness,numerosity,experience,timestamp,actionsetsize,accuracy,epsilon_0,selectTime,mean,std,generateTime,generality,convergence");
                foreach (SigmaNormalClassifier C in this.CList)
                {


                    sw.WriteLine(Configuration.T + "," + C.C.state + "," /*+ C.A + ","*/ + C.P + "," + C.Epsilon + "," + C.F + ","
                        + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.M + "," + Math.Sqrt(C.S / (C.St - 1)) + "," + C.GenerateTime + "," + C.C.Generality + "," + (C.IsConvergenceEpsilon() ? 1 : 0));
                }
            }
            sw.Close();

        }


        public override void Update(Population Pop, double P, StdList Sigma)
        {
            double SumNumerosity = 0;
            foreach (Classifier C in this.CList)
            {
                SumNumerosity += C.N;

            }

            foreach (Classifier C in this.CList)
            {
                C.Exp++;

                if (C.Exp < 1 / Configuration.Beta)
                {
                    C.P += (P - C.P) / C.Exp;
                    C.As += (SumNumerosity - C.As) / C.Exp;
                }
                else
                {
                    C.P += Configuration.Beta * (P - C.P);
                    C.As += Configuration.Beta * (SumNumerosity - C.As);
                }

                // 標準偏差計算
                C.St++;
                double X = P - C.M;
                C.M += X / C.St;
                C.S += (C.St - 1) * X * X / C.St;//あっている？？
                if (double.IsNaN(C.S))
                {
                    Console.ReadLine();
                }


                if (C.GetType().Name == "SigmaNormalClassifier")
                {
                    // このIterationまでのepsilonを記録ずらし
                    SigmaNormalClassifier SNC = (SigmaNormalClassifier)C;
                    for (int index = SNC.EpsilonList.Count() - 1; index > 0; index--)
                    {
                        SNC.EpsilonList[index] = SNC.EpsilonList[index - 1];
                    }

                    SNC.EpsilonList[0] = Math.Sqrt(C.S / (C.St - 1));

                    #region　分類子が照合する全状態のVTの分散と平均を使って　e0 を推測　　chou 160107
                    //分類子が照合する全状態のVTの分散と平均を使って　e0 を推測　　chou 160107
                    if (Configuration.IsConvergenceVT)
                    {
                        if (C.Exp > 2)//0120 cho 
                        {
                            if (SNC.IsConvergenceEpsilon())//分類子のstd が収束するときepsilon を更新
                            {
                                C.Epsilon = SNC.EpsilonList[0];
                            }
                            else//epsilon収束しない場合　強化学習
                            {
                                if (C.Exp < Configuration.Beta)
                                {
                                    C.Epsilon += (Math.Abs(P - C.P) - C.Epsilon) / C.Exp;
                                }
                                else
                                {
                                    C.Epsilon += Configuration.Beta * (Math.Abs(P - C.P) - C.Epsilon);
                                }
                            }
                        }
                        Classifier cl0 = new NormalClassifier();

                        cl0.S = 0;
                        cl0.M = 0;
                        cl0.St = 0;
                        List<StdList> cpStdLists = new List<StdList>();

                        foreach (var std in Configuration.ConvergentedVT)
                        {

                            if (std.IsIncluded(C.C.state))
                            {
                                cpStdLists.Add(std.Clone()); //クローンメソット　　
                            }

                        }
                        if (cpStdLists.Count == 1)
                        {
                            C.Epsilon_0 = cpStdLists[0].S + Configuration.Epsilon_0;

                        }
                        else
                        {
                            foreach (var std in cpStdLists)
                            {
                                //St 出現回数
                                cl0.St += std.T;
                                cl0.M += std.M * std.T;

                            }

                            //ここst= 0 , cl0.M がNULLになる
                            cl0.M = cl0.M / cl0.St;

                            foreach (var std in cpStdLists)
                            {
                                cl0.S += std.T * Math.Pow(std.S, 2) + std.T * Math.Pow((cl0.M - std.M), 2);
                            }

                            cl0.S = Math.Sqrt(cl0.S / cl0.St);

                            C.Epsilon_0 = cl0.S + Configuration.Epsilon_0;

                        }

                    }
                    #endregion

                    #region tatumi 160106 XCS-SAC  VTが全部収束したら　加重平均でe0更新
                    //if (Configuration.IsConvergenceVT)
                    //{
                    //    //tatumi 160106 XCS-SAC  VTが全部収束したら　加重平均でe0更新
                    //    if (C.Exp > 2 && SNC.IsConvergenceEpsilon())//0120 cho 
                    //    {

                    //        C.Epsilon = SNC.EpsilonList[0];
                    //    }

                    //    double WeightedSum = 0;
                    //    int WeightedCount = 0;
                    //    foreach (StdList SL in Configuration.Stdlist)
                    //    {
                    //        if (SL.IsIncluded(C.C.state))
                    //        {
                    //            if (!double.IsNaN(SL.S))
                    //            {
                    //                WeightedSum += (SL.T - 1) * Math.Pow(SL.S, 2);
                    //                WeightedCount += SL.T;
                    //            }
                    //        }
                    //    }
                    //    // 下駄適応済み
                    //    if (WeightedCount > 1)
                    //    {
                    //        WeightedSum = Math.Sqrt(WeightedSum / (WeightedCount - 1)) + Configuration.Epsilon_0;
                    //    }
                    //    else
                    //    {
                    //        WeightedSum = Configuration.Epsilon_0;
                    //    }
                    //    C.Epsilon_0 = WeightedSum;
                    //}
                    //else
                    //{
                    //    if (C.Exp < Configuration.Beta)
                    //    {
                    //        C.Epsilon += (Math.Abs(P - C.P) - C.Epsilon) / C.Exp;
                    //    }
                    //    else
                    //    {
                    //        C.Epsilon += Configuration.Beta * (Math.Abs(P - C.P) - C.Epsilon);
                    //    }
                    //}
                    #endregion
                }
                else //SigmaNormalClassifier　ではない場合
                {
                    //if( C.Exp < Configuration.Beta )//9-23 張　ここscript は　exp<Beta
                    if (C.Exp < Configuration.Beta)
                    {
                        C.Epsilon += (Math.Abs(P - C.P) - C.Epsilon) / C.Exp;
                    }
                    else
                    {
                        C.Epsilon += Configuration.Beta * (Math.Abs(P - C.P) - C.Epsilon);
                    }
                }
            }

            this.UpdateFitness();

            if (Configuration.DoActionSetSubsumption)
            {
                this.Subsumption(Pop);
                if (Pop.CountNumerosity() > 400)
                {
                    Console.WriteLine(" stop!!");
                }
               
            }
        }

        protected override void UpdateFitness()
        {
            
            /////////////////////////////////////////////////////////////

            double AccuracySum = 0;

            foreach (Classifier C in this.CList)
            {
                //if (C.C.state == "####0##1")
                //{
                //    Console.ReadLine();
                //}
                SigmaNormalClassifier SNC = (SigmaNormalClassifier)C;

                if (Configuration.T > 1000)
                {
                    //if (//またがるのものを排除 平均プラマイ　Pのプラマイ

                    //         (C.M - C.Epsilon) < Configuration.RewardAverage
                    //         && (C.M + C.Epsilon) > Configuration.RewardAverage
                    //    )
                    //{//またがる分類子の正確性を極端に下げる　PS：0にしてはいけない
                    // //またがる部分とEの割合でKappaの正確性を下げていく、またがる部分が大きいければ　下がるのが早い
                    //    double crossPercentage;
                    //    double mincross;
                    //    //puls minus 0.5e どうなるかな0128 ダメだった　余計なもの残した
                    //    mincross = Math.Min(Math.Abs(C.M + C.Epsilon - Configuration.RewardAverage),
                    //        Math.Abs(C.M - C.Epsilon - Configuration.RewardAverage));
                    //    //crossPercentage = mincross / (C.Epsilon);
                    //    crossPercentage = mincross / Configuration.RewardAverage;//12/11変更、許容範囲の図り方、平均値と比較

                    //    //規制緩和　c.epsilon の　20% ぐらいまたがる　を許す、緩和しない基本的に+-epsilon 十分緩いい
                    //    //±σ　相当　片側85% と　片側15% の関係

                    //    if (crossPercentage < Configuration.CoverPersentage)//許容範囲で通常のやり方
                    //    {
                    //        if (C.Epsilon < C.Epsilon_0)
                    //        {
                    //            C.Kappa = 1;
                    //        }
                    //        else
                    //        {//epsilon>epsilon0 の場合　
                    //            C.Kappa = Configuration.Alpha * Math.Pow(C.Epsilon / C.Epsilon_0, -Configuration.Nyu);
                    //        }
                    //    }
                    //    else//許容範囲を超えた
                    //    {
                    //        C.Kappa = Configuration.Alpha * Math.Pow(1 + crossPercentage, -Configuration.Nyu);
                    //        // C.Kappa = Math.Pow(Math.E,-Math.Pow(5*crossPercentage,2));
                    //    }


                    //    AccuracySum += C.Kappa * C.N;
                    //    if (double.IsNaN(AccuracySum))
                    //    {
                    //        Console.ReadLine();
                    //    }

                    //}
                    //else //またがらない分類子は 通常のやり方
                    //{
                        if (C.Epsilon <= C.Epsilon_0)
                        {
                            C.Kappa = 1;
                        }
                        else
                        {//epsilon>epsilon0 の場合　
                            C.Kappa = Configuration.Alpha * Math.Pow(C.Epsilon / C.Epsilon_0, -Configuration.Nyu);
                        }
                        AccuracySum += C.Kappa * C.N;
                        //Accuracy　はNaNなるとき　止める
                        if (double.IsNaN(AccuracySum))
                        {
                            Console.ReadLine();
                        }
                    //}
                }

                else //1000 回以下の場合 
                {
                    if (C.Epsilon <= C.Epsilon_0)
                    {
                        C.Kappa = 1;
                    }
                    else
                    {

                        //ここC.Epsilon_0 == 0, kappa の計算がおかしいから, SNC.epsilon_0 下駄を足す
                        C.Kappa = Configuration.Alpha * Math.Pow(C.Epsilon / SNC.Epsilon_0, -Configuration.Nyu);
                    }
                    if (double.IsNaN(C.Kappa))
                    {
                        Console.WriteLine("kappa =NaN");
                        Console.ReadLine();
                    }
                    AccuracySum += C.Kappa * C.N;
                }
            }
            //
            foreach (Classifier C in this.CList)
            {

                C.F += Configuration.Beta * (C.Kappa * C.N / AccuracySum - C.F);

                if (double.IsNaN(C.F))
                {
                    Console.ReadLine();
                }
            }
            
        }

        // 包摂
        protected override void Subsumption(Population Pop)
        {
            List<Classifier> copyActionSet = new List<Classifier>();
            foreach (Classifier classifier in CList)
            {
                copyActionSet.Add(classifier);
            }
            int N = copyActionSet.Count;

            //最大N回実行する 、Nは変化する，もっと一般化されたものは毎回削除される
            for (int i = 0; i < N; i++)
            {
                #region subsume
                Classifier Subsumber_cl= null;

                //actionsetなかに最も一般的な分類子をClにする、残った分類子の中にもっと一般化な分類子を抽出
                foreach (Classifier C in copyActionSet)
                {
                    if (C.CouldSubsume())
                    {
                        if ((Subsumber_cl== null) || (C.C.NumberOfSharp > Subsumber_cl.C.NumberOfSharp) || ((C.C.NumberOfSharp == Subsumber_cl.C.NumberOfSharp) && (Configuration.MT.NextDouble() < 0.5)))
                        {
                            Subsumber_cl= C;
                        }
                    }
                }
                if (Subsumber_cl != null)
                {
                    // 削除中にforeachできない
                    List<Classifier> CL = new List<Classifier>();
                    // 包摂された、削除したいClassifier C　をCLに登録
                    // まずCopyActionSetは　Subsumber_cl を削除する、しないと自分を削除される
                    copyActionSet.Remove(Subsumber_cl);
                    for (int index = 0; index < copyActionSet.Count; index++)
                    {
                        Classifier C = copyActionSet[index];

                        if (Subsumber_cl.IsMoreGeneral(C))
                        {
                            SigmaNormalClassifier Snc_ko = (SigmaNormalClassifier)C;
                            SigmaNormalClassifier Snc_oya = (SigmaNormalClassifier)Subsumber_cl;

                            // e0 の値を３位まで見る、近いものは差がないとみなす
                            var subsumed = Math.Round(C.Epsilon_0, 3);
                            var subsumer = Math.Round(Subsumber_cl.Epsilon_0, 3);

                            if ((subsumer <= (subsumed + subsumer / 10))
                               && Snc_ko.IsConvergenceEpsilon()
                            && Snc_oya.IsConvergenceEpsilon()
                                )
                            {
                                if (C.C.state[4] == '0' & C.C.state[7] == '1')//"bath0 rehabi1"
                                {
                                    Configuration.Problem.WriteLine(C.C.state + "," + Configuration.T + "," + C.P + "," + C.M + "," + C.Epsilon + "," +
                                        C.F + "," + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.GenerateTime + ", AS subsumed");

                                    Configuration.Problem.WriteLine(Snc_oya.C.state + "," + Configuration.T + "," + Snc_oya.P + "," + Snc_oya.M + "," + Snc_oya.Epsilon + "," +
                                        Snc_oya.F + "," + Snc_oya.N + "," + Snc_oya.Exp + "," + Snc_oya.Ts + "," + Snc_oya.As + "," + Snc_oya.Kappa + "," +
                                        Snc_oya.Epsilon_0 + "," + Snc_oya.St + "," + Snc_oya.GenerateTime + ", AS subsumer");
                                }
                                Subsumber_cl.N += C.N;
                                //包摂された分類子を削除
                                copyActionSet.RemoveAt(index);
                                //AS で削除,pop で削除
                                this.Remove(C);
                                Pop.Remove(C);
                            }
                        }
                    }
                }
                //if (Subsumber_cl!= null)
                //{
                //    // 削除中にforeachできない
                //    List<Classifier> CL = new List<Classifier>();
                //    // 包摂された、削除したいClassifier C　をCLに登録
                //    // まずCopyActionSetは　Subsumber_cl を削除する、しないと自分を削除される
                //    copyActionSet.Remove(Subsumber_cl);
                //    foreach (Classifier C in copyActionSet)
                //    {
                //        if (Subsumber_cl.IsMoreGeneral(C))
                //        {
                //            SigmaNormalClassifier Snc_ko = (SigmaNormalClassifier)C;
                //            SigmaNormalClassifier Snc_oya = (SigmaNormalClassifier)Subsumber_cl;

                //            // e0 の値を３位まで見る、近いものは差がないとみなす
                //            var subsumed = Math.Round(C.Epsilon_0, 3);
                //            var subsumer = Math.Round(Subsumber_cl.Epsilon_0, 3);

                //            if ((subsumer <= (subsumed + subsumer / 10))
                //               && Snc_ko.IsConvergenceEpsilon()
                //            && Snc_oya.IsConvergenceEpsilon()
                //                )
                //            {
                //                if (C.C.state[4] == '0' & C.C.state[7] == '1')//"bath0 rehabi1"
                //                {
                //                    Configuration.Problem.WriteLine(C.C.state + "," + Configuration.T + "," + C.P + "," + C.M + "," + C.Epsilon + "," + 
                //                        C.F + "," + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.GenerateTime + ", AS subsumed");

                //                    Configuration.Problem.WriteLine(Snc_oya.C.state + "," + Configuration.T + "," + Snc_oya.P + "," + Snc_oya.M + "," + Snc_oya.Epsilon + "," +
                //                        Snc_oya.F + "," + Snc_oya.N + "," + Snc_oya.Exp + "," + Snc_oya.Ts + "," + Snc_oya.As + "," + Snc_oya.Kappa + "," +
                //                        Snc_oya.Epsilon_0 + "," + Snc_oya.St + "," + Snc_oya.GenerateTime + ", AS subsumer");
                //                }
                //                Subsumber_cl.N += C.N;
                //                CL.Add(C);
                //            }
                //        }
                //    }

                //    foreach (Classifier C in CL)
                //    {
                //        this.Remove(C);//as から削除
                //        Pop.Remove(C);//pop から削除
                //    }
                //}
                #endregion
            }
        }

        // Actionsetから削除
        public override void Remove(Classifier C)
        {
            this.CList.Remove(C);
        }

        public override void RunGA(State Situation, Population P)
        {
            
            double NumerositySum = 0.0;
            double TimeStampSum = 0.0;

            
            foreach (Classifier C in this.CList)
            {
                NumerositySum += C.N;
                TimeStampSum += C.Ts * C.N;
            }

            if (Configuration.T - TimeStampSum / NumerositySum > Configuration.Theta_GA)
            {
                foreach (Classifier C in this.CList)
                {
                    C.Ts = Configuration.T;
                }

                Classifier Parent_1 = this.SelectOffspring();

                Classifier Parent_2 = this.SelectOffspring();
                //親を選択する、同じ親選ばないようにする
                //どうしても同じ親を選ぶ場合、GAをやめる、return 

                var selectCounter = 0;
                while (CList.Count != 1 && Parent_2.C.state == Parent_1.C.state)
                {
                    Parent_2 = this.SelectOffspring();
                    selectCounter += 1;
                    if (selectCounter > 10)
                    {
                        return;
                    }
                }

                Classifier Child_1 = new SigmaNormalClassifier((SigmaNormalClassifier)Parent_1);
                Classifier Child_2 = new SigmaNormalClassifier((SigmaNormalClassifier)Parent_2);

                ///nakata added
                Child_1.F /= Child_1.N;
                Child_2.F /= Child_2.N;
                //////////////

                Child_1.N = Child_2.N = 1;
                Child_1.Exp = Child_2.Exp = 0;
                Child_1.St = Child_2.St = 0;
                Child_1.M = Child_2.M = 0;
                Child_1.S = Child_2.S = 0;
                //Child_1.Epsilon_0 = Child_2.Epsilon_0 = Configuration.Epsilon_0;

                SigmaNormalClassifier SNP1 = (SigmaNormalClassifier)Parent_1;
                SigmaNormalClassifier SNP2 = (SigmaNormalClassifier)Parent_2;
                SigmaNormalClassifier SNC1 = (SigmaNormalClassifier)Child_1;
                SigmaNormalClassifier SNC2 = (SigmaNormalClassifier)Child_2;

                // 交叉
                if (Configuration.MT.NextDouble() < Configuration.Chai)
                {
                    Child_1.Crossover(Child_2);
                    Child_1.P = (Parent_1.P + Parent_2.P) / 2;
                    //Child_1.Epsilon = Parent_1.Epsilon + Parent_2.Epsilon;
                    Child_1.Epsilon = (Parent_1.Epsilon + Parent_2.Epsilon) / 2;
                    Child_1.F = (Parent_1.F + Parent_2.F) / 2;

                    Child_1.Epsilon_0 = (Parent_1.Epsilon_0 + Parent_2.Epsilon_0) / 2;
                    //Child_1.Epsilon_0 = Math.Min(Parent_1.Epsilon_0, Parent_2.Epsilon_0);
                    Child_2.P = Child_1.P;
                    Child_2.Epsilon = Child_1.Epsilon;
                    Child_2.F = Child_1.F;

                    Child_2.Epsilon_0 = Child_1.Epsilon_0;
                }

                Child_1.F *= 0.1;
                Child_2.F *= 0.1;

                // bothChild
                Child_1.Mutation(Situation);
                Child_2.Mutation(Situation);

                if (Child_1.C.state[4] == '0' & Child_1.C.state[7] == '1')//"bath0 rehabi1"
                {
                    Configuration.Problem.WriteLine(Child_1.C.state + "," + Configuration.T + "," + Child_1.P + "," + Child_1.M + "," + Child_1.Epsilon + "," + Child_1.F + ","
                        + Child_1.N + "," + Child_1.Exp + "," + Child_1.Ts + "," + Child_1.As + "," + Child_1.Kappa + "," + Child_1.Epsilon_0 + "," + Child_1.St + "," + Child_1.GenerateTime + ", child 2 ");
                    Configuration.Problem.WriteLine(Parent_1.C.state + "," + Configuration.T + "," + Parent_1.P + "," + Parent_1.M + "," + Parent_1.Epsilon + "," + Parent_1.F + ","
                        + Parent_1.N + "," + Parent_1.Exp + "," + Parent_1.Ts + "," + Parent_1.As + "," + Parent_1.Kappa + "," + Parent_1.Epsilon_0 + "," + Parent_1.St + "," + Parent_1.GenerateTime + ",Parent1");

                    Configuration.Problem.WriteLine(Parent_2.C.state + "," + Configuration.T + "," + Parent_2.P + "," + Parent_2.M + "," + Parent_2.Epsilon + "," + Parent_2.F + ","
                        + Parent_2.N + "," + Parent_2.Exp + "," + Parent_2.Ts + "," + Parent_2.As + "," + Parent_2.Kappa + "," + Parent_2.Epsilon_0 + "," + Parent_2.St + "," + Parent_2.GenerateTime + ",Parent2");
                }

                if (Child_2.C.state[4] == '0' & Child_2.C.state[7] == '1')//"bath0 rehabi1"
                {
                    Configuration.Problem.WriteLine(Child_2.C.state + "," + Configuration.T + "," + Child_2.P + "," + Child_2.M + "," + Child_2.Epsilon + "," + Child_2.F + ","
                        + Child_2.N + "," + Child_2.Exp + "," + Child_2.Ts + "," + Child_2.As + "," + Child_2.Kappa + "," + Child_2.Epsilon_0 + "," + Child_2.St + "," + Child_2.GenerateTime + ",child 2");
                    Configuration.Problem.WriteLine(Parent_1.C.state + "," + Configuration.T + "," + Parent_1.P + "," + Parent_1.M + "," + Parent_1.Epsilon + "," + Parent_1.F + ","
                        + Parent_1.N + "," + Parent_1.Exp + "," + Parent_1.Ts + "," + Parent_1.As + "," + Parent_1.Kappa + "," + Parent_1.Epsilon_0 + "," + Parent_1.St + "," + Parent_1.GenerateTime + ",Parent1");

                    Configuration.Problem.WriteLine(Parent_2.C.state + "," + Configuration.T + "," + Parent_2.P + "," + Parent_2.M + "," + Parent_2.Epsilon + "," + Parent_2.F + ","
                        + Parent_2.N + "," + Parent_2.Exp + "," + Parent_2.Ts + "," + Parent_2.As + "," + Parent_2.Kappa + "," + Parent_2.Epsilon_0 + "," + Parent_2.St + "," + Parent_2.GenerateTime + ",Parent2");
                }


                
                if (Configuration.DoGASubsumption)
                {
                    if (Parent_1.DoesSubsume(Child_1))
                    {
                        Parent_1.N++;
                    }
                    else if (Parent_2.DoesSubsume(Child_1))
                    {
                        Parent_2.N++;
                    }
                    else
                    {
                        P.Insert(Child_1);
                    }
                    P.Delete();

                    if (Parent_1.DoesSubsume(Child_2))
                    {
                        Parent_1.N++;
                    }
                    else if (Parent_2.DoesSubsume(Child_2))
                    {
                        Parent_2.N++;
                    }
                    else
                    {
                        P.Insert(Child_2);
                    }
                    P.Delete();
                }
                else
                {
                    P.Insert(Child_1);
                    P.Delete();
                    P.Insert(Child_2);
                    P.Delete();
                }

            }
        }

        protected override Classifier SelectOffspring()
        {
            double KappaSum = 0;
            //double FitnessSum = 0;
            // Fitness Sum を計算する
            foreach (Classifier C in this.CList)
            {
                //FitnessSum += C.F;
                if (!Double.IsNaN(C.Kappa) && (C.Exp > Configuration.ExpThreshold))
                {
                    //e0が小さいものが優位,追加したもの
                    //KappaSum += C.Kappa * Math.Pow((1 - C.Epsilon_0 / Configuration.Rho), 5);
                    KappaSum += 1 / C.Kappa; //0115
                }
            }

            double ChoicePoint = Configuration.MT.NextDouble() * KappaSum;
            //double ChoicePoint = Configuration.MT.NextDouble() * FitnessSum;
            KappaSum = 0;
            // ルーレット選択する
            foreach (Classifier C in this.CList)
            {
                //if( !Double.IsNaN( C.F ) && ( C.Exp > Configuration.ExpThreshold ) )
                if (!Double.IsNaN(C.Kappa) && (C.Exp > Configuration.ExpThreshold))
                {
                    //KappaSum += C.Kappa * Math.Pow((1 - C.Epsilon_0 / Configuration.Rho), 5);//e0が小さいものが優位,追加したもの
                    KappaSum += 1 / C.Kappa;
                }

                //if (!(Double.IsNaN(C.Kappa)))
                //{
                //    FitnessSum += C.F;
                //}
                if (KappaSum > ChoicePoint)
                {
                    return C;
                }
            }
            // FitnessにNaNが入っているとき、すべて0の時
            return this.CList[Configuration.MT.Next(this.CList.Count - 1)];
        }
    }
}