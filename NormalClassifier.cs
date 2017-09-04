using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	class NormalClassifier : Classifier
	{
		// Covering
		public NormalClassifier( State S/*, List<char> Actions*/ )
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
		}

		
		// コピーコンストラクタ
		public NormalClassifier( NormalClassifier C )
		{
			this.C = new BinaryState( C.C );
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
		}

	    public NormalClassifier()
	    {

	    }

	    public override bool CouldSubsume()
		{
			if( this.Exp > Configuration.Theta_sub )
			{
				if( this.Epsilon < this.Epsilon_0 )
				{
					return true;
				}
			}
			return false;
		}

		// 包摂条件判定用
		public override bool IsMoreGeneral( Classifier Spec )
		{
			if( this.C.NumberOfSharp <= Spec.C.NumberOfSharp )
			{
				return false;
			}

			int i = 0;

			do
			{
				if( this.C.state[i] != '0' && this.C.state[i] != Spec.C.state[i] )
				{
					return false;
				}
				i++;
			} while( i < this.C.state.Length );

			return true;
		}
        //二点交差
		public override void Crossover( Classifier C )
		{
			double x = Configuration.MT.NextDouble() * ( this.C.Length + 1 );
			double y = Configuration.MT.NextDouble() * ( this.C.Length + 1 );

			if( x > y )
			{
				double tmp = x;
				x = y;
				y = tmp;
			}

			int i = 0;
			do
			{
				if( x <= i && i < y )
				{
					this.C.Switch( C.C, i );
				}
				i++;
			} while( i < y );
		}

		public override void Mutation( State S )
		{
			int i = 0;

			string state = "";
			do
			{
				if( Configuration.MT.NextDouble() < Configuration.Myu )
				{
					// #とstateの切り替え
					if( this.C.state[i] == '0' )
					{
						state += S.state[i];
					}
					else
					{
						state += '*';
					}
				}
				else
				{
					state += this.C.state[i];
				}
				i++;
			} while( i < this.C.state.Length );
			this.C.state = state;
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
						return true;
					}
				}
			//}
			return false;
		}
	}
}