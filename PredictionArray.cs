using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	abstract class PredictionArray
	{
		// 行動選択用
		public double?[] PA { set; get; }
		// 行動決定
		abstract public char SelectAction();
		// マルチステップ問題用
		abstract public double MAX();
	}
}