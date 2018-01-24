using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
    class NormalPopulation : Population
    {
        public NormalPopulation(int Number)
        {
            this.Number = Number;
            this.CList = new List<Classifier>();
        }

        // ListにClassifier追加
        public override void Add(Classifier C)
        {
            // 最大サイズになったとき
            if (this.Number == this.CList.Count)
            {
                
                return; // 削除実装予定
            }
            this.CList.Add(C);
        }

        // ListからClassifier削除
        public override void Remove(Classifier C)
        {
            
            this.CList.Remove(C);
        }

        // すべてのClassifier表示(確認用)
        public override void Show()
        {
            StreamWriter sw = new StreamWriter("./Population_" + Configuration.T + ".csv", true, System.Text.Encoding.GetEncoding("shift_jis"));
          
            if (Configuration.ASName != "CS" && Configuration.ASName != "MaxCS" && Configuration.ASName != "Max" && Configuration.ASName != "Updatee0CS")
            {
                sw.WriteLine("state,prediction,epsilon,average reward,fitness,numerosity,experience,timestamp,actionsetsize,accuracy,epsilon_0,selectTime,mean,std,generateTime,generality");
                foreach (Classifier C in this.CList)
                {
                    //Console.WriteLine( "state: " + C.C.state + " action: " + C.A + " Prediction: " + C.P + " Epsilon: " + C.Epsilon + " Fitness" + C.F + " Numerosity: " + C.N + " Experience: " + C.Exp + " TimeStamp: " + C.Ts + " ASsize: " + C.As + " Accuracy: " + C.Kappa + "Epsilon_0: " + C.Epsilon_0 );
                    //Console.WriteLine();

                    sw.WriteLine(C.C.state + "," + C.P + "," + C.M + "," + C.Epsilon + "," + C.F + "," + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.M + "," + Math.Sqrt(C.S / (C.St - 1)) + "," + C.GenerateTime + "," + C.C.Generality);
                }
            }
            else
            {
                sw.WriteLine("state,起床,就寝,お茶,園芸,入浴,おやつ,新聞,リハビリ," +
                    "prediction,average reward,epsilon,fitness,numerosity,experience," +
                    "timestamp,actionsetsize,accuracy,epsilon_0,selectTime,mean,std,generateTime,generality" +
                    "convergen");
                foreach (Classifier Cl in this.CList)
                {
                    SigmaNormalClassifier C = (SigmaNormalClassifier)Cl;
                    string classifierState = "";
                    for (int i = 0; i < C.C.state.Length; i++)
                    {
                        if (i % 4 == 0)
                        {
                            classifierState = classifierState + "," + C.C.state[i];
                        }
                        else
                        {
                            classifierState += C.C.state[i];
                        }
                    }
                    sw.WriteLine(classifierState  + "," + C.P + "," + C.M + "," + C.Epsilon + "," + C.F +
                        "," + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + 
                        "," + C.St + "," + C.M + "," + Math.Sqrt(C.S / (C.St - 1)) + "," + C.GenerateTime + "," + C.C.Generality
                        + "," + (C.IsConvergenceEpsilon() ? 1 : 0));
                }
            }
            sw.Close();
        }

        // Situationに合ったClassifierをMatchSetに渡す
        public override List<Classifier> MatchSituation(State S)
        {
            List<Classifier> MatchSet = new List<Classifier>();

            foreach (Classifier C in this.CList)
            {
                if (S.Match(C.C))
                {
                    MatchSet.Add(C);
                }
            }

            return MatchSet;
        }

        // Populationからfitnessが一番小さいものを削除

        public override void Delete()
        {
            //double FitnessSum = 0;
            //// Fitness Sum を計算する
            //foreach (Classifier C in this.CList)
            //{
            //    //FitnessSum += C.F;
            //    if (!Double.IsNaN(C.Kappa) && (C.Exp > Configuration.ExpThreshold))
            //    {
            //        //e0が小さいものが優位,追加したもの
            //        FitnessSum += C.Kappa * Math.Pow((1 - C.Epsilon_0 / Configuration.Rho), 5);
            //    }
            //}

            //double ChoicePoint = Configuration.MT.NextDouble() * FitnessSum;
            //FitnessSum = 0;
            //// ルーレット選択する
            //foreach (Classifier C in this.CList)
            //{
            //    //if( !Double.IsNaN( C.F ) && ( C.Exp > Configuration.ExpThreshold ) )
            //    if (!Double.IsNaN(C.Kappa) && (C.Exp > Configuration.ExpThreshold))
            //    {
            //        FitnessSum += C.Kappa * Math.Pow((1 - C.Epsilon_0 / Configuration.Rho), 5);//e0が小さいものが優位,追加したもの
            //    }

            //    //if (!(Double.IsNaN(C.Kappa)))
            //    //{
            //    //    FitnessSum += C.F;
            //    //}
            //    if (FitnessSum > ChoicePoint)
            //    {
            //        return C;
            //    }
            //}
            ////////////////////////////////////////////
            int SumNumerosity = 0;
            double SumFitness = 0.0;

            foreach (Classifier C in this.CList)
            {
                SumNumerosity += C.N;
                SumFitness += C.F;
            }
            if (SumNumerosity <= Configuration.N)
            {
                return;
            }

            double AvFitness = SumFitness / SumNumerosity;
            double VoteSum = 0;

            foreach (Classifier C in this.CList)
            {
                VoteSum += this.DeletionVote(C, AvFitness);
            }

            double ChoicePoint = Configuration.MT.NextDouble() * VoteSum;//ルーレット選択

            VoteSum = 0;

            foreach (Classifier C in this.CList)
            {
                VoteSum += this.DeletionVote(C, AvFitness);

                if (VoteSum > ChoicePoint)
                {
                    if (C.N > 1)
                    {
                        if (C.C.state.Substring(16, 4).Equals("0***") & C.C.state.Substring(28, 4).Equals("*0**"))//"bath0 rehabi1"
                        {
                            Configuration.Problem.WriteLine(C.C.state + "," + Configuration.T + "," + C.P + "," + C.M + ","
                                + C.Epsilon + "," + C.F + "," + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.GenerateTime + ", in pop -1");
                        }

                        C.N--;
                    }
                    else
                    {

                        if (C.C.state.Substring(16, 4).Equals("0***") & C.C.state.Substring(28, 4).Equals("*0**"))//"bath0 rehabi1"
                        {
                            Configuration.Problem.WriteLine(C.C.state + "," + Configuration.T + "," + C.P + "," + C.M + ","
                                + C.Epsilon + "," + C.F + "," + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.GenerateTime + ", in pop delete");
                        }
                        this.Remove(C);
                    }
                    return;
                }
            }
        }

        /// <summary>
        ///  POP subsumption
        /// </summary>
        /// <param name="pop"></param>
        public override void Subsumption()
        {
            List<Classifier> copyPopSet = new List<Classifier>();
            foreach (Classifier classifier in CList)
            {   
                //正確かつ収束した分類子に対して行う
                SigmaNormalClassifier s_classifier = (SigmaNormalClassifier)classifier;
                if (s_classifier.Kappa==1 & s_classifier.IsConvergenceEpsilon())
                {
                    copyPopSet.Add(classifier);
                }
                
            }
            int N = copyPopSet.Count;

            //最大N回実行する 、Nは変化する，もっと一般化されたものは毎回削除される
            for (int i = 0; i < N; i++)
            {
                #region subsume
                Classifier Subsumber_cl = null;

                //actionsetなかに最も一般的な分類子をClにする、残った分類子の中にもっと一般化な分類子を抽出
                foreach (Classifier C in copyPopSet)
                {
                    if (C.CouldSubsume())
                    {
                        if ((Subsumber_cl == null) || (C.C.NumberOfSharp > Subsumber_cl.C.NumberOfSharp) || ((C.C.NumberOfSharp == Subsumber_cl.C.NumberOfSharp) && (Configuration.MT.NextDouble() < 0.5)))
                        {
                            
                            Subsumber_cl = C;
                        }
                    }
                }

                // subsumer が包摂できる分類子を包摂する
                if (Subsumber_cl != null)
                {
                    // 削除中にforeachできない
                    List<Classifier> CL = new List<Classifier>();
                    // 包摂された、削除したいClassifier C　をCLに登録
                    // まずCopyActionSetは　Subsumber_cl を削除する、しないと自分を削除される
                    copyPopSet.Remove(Subsumber_cl);
                    for (int index = 0; index < copyPopSet.Count; index++)
                    {
                        Classifier C = copyPopSet[index];

                        if (Subsumber_cl.IsMoreGeneral(C))
                        {
                            SigmaNormalClassifier Snc_ko = (SigmaNormalClassifier)C;
                            SigmaNormalClassifier Snc_oya = (SigmaNormalClassifier)Subsumber_cl;

                            // e0 の値を３位まで見る、近いものは差がないとみなす
                            var subsumed = Math.Round(C.Epsilon_0, 3);
                            var subsumer = Math.Round(Subsumber_cl.Epsilon_0, 3);

                            if (( (Snc_oya.M+Snc_oya.Epsilon < Configuration.RewardAverage) 
                                | (Snc_oya.M - Snc_oya.Epsilon > Configuration.RewardAverage))
                               && Snc_ko.IsConvergenceEpsilon()
                            && Snc_oya.IsConvergenceEpsilon()
                                )
                            {
                                if (C.C.state.Substring(16, 4).Equals("0***") & C.C.state.Substring(28, 4).Equals("*0**"))//"bath0 rehabi1"
                                {
                                    Configuration.Problem.WriteLine(C.C.state + "," + Configuration.T + "," + C.P + "," + C.M + "," + C.Epsilon + "," +
                                        C.F + "," + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.GenerateTime + ", Pop subsumed");

                                    Configuration.Problem.WriteLine(Snc_oya.C.state + "," + Configuration.T + "," + Snc_oya.P + "," + Snc_oya.M + "," + Snc_oya.Epsilon + "," +
                                        Snc_oya.F + "," + Snc_oya.N + "," + Snc_oya.Exp + "," + Snc_oya.Ts + "," + Snc_oya.As + "," + Snc_oya.Kappa + "," +
                                        Snc_oya.Epsilon_0 + "," + Snc_oya.St + "," + Snc_oya.GenerateTime + ", Pop subsumer");
                                }
                                Subsumber_cl.N += C.N;
                                //包摂された分類子を削除
                                copyPopSet.RemoveAt(index);
                                this.Remove(C);
                            }
                        }
                    }
                    //foreach (Classifier C in copyPopSet)
                    //{
                    //    Classifier to_delete = null;
                    //    if (Subsumber_cl.IsMoreGeneral(C))
                    //    {
                    //        SigmaNormalClassifier Snc_ko = (SigmaNormalClassifier)C;
                    //        SigmaNormalClassifier Snc_oya = (SigmaNormalClassifier)Subsumber_cl;

                    //        // e0 の値を３位まで見る、近いものは差がないとみなす
                    //        //var subsumed = Math.Round(C.Epsilon_0, 3);
                    //        //var subsumer = Math.Round(Subsumber_cl.Epsilon_0, 3);

                    //        //if ((subsumer <= (subsumed + subsumer / 10))
                    //        //   && Snc_ko.IsConvergenceEpsilon()
                    //        //&& Snc_oya.IsConvergenceEpsilon()
                    //        //    )
                    //        //{
                    //            if (C.C.state[4] == '0' & C.C.state[7] == '1')//"bath0 rehabi1"
                    //            {
                    //                Configuration.Problem.WriteLine(C.C.state + "," + Configuration.T + "," + C.P + "," + C.M + "," + C.Epsilon + "," +
                    //                    C.F + "," + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.GenerateTime + ", AS subsumed");

                    //                Configuration.Problem.WriteLine(Snc_oya.C.state + "," + Configuration.T + "," + Snc_oya.P + "," + Snc_oya.M + "," + Snc_oya.Epsilon + "," +
                    //                    Snc_oya.F + "," + Snc_oya.N + "," + Snc_oya.Exp + "," + Snc_oya.Ts + "," + Snc_oya.As + "," + Snc_oya.Kappa + "," +
                    //                    Snc_oya.Epsilon_0 + "," + Snc_oya.St + "," + Snc_oya.GenerateTime + ", AS subsumer");
                    //            }
                    //            Subsumber_cl.N += C.N;
                    //            to_delete = C;
                    //            break;

                    //            //CL.Add(C);
                    //        //}
                    //    }
                    //}

                    //foreach (Classifier C in CL)
                    //{
                    //    this.Remove(C);
                    //    //pop から削除
                    //}

                    //いまの最も一般化されたものを削除する
                    //copyPopSet.Remove(Subsumber_cl);

                }
                #endregion
            }
        }
        protected double SumEpsilon_0()
        {//2015 10 8 chou 
            double sum = 0;
            foreach (Classifier C in this.CList)
            {
                sum += C.Epsilon_0;
            }
            return sum;
        }
        protected double MaxEpsilon_0()
        {
            double max = this.CList[0].Epsilon_0;

            foreach (Classifier C in this.CList)
            {
                if (max < C.Epsilon_0)
                    max = C.Epsilon_0;
            }
            return max;
        }
        protected override double DeletionVote(Classifier C, double AvFitness)
        {
            double Vote = C.As * C.N;

            if ((C.Exp > Configuration.Theta_del) && (C.F / C.N < Configuration.Delta * AvFitness))
            {
                Vote *= AvFitness / (C.F / C.N);
            }

            return Vote;
        }

        public override void Insert(Classifier Cl)
        {
            foreach (Classifier C in this.CList)
            {
                if (C.Equals(Cl))
                {
                    C.N++;
                    return;
                }
            }
            this.CList.Add(Cl);
        }

        public override int CountNumerosity()
        {
            int CN = 0;
            foreach (Classifier C in this.CList)
            {
                CN += C.N;
            }

            return CN;
        }



        //学習終了後に圧縮にする
        public override void Compact()
        {
            List<Classifier> copyActionSet = new List<Classifier>();

            foreach (Classifier classifier in CList)
            {
                copyActionSet.Add(classifier);
            }

            int N = copyActionSet.Count;

            for (int i = 0; i < N; i++)
            {
                #region subsume
                Classifier Cl = null;

                //なかに最も一般的な分類子をClにする
                foreach (Classifier C in copyActionSet)
                {
                    if (C.CouldSubsume())
                    {
                        if ((Cl == null) || (C.C.NumberOfSharp > Cl.C.NumberOfSharp) || ((C.C.NumberOfSharp == Cl.C.NumberOfSharp) && (Configuration.MT.NextDouble() < 0.5)))
                        {
                            Cl = C;
                        }
                    }
                }

                //Cl が包摂できる分類子を吸収する
                if (Cl != null)
                {
                    // 削除中にforeachできない
                    List<Classifier> CL = new List<Classifier>();

                    foreach (Classifier C in copyActionSet)
                    {
                        if (Cl.IsMoreGeneral(C))
                        {
                            SigmaNormalClassifier Snc_ko = (SigmaNormalClassifier)C;
                            SigmaNormalClassifier Snc_oya = (SigmaNormalClassifier)Cl;
                            //10の数字は適当　可変にするか？ 1/10にする　10－7 あんまり変わらない
                            //分散が同じの判断基準　他に？？t test を使う　予定  特殊化がでない
                            //
                            //double t = (Snc_oya.M - Snc_ko.M)/Math.Sqrt(Snc_oya.S/30 + Snc_ko.S/30);

                            //double studentT = StudentT.InvCDF(0, 1, 60, 0.005);

                            //if (t < studentT || t > Math.Abs(studentT))//t test で有意差がある 包摂しない
                            //{

                            //}
                            // else//有意差がない　統合しでもよい ただ　特殊化ルールどう識別するか
                            //{
                            if (/*Math.Abs(Cl.M - C.M) < 10 &&*/
                                // Math.Abs(Cl.Epsilon_0 - C.Epsilon_0)< 10&&
                                // Cl.Kappa == 1 
                                //&& Snc_oya.Epsilon < Snc_ko.Epsilon
                              (Cl.Epsilon_0 < C.Epsilon_0 || Math.Abs(Cl.Epsilon_0 - C.Epsilon_0) < Cl.Epsilon_0 / 10)
                               //&& Math.Abs(Cl.Epsilon_0 - C.Epsilon_0) < 10//case 1 +- 10
                               // Cl.Epsilon < C.Epsilon
                               && Snc_ko.IsConvergenceEpsilon()
                            && Snc_oya.IsConvergenceEpsilon()

                                ) //  
                            {
                                Cl.N += C.N;
                                // 包摂された、削除したいClassifier C　をCLに登録
                                CL.Add(C);
                            }
                            // }

                        }
                    }

                    foreach (Classifier C in CL)
                    {
                        this.Remove(C);//包摂された分類子　C　を　pop からCを削除

                    }
                }

                //最も一般化されるもの　copyActionSetからを削除　
                copyActionSet.Remove(Cl);

                #endregion
            }
            
        }


    }
}