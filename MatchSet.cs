using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	abstract class MatchSet
	{
		/// <summary>
		/// 保持するClassifierの集まり
		/// </summary>
		public List<Classifier> CList { protected set; get; }

		/// <summary>
		/// 足りないときのCovering
		/// </summary>
		/// <param name="S">situation</param>
		/// <param name="P">Population</param>
		protected abstract void Covering( State S, Population P );

		/// <summary>
		/// 保持するClassifier表示
		/// </summary>
		public abstract void Show();

		/// <summary>
		/// Actionに一致するClassifierをActionSetに渡す
		/// </summary>
		/// <param name="Action">選択行動</param>
		/// <returns>Actionに一致するClassifierの集合</returns>
		public abstract List<Classifier> MatchAction( char Action );
	}
}