using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MathNet.Numerics.Statistics;

//10月のバージョン 再整理　160106 加重平均e0 actionset を外す
namespace XCS
{
    class Program
    {
        static void Main(string[] args)
        {
            // 環境設定
            // 未指定用(デフォルト)
            Configuration.Seed = 0;
            Configuration.NoiseRate = 0;
            Configuration.ImbalanceLevel = 0;
            Configuration.NoiseWidth = 0;
            Configuration.ASName = "CS";
            Configuration.L = 8;
            
            Environment Env = new OneBitEnvironment();
            Configuration.Theta_sub = 20;
            Configuration.ExpThreshold = 20;
            Configuration.DifferenceSigma = 0.1;//for real data  0~1.0
            Configuration.LookBackSigma = 15;
            Configuration.DifferenceEpsilon = 0.1;//for real data 0.0~1.0
            Configuration.LookBackEpsilon = 15;
            Configuration.P_sharp = 0.35;
            Configuration.CoverPersentage = 0.155;//重なる部分の許す範囲 0~1.0


            string EnvName = "hoge";
            string Comment = "hoge";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-s" | args[i] == "-S" | args[i] == "--Seed" | args[i] == "--seed")
                {


                    Configuration.Seed = int.Parse(args[++i]);
                }

                if (args[i] == "-c" | args[i] == "-C" | args[i] == "--Complexity" | args[i] == "--complexity")
                {
                    Configuration.NoiseRate = double.Parse(args[++i]);
                    Configuration.ImbalanceLevel = (int)double.Parse(args[i]);  // int型に変換

                    Configuration.NoiseWidth = double.Parse(args[i]);
                }

                if (args[i] == "-a" | args[i] == "-A" | args[i] == "--as" | args[i] == "--As")
                {
                    Configuration.ASName = args[++i];
                }

                if (args[i] == "-e" | args[i] == "-E" | args[i] == "--env" | args[i] == "--Env")
                {
                    EnvName = args[++i];

                    if (args[i] == "fourState")
                    {
                        Env = new OneBitEnvironment();
                    }

                    else
                    {
                        // 普通のMultiplexer問題
                        //Env = new MultiplexerEnvironment( Configuration.L );
                    }
                }

                if (args[i] == "--ts" | args[i] == "--Ts" | args[i] == "--ThetaSub")
                {
                    Configuration.Theta_sub = int.Parse(args[++i]);
                    Configuration.ExpThreshold = int.Parse(args[i]);
                }

                if (args[i] == "--ds" | args[i] == "--Ds" | args[i] == "--DifferenceSigma")
                {
                    // 分散差分許容範囲
                    Configuration.DifferenceSigma = double.Parse(args[++i]);
                }

                if (args[i] == "--ls" | args[i] == "--Ls" | args[i] == "--LookbackSigma")
                {
                    // 分散が安定したか見返す数
                    Configuration.LookBackSigma = int.Parse(args[++i]);
                }

                if (args[i] == "--de" | args[i] == "--De" | args[i] == "--DifferenceEpsilon")
                {
                    // 各分類子の差分許容範囲
                    Configuration.DifferenceEpsilon = double.Parse(args[++i]);
                }

                if (args[i] == "--le" | args[i] == "--Le" | args[i] == "--LookbackEpsilon")
                {
                    // 各分類子のepsilon(分散)が安定したか見返す数
                    Configuration.LookBackEpsilon = int.Parse(args[++i]);
                }

                if (args[i] == "--cm" | args[i] == "--comment" | args[i] == "--Comment")
                {
                    Comment = args[++i];
                }

                if (args[i] == "-l" | args[i] == "--Length" | args[i] == "--length")
                {
                    // situation(Condition)の長さ
                    Configuration.L = int.Parse(args[++i]);
                    //Configuration.ExploitEnv = new MultiplexerEnvironment( Configuration.L );
                }

                if (args[i] == "--ps" | args[i] == "--Ps" | args[i] == "--PSharp")
                {
                    Configuration.P_sharp = double.Parse(args[++i]);
                }

                if (args[i] == "--per" | args[i] == "-percentage")
                {
                    Configuration.CoverPersentage = double.Parse(args[++i]);
                }

            }


            // 変数設定
            Config(args);

            // フォルダ名用時間取得
            DateTime dt = DateTime.Now;

            // フォルダ指定
            Configuration.pppp = dt.Year + "" + dt.Month.ToString("D2") + "" + dt.Day.ToString("D2");
            Configuration.pppp += "_" + Comment + "_" + Configuration.L + "_" + Configuration.P_sharp + "_" + Configuration.Theta_sub + "_" + Configuration.DifferenceSigma + "_" + Configuration.LookBackSigma;
            string Path = "./" + Configuration.pppp + "/" + Configuration.NoiseWidth + "/";

            Path += dt.Year + "" + dt.Month.ToString("D2") + "" + dt.Day.ToString("D2") + "" + dt.Hour.ToString("D2") + "" + dt.Minute.ToString("D2") + "" + dt.Second.ToString("D2");
            Path += "_s" + Configuration.Seed;
            Path += "_c" + Configuration.NoiseWidth;
            Path += "_l" + Configuration.L;
            Path += "_a" + Configuration.ASName;
            Path += "_e" + EnvName;
            Path += "_ts" + Configuration.Theta_sub;
            Path += "_ds" + Configuration.DifferenceSigma;
            Path += "_ls" + Configuration.LookBackSigma;
            Path += "_de" + Configuration.DifferenceEpsilon;
            Path += "_le" + Configuration.LookBackEpsilon;
            Path += "_ps" + Configuration.P_sharp;
            Path += "_per" + Configuration.CoverPersentage;
            //foreach( string parameter in args )
            //{
            //	Path += ( "_" + parameter );
            //}

            System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(Path);
            System.IO.Directory.SetCurrentDirectory(Path);
            
            Configuration.Problem = new StreamWriter("./problem" + ".csv", true, System.Text.Encoding.GetEncoding("shift_jis"));

            Configuration.ESW = new StreamWriter("./epsilon_" + Configuration.Seed + "CnoiseWidth_" + Configuration.NoiseWidth
                + "AS_" + "CS" + "ET_" + Configuration.ExpThreshold + "DS_" + Configuration.DifferenceSigma + "LS_" + Configuration.LookBackSigma
                + "DE_" + Configuration.DifferenceEpsilon + "LE_" + Configuration.LookBackEpsilon + ".csv", true, System.Text.Encoding.GetEncoding("shift_jis"));


            // 初期化
            // Population初期化
            Population P = new NormalPopulation(Configuration.N);

            Experiment(Env, P);
        }

        public static void Config(string[] args)
        {
            // 変数設定

            // Populationの最大サイズ
            Configuration.N = 400;
            //Configuration.N = 800;	// 150116
            if (Configuration.L == 6)
            {
                Configuration.N = 400;
            }

            else if (Configuration.L == 11)
            {
                Configuration.N = 800;
            }
            else if (Configuration.L == 20)
            {
                Configuration.N = 2000;
            }
            // situation(Condition)の種類(進数)
            //Configuration.Type = "Binary";
            // Covering閾値(行動の数)
            //Configuration.Theta_mna = 2;
            Configuration.Theta_mna = 0;

            // Covering時の#に変化させる割合

            // 乱数生成
            MersenneTwister MT = new MersenneTwister(Configuration.Seed);
            //MersenneTwister MT = new MersenneTwister();
            Configuration.MT = MT;
            Configuration.MT_P = new MersenneTwister(Configuration.Seed);
            // Covering時初期値
            Configuration.P_I = 0.01;
            Configuration.Epsilon_I = 0.0;
            Configuration.F_I = 0.01;
            // 削除閾値(経験値)
            Configuration.Theta_del = 20;
            // 削除閾値(Fitness)
            Configuration.Delta = 0.1;
            // Epsilon-greedyのランダム割合
            Configuration.P_explr = 1.0;    // 常にランダム
                                            // MultiStep問題の割引率
            Configuration.Gamma = 0.71;
            // 学習割合
            Configuration.Beta = 0.2;
            // 報酬
            Configuration.Rho = 1000;
            // Fitnessの計算パラメータ 下駄　1/1000
            Configuration.Epsilon_0 = 10;
            Configuration.Alpha = 0.1;
            Configuration.Nyu = 15;
            // 包摂
            Configuration.DoActionSetSubsumption = true;//
            Configuration.DoPopSubsumption = true;

            // 包摂閾値(経験値)
            //Configuration.Theta_sub = 20;
            //Configuration.Theta_sub = 200;	// 150116
            // GA閾値(TimeStamp)
            Configuration.Theta_GA = 25;
            // Crossover割合
            Configuration.Chai = 0.8;//komine
                                     // 突然変異割合
            Configuration.Myu = 0.04;//komine
                                     // GA時親に包摂
            Configuration.DoGASubsumption = true;//9-8 chou
                                                 // 試行回数
            Configuration.Iteration = 100000;
           
            // 単純移動平均
            Configuration.SMA = 100;
            //Configuration.SMA = 5000;	// 150116
            // 手法開始フラグ
            Configuration.FlagEpsilon = Configuration.FlagSigma = true;
            Configuration.StartTime = -1;
            // ε学習率
            Configuration.LearningRateEpsilon = 0.05;
            // 分類子学習期限
            //Configuration.MatureTime = 30;
            //Configuration.MatureTime = Configuration.LookBackEpsilon * 5;
            Configuration.MatureTime = Configuration.LookBackEpsilon * 4 / 5;
            Configuration.URE_Epsilon0 = -1;
            // トーナメント選択
            Configuration.Tau = 0.4;

            Configuration.IsConvergenceVT = false;
            //chou 全体平均記録手法
            Configuration.RewardListFlag = true;
            Configuration.RewardList = new List<double>();
            Configuration.RewardAverage = 0;

        }

        public static void Experiment(Environment Env, Population P)
        {
            // 変数初期化
            Configuration.T = 0;
            ActionSet PreviousAS = null;
            double PreviousRho = 0;
            State PreviousS = null;

            // stdlist収束
            bool ConvergenceStelist = false;

            // 移動平均計算用
            double[] RhoArray = new double[Configuration.Iteration];
            int[] Num = new int[Configuration.Iteration];
            //double[] Std = new double[Configuration.Iteration];

            Configuration.ZeroList = new List<double>();
            Configuration.OneList = new List<double>();

            List<string> DataList = Env.GetDataList();
            List<string> DistinctDataList = DataList.Distinct().ToList();

            int DistinctDataNum = 4;
            // 提案手法　入力データ個数分の分散
            Configuration.Stdlist = new StdList[DistinctDataNum];
            // 収束した　VTの値を保存する　　ちょう
            Configuration.ConvergentedVT = new StdList[DistinctDataNum];

            for (int i = 0; i < DistinctDataNum; i++)
            {
                Configuration.ConvergentedVT[i] = new StdList(DistinctDataList[i], '0');
                //Configuration.ConvergentedVT[i * 4 + 1] = new StdList(i, '1');
            }
            for (int i = 0; i < DistinctDataNum; i++)
            {
                Configuration.Stdlist[i] = new StdList(DistinctDataList[i], '0');
                //Configuration.Stdlist[i * 4 + 1] = new StdList( i, '1' );
            }

            // 実験1a ノイズを既知のものとして扱う
            if (Configuration.ASName == "WellKnown")
            {
                Configuration.Epsilon_0 += Configuration.NoiseWidth;
            }

            Configuration.Problem.WriteLine("state ,iter,P , cl.M ,cl.Epsilon , cl.F , cl.N , cl.Exp , cl.Ts ,cl.As , cl.Kappa ,cl.Epsilon_0 , cl.St , cl.GenerateTime");
            StreamWriter goodsleep1 = new StreamWriter("./goodsleep_rule1.csv");
            goodsleep1.WriteLine("state ,iter,P , cl.M ,cl.Epsilon , cl.F , cl.N , cl.Exp , cl.Ts ,cl.As , cl.Kappa ,cl.Epsilon_0 , cl.St , cl.GenerateTime");

            StreamWriter goodsleep2 = new StreamWriter("./goodsleep_rule2.csv");
            goodsleep2.WriteLine("state ,iter,P , cl.M ,cl.Epsilon , cl.F , cl.N , cl.Exp , cl.Ts ,cl.As , cl.Kappa ,cl.Epsilon_0 , cl.St , cl.GenerateTime");

            StreamWriter badsleep = new StreamWriter("./badsleep_rule.csv");
            badsleep.WriteLine("state ,iter,P , cl.M ,cl.Epsilon , cl.F , cl.N , cl.Exp , cl.Ts ,cl.As , cl.Kappa ,cl.Epsilon_0 , cl.St , cl.GenerateTime,converge");
            // メインループ
            #region main roop
            while (Configuration.T < Configuration.Iteration)
            {
                if (!Configuration.IsConvergenceVT)
                {
                    bool flag = true;
                    //入力データのSLが収束すれば、VTが収束とみなす。
                    foreach (StdList SL in Configuration.Stdlist)
                    {
                        if (flag && !SL.IsConvergenceSigma())
                        {
                            flag = false;
                            break;
                        }
                        
                    }
                    if (flag)	// 初めてTrue
                    {
                        Configuration.IsConvergenceVT = true;
                        //収束したVTを保存する
                        for (int i = 0; i < DistinctDataList.Count; i++)
                        {
                            Configuration.ConvergentedVT[i].M = Configuration.Stdlist[i].M;
                            //Configuration.ConvergentedVT[i * 4+1].M = Configuration.Stdlist[i * 2+1].M;

                            Configuration.ConvergentedVT[i].S = Configuration.Stdlist[i].S;
                            //Configuration.ConvergentedVT[i * 4 + 1].S = Configuration.Stdlist[i*2+1].S;

                            Configuration.ConvergentedVT[i].T = Configuration.Stdlist[i].T;
                            //Configuration.ConvergentedVT[i * 4 + 1].T = Configuration.Stdlist[i * 2 + 1].T;
                        }
                        // [P]の全てを新しい基準で再評価
                        foreach (Classifier C in P.CList)
                        {
                            // 加重平均
                            double ST = 0;
                            int SumT = 0;
                            foreach (StdList SL in Configuration.Stdlist)
                            {
                                if (SL.IsIncluded(C.C.state))
                                {
                                    ST += SL.S * SL.T;
                                    SumT += SL.T;
                                }

                            }
                            ST /= SumT;

                            SigmaNormalClassifier SNC = (SigmaNormalClassifier)C;
                            C.Epsilon_0 = ST + Configuration.Epsilon_0;
                            if (C.Exp > 2)
                            {
                                C.Epsilon = SNC.EpsilonList[0];
                            }
                        }
                    }
                }
                
                State S = Env.GetState();

                // MatchSet生成
                MatchSet M = new NormalMatchSet(S, P);
                Console.WriteLine("after matchset" + P.CountNumerosity());
                
                // ActionSetはただMをコピーするだけ,アクションがないから
                ActionSet AS;
                if (Configuration.ASName == "CS")
                {
                    AS = new ConditionSigmaActionSet(M.CList);
                }
                else
                {
                    AS = new NormalActionSet(M.CList);/*M.MatchAction(Action))*/;
                }
                
                char Action = '0';//action ないから、全部０にする
                double Rho = Env.ExecuteAction(Action);

                StdList Sigma = null;

                // 提案手法　分散の計算
                foreach (StdList SL in Configuration.Stdlist)
                {
                    if ((SL.C == S.state) /*&& ( SL.A == Action )*/ )
                    {
                        // situationの分散取得
                        SL.Update(Rho);
                        Sigma = SL;
                    }
                }

                // 提案手法(中田)
                if (Configuration.ASName == "CS")
                {
                    Configuration.URE_Epsilon0 = -1;

                    // 最小値
                    double d = Configuration.Rho;

                    foreach (SigmaNormalClassifier C in AS.CList)
                    {
                        if (d > C.S && C.IsConvergenceEpsilon())
                        {
                            d = Math.Sqrt(C.S / (C.St - 1));
                        }
                    }

                    Configuration.URE_Epsilon0 = d;
                }
                //chou 1000回の報酬平均を保存

                if (Configuration.T < 1000)
                {
                    Configuration.RewardList.Add(Rho);
                }
                if (Configuration.T == 1000)
                {
                    Configuration.RewardAverage = Configuration.RewardList.Mean();
                }

                // マルチステップ問題の終着またはシングルステップ問題
                if (Env.Eop)
                {
                    double p = Rho;
                    
                    AS.Update(P, p, Sigma);

                    Console.WriteLine("after AS update " + P.CountNumerosity());
                    AS.RunGA(S, P);
                    if (P.CountNumerosity() > 400)
                    {
                        Console.ReadLine();
                    }
                    Console.WriteLine("after GA " + P.CountNumerosity());
                    //if (Configuration.DoPopSubsumption)
                    //{
                    //    P.Subsumption();//do pop subsumption
                    //}

                    PreviousAS = null;
                }
                else
                {
                    PreviousAS = AS;
                    PreviousRho = Rho;
                    PreviousS = S;
                }

                Num[Configuration.T] = P.CList.Count();
                //Std[Configuration.T] = Math.Sqrt( Stdlist[20].Sigma / (Stdlist[20].T - 1));

                if (Configuration.StartTime < 0 && Configuration.FlagSigma && Configuration.FlagEpsilon)
                {
                    Configuration.StartTime = Configuration.T;
                }
                Configuration.FlagSigma = Configuration.FlagEpsilon = false;
                if (!ConvergenceStelist && (Configuration.ASName == "CS" || Configuration.ASName == "MaxCS" || Configuration.ASName == "Max" || Configuration.ASName == "Updatee0CS"))
                {
                    int i = 1;

                    foreach (StdList SL in Configuration.Stdlist)
                    {
                        i *= (SL.IsConvergenceSigma() ? 1 : 0);
                    }

                    if (i == 1)
                    {
                        StreamWriter stdSw = new StreamWriter("./ConvergenceVT_" + Configuration.T + "_" + Configuration.Seed + "CnoiseWidth" + Configuration.NoiseWidth
                + "AS_" + Configuration.ASName + "ET_" + Configuration.ExpThreshold + "DS_" + Configuration.DifferenceSigma + "LS_" + Configuration.LookBackSigma
                + "DE_" + Configuration.DifferenceEpsilon + "LE_" + Configuration.LookBackEpsilon + ".csv", true, System.Text.Encoding.GetEncoding("shift_jis"));

                        stdSw.WriteLine("condition,action,sigma,average,time,convergence");
                        foreach (StdList SL in Configuration.Stdlist)
                        {
                            stdSw.WriteLine(SL.C + "," + SL.A + "," + SL.S + "," + SL.M + "," + SL.T + "," + (SL.IsConvergenceSigma() ? 1 : 0));    // 1 : 収束
                        }
                        stdSw.Close();
                        ConvergenceStelist = true;
                    }
                }

                
                Console.WriteLine(Configuration.T);
                Console.WriteLine(P.CountNumerosity());
                Configuration.T++;

            }
            //最後の一回まとめる
            P.Subsumption();

            P.Show();
            
            #endregion 
            goodsleep1.Close();
            goodsleep2.Close();
            badsleep.Close();

            Configuration.Problem.Close();
            
            if ((Configuration.ASName == "CS" || Configuration.ASName == "MaxCS" || Configuration.ASName == "Max" || Configuration.ASName == "Updatee0CS"))
            {
                StreamWriter stdSw = new StreamWriter("./VarianceTable_" + Configuration.T + "_" + Configuration.Seed + "CnoiseWidth" + Configuration.NoiseWidth
            + "AS_" + Configuration.ASName + "ET_" + Configuration.ExpThreshold + "DS_" + Configuration.DifferenceSigma + "LS_" + Configuration.LookBackSigma
            + "DE_" + Configuration.DifferenceEpsilon + "LE_" + Configuration.LookBackEpsilon + ".csv", true, System.Text.Encoding.GetEncoding("shift_jis"));

                stdSw.WriteLine("condition,action,sigma,time,convergence,convergencetime");
                foreach (StdList SL in Configuration.Stdlist)
                {
                    stdSw.WriteLine(SL.C + "," + SL.A + "," + SL.S + "," + SL.T + "," + (SL.IsConvergenceSigma() ? 1 : 0) + "," + SL.ConvergenceTime);  // 1 : 収束
                }
                stdSw.Close();
            }

            //LD.Close();


            StreamWriter sw = new StreamWriter("./performance_" + Configuration.Seed + "CnoiseWidth_" + Configuration.NoiseWidth
                + "AS_" + Configuration.ASName + "ET_" + Configuration.ExpThreshold + "DS_" + Configuration.DifferenceSigma + "LS_" + Configuration.LookBackSigma
                + "DE_" + Configuration.DifferenceEpsilon + "LE_" + Configuration.LookBackEpsilon + ".csv", true, System.Text.Encoding.GetEncoding("shift_jis"));



            sw.WriteLine("Performance,PopulationSize," + Configuration.StartTime);

            for (int i = 0; i < RhoArray.Count() - Configuration.SMA; i++)
            {
                double R = 0;
                double N = 0;
                for (int j = 0; j < Configuration.SMA; j++)
                {
                    R += RhoArray[i + j];
                    N += Num[i + j];
                }
                R /= Configuration.SMA;
                N /= Configuration.SMA;

                sw.WriteLine(R + "," + N);
            }

            sw.Close();

            StreamWriter Zerosw = new StreamWriter("./Zero_per_" + Configuration.Seed + "CnoiseWidth_" + Configuration.NoiseWidth
                + "AS_" + Configuration.ASName + "ET_" + Configuration.ExpThreshold + "DS_" + Configuration.DifferenceSigma + "LS_" + Configuration.LookBackSigma
                + "DE_" + Configuration.DifferenceEpsilon + "LE_" + Configuration.LookBackEpsilon + ".csv", true, System.Text.Encoding.GetEncoding("shift_jis"));


            Zerosw.WriteLine("Performance,dummy," + Configuration.StartTime);

            for (int i = 0; i < Configuration.ZeroList.Count() - Configuration.SMA; i++)
            {
                double R = 0;
                double N = 0;
                for (int j = 0; j < Configuration.SMA; j++)
                {
                    R += Configuration.ZeroList[i + j];
                    N += Num[i + j];
                }
                R /= Configuration.SMA;
                N /= Configuration.SMA;

                Zerosw.WriteLine(R + "," + N);
            }

            Zerosw.Close();


            StreamWriter Onesw = new StreamWriter("./One_per_" + Configuration.Seed + "CnoiseWidth_" + Configuration.NoiseWidth
                + "AS_" + Configuration.ASName + "ET_" + Configuration.ExpThreshold + "DS_" + Configuration.DifferenceSigma + "LS_" + Configuration.LookBackSigma
                + "DE_" + Configuration.DifferenceEpsilon + "LE_" + Configuration.LookBackEpsilon + ".csv", true, System.Text.Encoding.GetEncoding("shift_jis"));

            Onesw.WriteLine("Performance,dummy," + Configuration.StartTime);

            for (int i = 0; i < Configuration.OneList.Count() - Configuration.SMA; i++)
            {
                double R = 0;
                double N = 0;
                for (int j = 0; j < Configuration.SMA; j++)
                {
                    R += Configuration.OneList[i + j];
                    N += Num[i + j];
                }
                R /= Configuration.SMA;
                N /= Configuration.SMA;

                Onesw.WriteLine(R + "," + N);
            }

            Onesw.Close();

            Configuration.ESW.Close();
            //Configuration.Problem.Close();


            System.IO.Directory.SetCurrentDirectory("../");
            StreamWriter swP = new StreamWriter("PPP.csv", true, System.Text.Encoding.GetEncoding("shift_jis"));

            swP.WriteLine(Configuration.NoiseWidth + "," + Configuration.ASName + "," + P.CList.Count());
            swP.Close();
        }

        private static int getIndexOfCriterion(Histogram hist)
        {
            var max = hist[0].Count;
            var array = new double[hist.BucketCount - 1];
            var key = new int[hist.BucketCount - 2];
            var result = new int[hist.BucketCount - 3];
            for (int i = 0; i < hist.BucketCount - 1; i++)
            {
                array[i] = hist[i + 1].Count - hist[i].Count;
            }
            for (int i = 0; i < hist.BucketCount - 2; i++)
            {
                if (array[i] * array[i + 1] <= 0)
                {
                    key[i] = 1;
                }
                else
                {
                    key[i] = 0;
                }
            }
            var counter = 0;

            for (int i = 2; i < hist.BucketCount - 2; i++)
            {

                if (key[i] == 1)
                {
                    result[counter] = i + 2;
                    counter++;
                }
            }

            return result[1];//2番目のtruning point を返す

        }



        /// <summary>
        /// 評価
        /// </summary>
        /// <param name="Env">環境</param>
        /// <param name="P">Population</param>
        /// <returns>performance</returns>
        /*public static double Exploit( Population P )
		{
			// situation取得
			State S = Configuration.ExploitEnv.GetState();
			// Matchset生成
			MatchSet M = new NotCoveringMatchSet( S, P );
			// PredictionArray生成
			PredictionArray PA = new GreedyPredictionArray( M );
			// Action決定
			char Action = PA.SelectAction();
			// 報酬獲得
			double Rho = Configuration.ExploitEnv.ActionExploit( Action );
			//if( Rho == 0 && Configuration.T > 35530 )
			//{
			//	Console.WriteLine( Configuration.T + " : " + S.state + " : " + Action + " : " + Rho );
			//	foreach(double d in PA.PA)
			//	{
			//		Console.WriteLine(d);
			//	}
			//	System.Threading.Thread.Sleep( 1000 );
			//}
			
			if(Action == '0')
			{
				Configuration.ZeroList.Add( Rho );
			}
			else if(Action == '1')
			{
				Configuration.OneList.Add( Rho );
			}

			return Rho;
		}*/
    }
}