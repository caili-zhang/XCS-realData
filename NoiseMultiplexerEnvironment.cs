﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	class NoiseMultiplexerEnvironment : Environment
	{
		// Multiplexer用AddressBit
		private int AddressBit;
		private double NoiseWidth;
		private int AddRef;	// アドレスビットが指す場所

		// Environment作成
		public NoiseMultiplexerEnvironment( int Length, double NoiseWidth )
		{
			this.Length = Length;
			this.Number = 2;    // 2進数

			this.NoiseWidth = NoiseWidth;

			// MultiplexerのAddressBit, ReferenceBit算出
			int k = 0;
			int ReferenceBit = 1;
			do
			{
				k++;
				ReferenceBit *= 2;
			} while( this.Length > k + ReferenceBit );
			this.AddressBit = k;	// AddressBit桁数確定
		}

		// Populationに(ランダムな)Stateを答えを算出してから渡す
		override public State GetState()
		{
			this.s = new BinaryState( this.Length );
			State state = this.s.GetState();

			this.Action = this.ActionCalculation( state.state );

			return state;
		}

		// Multiplexer答え算出
		override protected char ActionCalculation( string S )
		{
			// AddressBit取得
			string AddressBits = S.Substring( 0, this.AddressBit );
			// 残りのReferenceBit取得
			string ReferenceBits = S.Substring( this.AddressBit );
			// ReferenceBitのAddressBit番目
			this.AddRef = Convert.ToInt32( AddressBits, 2 );
			return ReferenceBits[this.AddRef];
		}

		// Actionに対するReward
        public override double GetReward()
		{
			// シングルステップ問題
			this.Eop = true;
			double p = 0;
			// ばらつき度合いの決定
			if( this.AddRef == 1 )
			{
				p = 0.5;
			}
			else if( this.AddRef == 2 )
			{
				p = 0.8;
			}
			else if( this.AddRef == 3 )
			{
				p = 1;
			}

			p *= this.NoiseWidth * ( 1.0 - 2 * Configuration.MT.NextDouble() );

			//if( this.Action == act )
			//{
				return Configuration.Rho + p;
			//}
			//else
			//{
				//return 0.0 + p;
			//}
		}

		// Actionの正解･不正解
		public override int ActionExploit( char act )
		{
			// シングルステップ問題
			this.Eop = true;
			if( this.Action == act )
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}
	}
}