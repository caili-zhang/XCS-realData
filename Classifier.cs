using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	abstract class Classifier
	{
		/// <summary>
		/// Condition
		/// </summary>
		public State C { protected set; get; }
		/// <summary>
		/// Action
		/// </summary>
		public char A { protected set; get; }
		/// <summary>
		/// Prediction
		/// </summary>
		public double P { set; get; }
		/// <summary>
		/// prediction error ε
		/// </summary>
		public double Epsilon { set; get; }
		/// <summary>
		/// Fitness
		/// </summary>
		public double F { set; get; }
		/// <summary>
		/// numerosity
		/// </summary>
		public int N { set; get; }
		/// <summary>
		/// experience
		/// </summary>
		public double Exp { set; get; }
		/// <summary>
		/// time stamp
		/// </summary>
		public int Ts { set; get; }
		/// <summary>
		/// action set size
		/// </summary>
		public double As { set; get; }
		/// <summary>
		/// accuracy
		/// </summary>
		public double Kappa { set; get; }
		/// <summary>
		/// Epsilon_0
		/// </summary>
		public double Epsilon_0 { set; get; }

		/// <summary>
		/// 選択回数
		/// </summary>
		public int St { set; get; }

		/// <summary>
		/// 報酬値の平均
		/// </summary>
		public double M { set; get; }

		/// <summary>
		/// 報酬値の標準偏差
		/// </summary>
		public double S { set; get; }

		/// <summary>
		/// 生成時間
		/// </summary>
		public int GenerateTime { set; get; }

		/// <summary>
		/// 包摂の前提を満たすか
		/// </summary>
		/// <returns>満たす(true)</returns>
		public abstract bool CouldSubsume();

		/// <summary>
		/// 一般的か
		/// </summary>
		/// <param name="Spec">比較対象</param>
		/// <returns>比較対象が含まれる(true)</returns>
		public abstract bool IsMoreGeneral( Classifier Spec );

		/// <summary>
		/// 交差
		/// </summary>
		/// <param name="C">交差相手</param>
		public abstract void Crossover( Classifier C );

		/// <summary>
		/// situationに基づく突然変異
		/// </summary>
		/// <param name="S">situation</param>
		public abstract void Mutation( State S );

		/// <summary>
		/// 包摂できるか
		/// </summary>
		/// <param name="C">包摂相手</param>
		/// <returns>包摂できる(true)</returns>
		public abstract bool DoesSubsume( Classifier C );

		/// <summary>
		/// #を考慮せずに同じか
		/// </summary>
		/// <param name="obj">比較対象</param>
		/// <returns>同じ(true)</returns>
		/// 
		public override bool Equals( object obj )
		{
			if( obj == null || this.GetType() != obj.GetType() )
			{
				return false;
			}

			Classifier C = ( Classifier )obj;

			return ( ( this.C.Equals( C.C ) ) /*&& ( this.A == C.A )*/ );
		}

		/// <summary>
		/// HashCode
		/// </summary>
		/// <returns>HashCode</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}