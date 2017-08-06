using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	class MultiplexerEnvironment : Environment
	{
		// Multiplexer用AddressBit
		private int AddressBit;

		// Environment作成
		public MultiplexerEnvironment( int Length )
		{
			this.Length = Length;
			this.Number = 2;    // 2進数

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
			return ReferenceBits[Convert.ToInt32( AddressBits, 2 )];
		}

        // Actionに対するReward
        //      public override double GetReward()
        //{

        //		return Configuration.Rho;

        //}

        public override double ExecuteAction(char act)
        {
            // シングルステップ問題
            this.Eop = true;
            if (this.Action == act)
            {
                return 1000;
            }
            else
            {
                return 0.0;
            }
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