using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	class StdList
	{
		/// <summary>
		/// 条件部
		/// </summary>
		public string C { protected set; get; }

		/// <summary>
		/// 行動部
		/// </summary>
		public char A { protected set; get; }

		/// <summary>
		/// 出現回数
		/// </summary>
		public int T { set; get; }

		/// <summary>
		/// 平均
		/// </summary>
		public double M { set; get; }

		/// <summary>
		/// 条件部の分散
		/// </summary>
		public double[] Sigma { protected set; get; }

		/// <summary>
		/// 現在の分散(算出済み)
		/// </summary>
		public double S {  set; get; }

		private bool ConvergenceFlag = false;

		public int ConvergenceTime = -1;

	    public StdList()
	    {
           
           
            
            this.T = 0;
            this.M = 0;
            this.S = 0;
	       
	    }

	    public StdList(StdList toCopy)
	    {
            this.T = toCopy.T;
            this.M = toCopy.M;
            this.S = toCopy.S;
	    }

	    public StdList Clone()
	    {
	        var clone=new StdList();
	        clone.C = C;
	        clone.T = T;
	        clone.M = M;
	        clone.S = S;
            return clone;

	    }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="State">条件部(10進数)</param>
        /// <param name="Action">行動部(2進数)</param>
        public StdList(string State, char Action)
        {
            //XCSI 表現に変える

            string XCSI_state = "";
            for (int i = 0; i < State.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (j == Convert.ToInt32(State.Substring(i, 1)))
                    {
                        XCSI_state = XCSI_state + "0";
                    }
                    else
                    {
                        XCSI_state = XCSI_state + "*";
                    }
                }

            }

            this.C = XCSI_state;
            this.A = Action;
            this.T = 0;
            this.M = 0;
            this.S = 0;
            this.ConvergenceFlag = false;

            // 分散の記録初期化
            this.Sigma = new double[Configuration.LookBackSigma + 1];
            for (int i = 0; i < this.Sigma.Count(); i++)
            {
                this.Sigma[i] = 0;
            }
        }



        public void Update( double Reward )
		{
			// Sigma[2]まで一つずらし
			for( int i = this.Sigma.Count() - 1; i > 1; i-- )
			{
				this.Sigma[i] = this.Sigma[i - 1];
			}
			// 計算済みのSを代入
			this.Sigma[1] = this.S;

			this.T++;
			double X = Reward - this.M;
			this.M += X / this.T;
			this.Sigma[0] += ( this.T - 1 ) * X * X / this.T;
			// 現在のSを更新
			this.S = Math.Sqrt( this.Sigma[0] / ( this.T - 1 ) );
		}

		// 許容誤差ε0用分散収束
		public bool IsConvergenceSigma()
		{
			if( this.ConvergenceFlag )
			{
				return true;
			}

			if(this.T < this.Sigma.Count())
			{
				return false;
			}

			double Sum = 0;
			
			for(int i = 1; i < this.Sigma.Count(); i++)
			{
				Sum += Math.Abs( this.Sigma[i] - this.S );
			}

			if(Sum < Configuration.DifferenceSigma)
			{
				this.ConvergenceFlag = true;
				if(this.ConvergenceTime == -1)
				{
					this.ConvergenceTime = Configuration.T;
				}
			}

			return Sum < Configuration.DifferenceSigma;
		}

		// 被包摂か
		public bool IsIncluded(String State)
		{
			/*if(this.A != Action)
			{
				return false;
			}*/

			int i = 0;
			do
			{
				if( State[i] != '0' && this.C[i] != State[i] )
				{
					return false;
				}
				i++;
			} while( i < State.Length );

			return true;
		}

        //　自分の状態と　他の状態　2つの　分散と平均を用いて　e0 を推測機構
        //public void EstimateEpssilon0(StdList other)
        //{
	       
        //   S = Math.Sqrt(

        //        (T * Math.Pow(S, 2) + other.T * Math.Pow(other.S, 2)) / (T + other.T)

        //        + T * other.T * Math.Pow((　M - other.M), 2) / ((　T + other.T) * (　T + other.T - 1)) 

        //        );

        //    if (double.IsNaN(S))
        //    {
        //        Console.Read();
        //    }
        //    M = (M*T+other.M*other.T)/(T+other.T);
        //    T = T + other.T;
        //}
	}
}