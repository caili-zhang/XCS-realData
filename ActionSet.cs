using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	abstract class ActionSet
	{
		/// <summary>
		/// 保持するClassifierの集合
		/// </summary>
		public List<Classifier> CList { protected set; get; }

		/// <summary>
		/// 保持するClassifier表示
		/// </summary>
		public abstract void Show();

		/// <summary>
		/// Fitness更新
		/// </summary>
		/// <param name="Pop">Population</param>
		/// <param name="P">報酬の値</param>
		public abstract void Update( Population Pop, double P, StdList Sigma );

		/// <summary>
		/// Fitness更新
		/// </summary>
		protected abstract void UpdateFitness();

		/// <summary>
		/// Subsumpiton
		/// </summary>
		/// <param name="Pop">Population</param>
		protected abstract void Subsumption( Population Pop );

		/// <summary>
		/// Classifier消去
		/// </summary>
		/// <param name="C"></param>
		public abstract void Remove( Classifier C );

		/// <summary>
		/// GA
		/// </summary>
		/// <param name="Situation">situation</param>
		/// <param name="P">Population</param>
		public abstract void RunGA( State Situation, Population P );

		/// <summary>
		/// 親選択
		/// </summary>
		/// <returns>親</returns>
		protected abstract Classifier SelectOffspring();
	}
}