using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	class Configuration
	{
		/// <summary>
		/// 乱数用シード
		/// </summary>
		public static int Seed { set; get; }

		/// <summary>
		/// situation, Conditionの長さ
		/// </summary>
		public static int L { set; get; }

		/// <summary>
		/// Populationの最大Classifier数
		/// </summary>
		public static int N { set; get; }

		/// <summary>
		/// situation, Conditionの種類
		/// </summary>
		public static string Type { set; get; }

		/// <summary>
		/// Covering閾値
		/// </summary>
		public static int Theta_mna { set; get; }

		/// <summary>
		/// 削除閾値(経験値)
		/// </summary>
		public static int Theta_del { set; get; }

		/// <summary>
		/// Covering時の#に変化させる割合
		/// </summary>
		public static double P_sharp { set; get; }

		/// <summary>
		/// 乱数生成用
		/// </summary>
		public static MersenneTwister MT { set; get; }

		/// <summary>
		/// 問題用乱数生成用
		/// </summary>
		public static MersenneTwister MT_P { set; get; }

		/// <summary>
		/// Covering時の初期値
		/// </summary>
		public static double P_I { set; get; }
		/// <summary>
		/// Covering時の初期値
		/// </summary>
		public static double Epsilon_I { set; get; }
		/// <summary>
		/// Covering時の初期値
		/// </summary>
		public static double F_I { set; get; }

		/// <summary>
		/// 実時間
		/// </summary>
		public static int T { set; get; }

		/// <summary>
		/// Fitnessの平均値の割合
		/// </summary>
		public static double Delta { set; get; }

		/// <summary>
		/// ランダム行動選択割合
		/// </summary>
		public static double P_explr { set; get; }

		/// <summary>
		/// MultiStep問題の割引率
		/// </summary>
		public static double Gamma { set; get; }

		/// <summary>
		/// 学習率
		/// </summary>
		public static double Beta { set; get; }

		/// <summary>
		/// 報酬
		/// </summary>
		public static double Rho { set; get; }

		/// <summary>
		/// Fitness計算パラメータ
		/// </summary>
		public static double Epsilon_0 { set; get; }

		/// <summary>
		/// Fitness計算パラメータ
		/// </summary>
		public static double Alpha { set; get; }

		/// <summary>
		/// Fitness計算パラメータ 
		/// </summary>
		public static double Nyu { set; get; }

		/// <summary>
		/// AS  包摂
		/// </summary>
		public static bool DoActionSetSubsumption { set; get; }

        /// <summary>
		/// POP 包摂
		/// </summary>
		public static bool DoPopSubsumption { set; get; }
        /// <summary>
        /// 包摂閾値
        /// </summary>
        public static int Theta_sub { set; get; }

		/// <summary>
		/// GA閾値
		/// </summary>
		public static double Theta_GA { set; get; }

		/// <summary>
		/// Crossover割合
		/// </summary>
		public static double Chai { set; get; }

		/// <summary>
		/// 突然変異割合
		/// </summary>
		public static double Myu { set; get; }

        /// <summary>
        /// 重なる部分の許す範囲　0~1
        /// </summary>
        public static double CoverPersentage { set; get; }

		/// <summary>
		/// GA時親に包摂
		/// </summary>
		public static bool DoGASubsumption { set; get; }

		/// <summary>
		/// 試行回数
		/// </summary>
		public static int Iteration { set; get; }

		/// <summary>
		/// 固定ノイズ率
		/// </summary>
		public static double NoiseRate { set; get; }

		/// <summary>
		/// 単純移動平均(Simple Moving Average)
		/// </summary>
		public static int SMA { set; get; }

		/// <summary>
		/// ノイズの最大幅
		/// </summary>
		public static double NoiseWidth { set; get; }

		/// <summary>
		/// ImbalanceLevel (2^{-ir}と用いられる)
		/// </summary>
		public static int ImbalanceLevel { set; get; }

		/// <summary>
		/// ActionSet選択用
		/// </summary>
		public static string ASName { set; get; }

		/// <summary>
		/// 包摂用経験閾値
		/// </summary>
		public static int ExpThreshold { set; get; }

		/// <summary>
		/// 分散差分許容範囲
		/// </summary>
		public static double DifferenceSigma { set; get; }

		/// <summary>
		/// 分散が安定したか確認する見返す数
		/// </summary>
		public static int LookBackSigma { set; get; }

		/// <summary>
		/// 分散記録
		/// </summary>
		public static StdList[] Stdlist { set; get; }

		/// <summary>
		/// すべての分散が収束したか
		/// </summary>
		/// <returns></returns>
		public static bool IsConvergenceSigma()
		{
			foreach(StdList S in Stdlist)
			{
				if(!S.IsConvergenceSigma())
				{
					return false;
				}
			}
			return true;
		}


	    /// <summary>
	    /// VT の収束判定
	    /// </summary>
	    /// <returns></returns>
	    public static bool IsConvergenceVT { set; get; }


        /// <summary>
        /// VT 収束したものを保存する
        /// </summary>
        /// <returns></returns>
        public static StdList[] ConvergentedVT { set; get; }

	    /// <summary>
		/// epsilon差分許容範囲
		/// </summary>
		public static double DifferenceEpsilon { set; get; }

		/// <summary>
		/// Epsilonが安定するか見返す数
		/// </summary>
		public static int LookBackEpsilon { set; get; }

		public static StreamWriter ESW;

		public static int Count;

		/// <summary>
		/// sigmaが落ち着いて利用されているか
		/// </summary>
		public static bool FlagSigma { set; get; }
		/// <summary>
		/// Epsilonが落ち着いて利用されているか
		/// </summary>
		public static bool FlagEpsilon { set; get; }
		/// <summary>
		/// 手法開始時刻
		/// </summary>
		public static int StartTime { set; get; }

		/// <summary>
		/// εの学習率
		/// </summary>
		public static double LearningRateEpsilon { set; get; }

        /// <summary>
        /// 全体の報酬　手法のフラグ
        /// </summary>
        public static bool RewardListFlag { set; get; }

        /// <summary>
        /// 全体の報酬
        /// </summary>
        public static List<double> RewardList { set; get; }

        /// <summary>
        /// 全体の報酬平均
        /// </summary>
        public static double RewardAverage { set; get; }

		/// <summary>
		/// 分類子成熟期限
		/// </summary>
		public static int MatureTime { set; get; }
        //1000##　の発生　収束　消失　見たい
		
        public static StreamWriter Problem;
        
        

		/// <summary>
		/// 提案手法(中田)
		/// </summary>
		public static double URE_Epsilon0 { set; get; }

		public static string pppp;

		/// <summary>
		/// 評価用環境
		/// </summary>
		public static Environment ExploitEnv { set; get; }

		/// <summary>
		/// ClassImbalance用
		/// </summary>
		public static List<double> ZeroList { set; get; }
		public static List<double> OneList { set; get; }
		/// <summary>
		/// トーナメント選択
		/// </summary>
		public static double Tau { set; get; }
	}
}