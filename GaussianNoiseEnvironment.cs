using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	class GaussianNoiseEnvironment : Environment
	{
		// Multiplexer用AddressBit
		private int AddressBit;
		private double NoiseWidth;

		// Environment作成
		public GaussianNoiseEnvironment( int Length, double NoiseWidth )
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
            
                this.s = new BinaryState(this.Length);//長さ設定
      
                State state = this.s.GetState();//BinaryState のGetStateで　ランダムsta
      

                    this.Action = this.ActionCalculation(state.state);
                    //if (state.state.Substring(0, 4) == "0000")//state から　0000を取り除く
                    //    return  GetState();
                    //else
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

		// Actionに対するReward　ばらつき幅同じ
		public override double GetReward(  )
		{ 
			// シングルステップ問題
			this.Eop = true;
            //最初の4ビットを保存する
            string s4 =this.s.state.Substring(0, 4);
            //s4 の中に　1の位置を保存するリストを作る 1010 の場合　L4= <0,2>
        
                List<int> L4 = new List<int>();

                for (int i = 0; i < s4.Length; i++)
                {
                    if (s4[i] == '1')
                    { L4.Add(i); }  //S4 中に　１の位置を保存する
                }
                if (L4.Count != 0)
                {
                    int index = (int)(Configuration.MT_P.NextDouble() * L4.Count);//L4=<0,2> の場合index = 0 or 1

                    int IndexOfS4 = L4[index];

                    switch (IndexOfS4)
                    {
                        case 0:
                            //1000## return 1000+e
                            return Configuration.Rho + this.MakeNoise();

                        case 1:
                            //0100## return 0100-e
                            return Configuration.Rho - this.MakeNoise();

                        case 2:
                            //0010## return 0+e
                            return 0.0 + this.MakeNoise();

                        case 3:
                            //0001## return 0-e    
                            return 0.0 - this.MakeNoise();

                        default:
                            return -10000;

                    }
                }
                else// 0000の場合 完全ランダムな値与える、排除したい
                {
                    return Configuration.MT_P.NextDouble()*Configuration.Rho;
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

		/// <summary>
		/// ガウシアンノイズ生成
		/// </summary>
		/// <returns>ガウシアンノイズ</returns>
		protected double MakeNoise()
		{
			double A = Configuration.MT_P.NextDoublePositive();
			double B = Configuration.MT_P.NextDoublePositive();

			return Math.Abs(this.NoiseWidth * ( Math.Sqrt( -2 * Math.Log( A, Math.E ) ) * Math.Sin( 2 * Math.PI * B ) ) );
		}
	}
}