﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	class SigmaNormalClassifier : Classifier
	{
		// Experience閾値
		int ExpThreshold;
		// epsilonストック
		public double[] EpsilonList;
        //reward 記録
	    public double[] RewardList;
		// 勝率記録
		public double[] WinningRate;
		// 収束フラグ
		private bool ConvergenceFlag = false;
		public int ConvergenceTime = -1;

		// Covering
		public SigmaNormalClassifier( State S,/* List<char> Actions,*/ int ExpThreshold )
		{
			// Covering済みConditionセット
			this.C = S;
			// MatchSetにない行動をランダムに選ぶ
			//this.A = Actions[Configuration.MT.Next( Actions.Count - 1 )];

			this.P = Configuration.P_I;
			this.Epsilon = Configuration.Epsilon_I;
			this.F = Configuration.F_I;
			this.Exp = 0;
			this.Ts = Configuration.T;
			this.As = 1;
			this.N = 1;
			this.Epsilon_0 = Configuration.Epsilon_0;
			this.St = 0;
			this.M = 0;
			this.S = 0;
			this.GenerateTime = Configuration.T;
			this.ExpThreshold = ExpThreshold;
			this.EpsilonList = new double[Configuration.LookBackEpsilon + 1];
			this.WinningRate = new double[Configuration.LookBackEpsilon];
			// はじめは消されにくいように
			for(int i = 0; i < this.WinningRate.Count(); i++)
			{
				this.WinningRate[i] = Configuration.F_I;
			}
			this.ConvergenceFlag = false;
		}

		

		// コピーコンストラクタ
		public SigmaNormalClassifier( SigmaNormalClassifier C )
		{
			this.C = new IntegralState( C.C );
			//this.A = C.A;
			this.P = C.P;
			this.Epsilon = C.Epsilon;
			this.F = C.F;
			this.N = C.N;
			this.Exp = C.Exp;
			this.Ts = C.Ts;
			this.As = C.As;
			this.Kappa = C.Kappa;
            
           
            this.Epsilon_0 = C.Epsilon_0;
            
			
			this.St = C.St;
			this.M = C.M;
			this.S = C.S;
			this.GenerateTime = Configuration.T;
			this.ExpThreshold = C.ExpThreshold;
			this.EpsilonList = new double[Configuration.LookBackEpsilon + 1];
			this.WinningRate = new double[Configuration.LookBackEpsilon];
			// はじめは消されにくいように
			for( int i = 0; i < this.WinningRate.Count(); i++ )
			{
				this.WinningRate[i] = C.WinningRate[i];
			}
			this.ConvergenceFlag = false;
		}

		public override bool CouldSubsume()
		{
            if (this.Exp > this.ExpThreshold)
            {
                if (this.IsConvergenceEpsilon())
                {
                    //Configuration.FlagEpsilon = true;
                    // original 条件は e < e0
                    if ( this.Epsilon < this.Epsilon_0||(this.Epsilon==0 &&this.Epsilon_0==0) )
                    {
						return true;
					}
                }
            }
            return false;
		}

        // 包摂条件判定用
        public override bool IsMoreGeneral(Classifier Spec)
        {
            if (this.C.NumberOfSharp <= Spec.C.NumberOfSharp)
            {
                return false;
            }

            int i = 0;

            do
            {
                if (this.C.state[i] != '0' && this.C.state[i] != Spec.C.state[i])
                {
                    return false;
                }
                i++;
            } while (i < this.C.state.Length);

            return true;
        }

        //交叉、属性ごとで交叉を行う、４ビットを１つ単位で交叉する、点交叉
        public override void Crossover(Classifier C)
        {
            double x = Math.Floor(Configuration.MT.NextDouble() * (this.C.Length / 4 + 1));
            int i = 0;
            do
            {
                if (i >= x * 4)
                {
                    this.C.Switch(C.C, i);
                }
                i++;
            } while (i < x * 4);
        }

        public override void Mutation(State S)
        {
            int i = 0;

            string state = "";
            do
            {
                if (Configuration.MT.NextDouble() < Configuration.Myu)
                {
                    // #とstateの切り替え
                    if (this.C.state[i] == '0')
                    {
                        state += '*';
                    }
                    else
                    {
                        state += '0';
                    }
                }
                else
                {
                    state += this.C.state[i];
                }
                i++;
            } while (i < this.C.state.Length);
            this.C.state = state;
            if (state== "00000000000000000000000000000000")
            {
                this.Mutation(S);
            }
            
            this.C.CountSharp();
        }

        // 包摂条件判定
        public override bool DoesSubsume( Classifier C )
		{
			/*if( this.A == C.A )
			{*/
				if( this.CouldSubsume() )
				{
					if( this.IsMoreGeneral( C ) )
					{
                        if (this.Epsilon_0 <= C.Epsilon_0)//chou 10-5 GA subsumption 
                        {
                            return true;
                        }
					}
				}
			//}
			return false;
		}

		// 誤差ε用分散収束
		public bool IsConvergenceEpsilon()
		{
			if( this.ConvergenceFlag )
			{
				return true;
			}

			if( this.Exp < Configuration.LookBackEpsilon )
			{
				return false;
			}

			double Sum = 0;

			for( int i = 1; i < this.EpsilonList.Count(); i++ )
			{
				Sum += Math.Abs( this.EpsilonList[i] - this.EpsilonList[0] );
			}

			if(Sum < Configuration.DifferenceEpsilon)
			{
				this.ConvergenceFlag = true;
				if( this.ConvergenceTime == -1 )
				{
					this.ConvergenceTime = Configuration.T;
				}
			}

			return Sum < Configuration.DifferenceEpsilon;
		}

		public bool HasMatured()
		{
			if(this.Exp < Configuration.MatureTime)
			{
				return false;
			}
			return true;
			
		}
	}
}