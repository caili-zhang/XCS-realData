﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	class SigmaNormalPopulation : Population
	{
		public SigmaNormalPopulation( int Number )
		{
			this.Number = Number;
			this.CList = new List<Classifier>();
		}

		// ListにClassifier追加
		public override void Add( Classifier C )
		{
			// 最大サイズになったとき
			if( this.Number == this.CList.Count )
			{
				return; // 削除実装予定
			}
			this.CList.Add( C );
		}

		// ListからClassifier削除
		public override void Remove( Classifier C )
		{
			//Console.WriteLine( Configuration.T + " del " + C.C.state + " : " + C.A + " : " + C.Epsilon + " : " + C.Epsilon_0 );
			//if( Configuration.T > 41000 )
			//	System.Threading.Thread.Sleep( 500 );
			this.CList.Remove( C );
		}

		// すべてのClassifier表示(確認用)
		public override void Show()
		{
			StreamWriter sw = new StreamWriter( "./Population_" + Configuration.T + "_" + Configuration.Seed + "CnoiseWidth" + Configuration.NoiseWidth
				+ "AS_" + Configuration.ASName + "ET_" + Configuration.ExpThreshold + "DS_" + Configuration.DifferenceSigma + "LS_" + Configuration.LookBackSigma
				+ "DE_" + Configuration.DifferenceEpsilon + "LE_" + Configuration.LookBackEpsilon + ".csv", true, System.Text.Encoding.GetEncoding( "shift_jis" ) );
			//StreamWriter sw = new StreamWriter( "./Population_" + Configuration.T + "_" + Configuration.Seed + "CnoiseWidth" + Configuration.NoiseWidth
			//	+ "AS_" + "CS" + "ET_" + Configuration.ExpThreshold + "DS_" + Configuration.DifferenceSigma + "LS_" + Configuration.LookBackSigma
			//	+ "DE_" + Configuration.DifferenceEpsilon + "LE_" + Configuration.LookBackEpsilon + ".csv", true, System.Text.Encoding.GetEncoding( "shift_jis" ) );
			sw.WriteLine( "state,action,prediction,epsilon,fitness,numerosity,experience,timestamp,actionsetsize,accuracy,epsilon_0,selectTime,mean,std,generateTime,generality,convergence,convergencetime" );
			foreach( SigmaNormalClassifier C in this.CList )
			{
				//Console.WriteLine( "state: " + C.C.state + " action: " + C.A + " Prediction: " + C.P + " Epsilon: " + C.Epsilon + " Fitness" + C.F + " Numerosity: " + C.N + " Experience: " + C.Exp + " TimeStamp: " + C.Ts + " ASsize: " + C.As + " Accuracy: " + C.Kappa + "Epsilon_0: " + C.Epsilon_0 );
				//Console.WriteLine();

				sw.WriteLine( C.C.state + "," /*+ C.A + ","*/ + C.P + "," + C.Epsilon + "," + C.F + "," + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.M + "," + Math.Sqrt( C.S / ( C.St - 1 ) ) + "," + C.GenerateTime + "," + C.C.Generality + "," + ( C.IsConvergenceEpsilon() ? 1 : 0 ) + "," + C.ConvergenceTime );
			}
			sw.Close();
		}

		// Situationに合ったClassifierをMatchSetに渡す
		public override List<Classifier> MatchSituation( State S )
		{
			List<Classifier> MatchSet = new List<Classifier>();

			foreach( Classifier C in this.CList )
			{
				if( S.Match( C.C ) )
				{
					MatchSet.Add( C );
				}
			}

			return MatchSet;
		}

		// Populationからfitnessが一番小さいものを削除
		public override void Delete()
		{
            int SumNumerosity = 0;
			double SumFitness = 0.0;

			foreach( Classifier C in this.CList )
			{
				SigmaNormalClassifier SNC = ( SigmaNormalClassifier )C;
				SumNumerosity += SNC.N;
				//SumFitness += C.F;
				SumFitness += SNC.WinningRate.Average();
				//Console.WriteLine(SNC.C.state + " ave: " + SNC.WinningRate.Average());
			}
            if (SumNumerosity > Configuration.N)
            {
                Console.ReadLine();
            }
			if( SumNumerosity <= Configuration.N )
			{
				return;
			}

			double AvFitness = SumFitness / SumNumerosity;
			double VoteSum = 0;

			foreach( Classifier C in this.CList )
			{
				VoteSum += this.DeletionVote( C, AvFitness );
			}

			double ChoicePoint = Configuration.MT.NextDouble() * VoteSum;
			VoteSum = 0;
            
			foreach( Classifier C in this.CList )
			{
				VoteSum += this.DeletionVote( C, AvFitness );

				if( VoteSum > ChoicePoint )
				{
					if( C.N > 1 )
					{
						C.N--;
					}
					else
                    {
                        this.Remove( C );
					}
					return;
				}
			}
		}

		protected override double DeletionVote( Classifier C, double AvFitness )
		{
			// SigmaNormalClassifier前提
			SigmaNormalClassifier SNC = ( SigmaNormalClassifier )C;

			double Vote = SNC.As * SNC.N;
			//Console.WriteLine("AS: " + SNC.As + " N: " + SNC.N);

			if( ( SNC.Exp > Configuration.Theta_del ) && ( SNC.WinningRate.Average() / SNC.N < Configuration.Delta * AvFitness ) )
			{
				if(SNC.WinningRate.Average() == 0)
				{
					//Console.WriteLine("0 : " + Vote);
					return Vote;
				}
				Vote *= AvFitness / ( SNC.WinningRate.Average() / SNC.N );
				//Console.WriteLine("hoge");
			}

			//Console.WriteLine(SNC.WinningRate.Average() + " : " + Vote);
			return Vote;
		}

		public override void Insert( Classifier Cl )
		{
			foreach( Classifier C in this.CList )
			{
				if( C.Equals( Cl ) )
				{
					C.N++;
					return;
				}
			}
			this.CList.Add( Cl );
		}

		public override int CountNumerosity()
		{
			int CN = 0;
			foreach( Classifier C in this.CList )
			{
				CN += C.N;
			}

			return CN;
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
                copyPopSet.Add(classifier);
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
                    for(int index = 0; index < copyPopSet.Count; i++)
                    {
                        Classifier C = copyPopSet[index];

                        if (Subsumber_cl.IsMoreGeneral(C))
                        {
                            SigmaNormalClassifier Snc_ko = (SigmaNormalClassifier)C;
                            SigmaNormalClassifier Snc_oya = (SigmaNormalClassifier)Subsumber_cl;

                            // e0 の値を３位まで見る、近いものは差がないとみなす
                            //var subsumed = Math.Round(C.Epsilon_0, 3);
                            //var subsumer = Math.Round(Subsumber_cl.Epsilon_0, 3);

                            //if ((subsumer <= (subsumed + subsumer / 10))
                            //   && Snc_ko.IsConvergenceEpsilon()
                            //&& Snc_oya.IsConvergenceEpsilon()
                            //    )
                            //{
                            
                            Subsumber_cl.N += C.N;
                            //包摂された分類子を削除
                            copyPopSet.RemoveAt(index);
                            this.Remove(C);
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
        /// <summary>
        /// 総当たり評価
        /// </summary>
        public void RoundRobin()
		{
			//double[,] Results = new double[this.CList.Count, this.CList.Count];

			//// 総当たり比較
			//for( int i = 0; i < this.CList.Count - 1; i++ )	// 比較元
			//{
			//	for( int j = i + 1; j < this.CList.Count; j++ )
			//	{
			//		//if(this.CList[i].Kappa > this.CList[j].Kappa)
			//		if( this.CList[i].F > this.CList[j].F )
			//		{
			//			Results[i, j] = 1;	// 勝ち点1
			//		}
			//		//else if(this.CList[i].Kappa == this.CList[j].Kappa)
			//		else if( this.CList[i].F == this.CList[j].F )
			//		{
			//			Results[i, j] = 0.5;	// 引き分け点0.5
			//		}
			//		else
			//		{
			//			Results[i, j] = 0;
			//		}
			//	}
			//}

			// 勝率計算
			for( int i = 0; i < this.CList.Count; i++ )
			{
				//double WinningPoint = 0;
				//for( int j = 0; j < i; j++ )
				//{
				//	WinningPoint += 1 - Results[j, i];
				//}
				//for( int j = i + 1; j < this.CList.Count; j++ )
				//{
				//	WinningPoint += Results[i, j];
				//}

				SigmaNormalClassifier SNC = ( SigmaNormalClassifier )this.CList[i];
				// 移動
				for( int index = SNC.WinningRate.Count() - 1; index > 0; index-- )
				{
					SNC.WinningRate[index] = SNC.WinningRate[index - 1];
				}

				//SNC.WinningRate[0] = WinningPoint / ( this.CList.Count - 1 );
				SNC.WinningRate[0] = SNC.F;
			}
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
                            if (/*Ma
                                
                                th.Abs(Cl.M - C.M) < 10 &&*/
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