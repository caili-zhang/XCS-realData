using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	abstract class State
	{
		/// <summary>
		/// situation, Condition
		/// </summary>
		public string state { set; get; }

		/// <summary>
		/// 長さ
		/// </summary>
		public int Length { protected set; get; }

		/// <summary>
		/// #を除く文字の種類
		/// </summary>
		public static int Number { protected set; get; }

		/// <summary>
		/// situation, Conditionに占める#の数
		/// </summary>
		public int NumberOfSharp { protected set; get; }
		
		/// <summary>
		/// Generality
		/// </summary>
		public double Generality { protected set; get; }

		/// <summary>
		/// 状態をPopulationにEnvironment経由で渡す
		/// </summary>
		/// <returns>situation</returns>
		abstract public State GetState();

		/// <summary>
		/// Stateに一致するか(#を考慮)
		/// </summary>
		/// <param name="S">比較対象</param>
		/// <returns>一致(true)</returns>
		public bool Match( State S )
		{
			if( this.state.Length != S.state.Length )
			{
				return false;
			}

			for( int i = 0; i < this.state.Length; i++ )
			{
				if( ( this.state[i] != S.state[i] ) && ( this.state[i] != '0' ) )
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Covering(#に一部を変える)
		/// </summary>
		abstract public void Covering();

		//// #の数
		//abstract public int NumberOfSharp();

		/// <summary>
		/// i番目の要素を自分とSで入れ替える
		/// </summary>
		/// <param name="S">入れ替え対象</param>
		/// <param name="i">入れ替える場所</param>
		public abstract void Switch( State S, int i );

		/// <summary>
		/// #を考慮せずに同じものか
		/// </summary>
		/// <param name="obj">比較対象</param>
		/// <returns>一致(true)</returns>
		public override bool Equals( object obj )
		{
			if( obj == null || this.GetType() != obj.GetType() )
			{
				return false;
			}

			State S = ( State )obj;
			for( int i = 0; i < this.state.Length; i++ )
			{
				if( this.state[i] != S.state[i] )
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Equalの相方
		/// </summary>
		/// <returns>HashCode</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// State表示
		/// </summary>
		public abstract void Show();

		/// <summary>
		/// #の数カウント(生成後に実行必須)
		/// </summary>
		public void CountSharp()
		{
			int n = 0;
			for( int i = 0; i < this.state.Length; i++ )
			{
				if( this.state[i] == '0' )
				{
					n++;
				}
			}
			this.NumberOfSharp = n;

			this.Generality = ( double )n / this.state.Length;
		}

		/// <summary>
		/// すべての組み合わせを返す
		/// </summary>
		/// <returns>すべての組み合わせを配列で返す</returns>
		//public static abstract State[] AllState(int Length);
	}
}