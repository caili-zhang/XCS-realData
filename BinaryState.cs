using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	class BinaryState : State
	{
		// 長さと種類をそろえる(GetStateと統合可?)
		public BinaryState( int Length )
		{
			this.Length = Length;
			Number = 2;    // 0, 1
		}

		// すべてをリストアップ用
		private BinaryState( string S )
		{
			this.state = S;
			this.Length = S.Length;
			this.NumberOfSharp = 0;
		}

		// コピーコンストラクタ
		public BinaryState( State S )
		{
			this.state = S.state;
			this.Length = S.Length;
			this.NumberOfSharp = S.NumberOfSharp;
		}

		// Environment経由で0, 1で構成される長さLengthのStateを渡す(ランダム 
        
        
        
		override public State GetState()
		{
			this.state = "";

			for( int i = 0; i < this.Length; i++ )
			{
				//this.state += Configuration.MT_P.Next() % Number;
				this.state += ( int )( Configuration.MT_P.NextDouble() * Number );
			}

			this.CountSharp();

			return this;
		}

		// 確率的に#に変える
		public override void Covering()
		{
			string S = this.state;
			string CoveredState = "";
			for( int i = 0; i < S.Length; i++ )
			{
                if (S[i] == '0') {
                    CoveredState += '0';
                }
                else
                {
                    if (Configuration.MT.NextDouble() < Configuration.P_sharp)
                    {
                        CoveredState += '0';
                    }
                    else
                    {
                        CoveredState += S[i];
                    }
                }
				
			}
			this.state = CoveredState;
			this.CountSharp();
		}

		//public override int NumberOfSharp()
		//{
		//	int n = 0;
		//	for(int i = 0; i < this.state.Length; i++)
		//	{
		//		if(this.state[i] == '#')
		//		{
		//			n++;
		//		}
		//	}
		//	return n;
		//}

		// i番目を入れ替え
		public override void Switch( State S, int i )
		{
			string t = "";
			string s = "";

			for( int j = 0; j < this.Length; j++ )
			{
				if( i == j )
				{
					t += S.state[j];
					s += this.state[j];
				}
				else
				{
					t += this.state[j];
					s += S.state[j];
				}
			}

			this.state = t;
			S.state = s;

			this.CountSharp();
			S.CountSharp();
		}

		// 表示
		public override void Show()
		{
			Console.WriteLine( this.state );
		}

		/// <summary>
		/// すべての取り得るStateを生成
		/// </summary>
		/// <param name="Length">Multiplexerの長さ</param>
		/// <returns>すべての取り得るState</returns>
		public static State[] AllState( int Length )
		{
			State[] All = new State[( int )Math.Pow( 2, Length )];
			for( int i = 0; i < All.Length; i++ )
			{
				string S = Convert.ToString( i, 2 );
				// 頭に足りない分0を付加
				while( S.Length < Length )
				{
					S = "0" + S;
				}
				All[i] = new BinaryState( S );
			}
			return All;
		}
	}
}