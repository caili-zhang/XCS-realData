using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	abstract class Population
	{
		public List<Classifier> CList { protected set; get; }

		/// <summary>
		/// Populationの最大サイズ readonlyが望ましい
		/// </summary>
		public int Number { protected set; get; }

		/// <summary>
		/// 分類子追加 かぶらないと分かっているのでInsertと統合可能
		/// </summary>
		/// <param name="C">生成された分類子を追加</param>
		abstract public void Add( Classifier C );

		/// <summary>
		/// 分類子削除
		/// </summary>
		/// <param name="C">当てはまる分類子を削除</param>
		abstract public void Remove( Classifier C );

		/// <summary>
		/// Populationに属するClassifierを表示
		/// </summary>
		abstract public void Show();

		/// <summary>
		/// situationに一致するClassifierをMatchSetに渡す
		/// </summary>
		/// <param name="S">situation</param>
		/// <returns>situationに一致するClassifierの集合</returns>
		abstract public List<Classifier> MatchSituation( State S );

		/// <summary>
		/// 整理
		/// </summary>
		abstract public void Delete();

		/// <summary>
		/// 削除条件計算
		/// </summary>
		/// <param name="C">Classifier</param>
		/// <param name="AvFitness">AvFitness</param>
		/// <returns>計算結果</returns>
		abstract protected double DeletionVote( Classifier C, double AvFitness );

		/// <summary>
		/// 一致検査をして追加
		/// </summary>
		/// <param name="C">追加されるClassifier</param>
		abstract public void Insert( Classifier C );

		/// <summary>
		/// Numerosity合計値
		/// </summary>
		abstract public int CountNumerosity();
        abstract public void Compact();
    }
}