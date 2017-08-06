using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	abstract class Environment
	{
		/// <summary>
		/// 現時点でのsituation
		/// </summary>
		public State s { protected set; get; }

		/// <summary>
		/// 持っているsituationに対する正解行動(嘘つく(ノイズ)場合有り)
		/// </summary>
		public char Action { protected set; get; }

		/// <summary>
		/// situationの長さ
		/// </summary>
		public int Length { protected set; get; }

		/// <summary>
		/// #を抜いた文字の種類
		/// </summary>
		public int Number { protected set; get; }

		/// <summary>
		/// end of problem
		/// </summary>
		public bool Eop { set; get; }

		/// <summary>
		/// Populationへsituationを渡す
		/// </summary>
		/// <returns>situation</returns>
		abstract public State GetState();

		/// <summary>
		/// situationに対する答え(ここでノイズを考える)
		/// </summary>
		/// <param name="S">situation</param>
		/// <returns>situationに対する正解Action</returns>
		abstract protected char ActionCalculation( string S );

		/// <summary>
		/// Actionを受け取りRhoを返す
		/// </summary>
		/// <param name="act">選択行動</param>
		/// <returns>報酬</returns>
		abstract public double ExecuteAction( char act );

		/// <summary>
		/// Actionを受け取り正解かどうか返す
		/// </summary>
		/// <param name="act">行動</param>
		/// <returns>正解:1, 不正解:0</returns>
		abstract public int ActionExploit( char act );

	    //public abstract double GetReward();
	}
}