using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
    class ReadCsvEnvironment : Environment
    {
        // Multiplexer用AddressBit
        private int AddressBit;
        private double NoiseWidth;

        // データ
        private List<string> DataList = new List<string>();
        private List<double> RewardList = new List<double>();
        private List<char> ActionList = new List<char>();
        private int IndexCount = -1;    // 現在読んでいる点

        // Environment作成
        public ReadCsvEnvironment()
        {
            this.Length = 8;
            this.Number = 4;    // 進数

            // csv読み込み
            string file = "all_data.csv";
            StreamReader reader = new StreamReader(file, Encoding.GetEncoding("Shift_JIS"));
            while (reader.Peek() >= 0)
            {
                string[] cols = reader.ReadLine().Split(',');
                string data = "";

                int i = 0;
                for (; i < this.Length; i++)
                {
                    data += cols[i + 1];
                }

                DataList.Add(data);
                RewardList.Add(double.Parse(cols[i + 1]));
                ActionList.Add('0');
            }
        }

        // PopulationにStateを答えを算出してから渡
        override public State GetState()
        {
            // 読み込む場所決定
            IndexCount = (++IndexCount) % DataList.Count;
            // 読み込みデータにする
            this.s = new IntegralState(this.DataList[IndexCount]);
            State state = this.s.GetState();    

            this.Action = ActionCalculation(IndexCount);  // 仮



            //if (Configuration.IsConvergenceVT == true) {
            //    //弾く　＋ーepsilonの入力
            //    foreach(StdList sl in Configuration.Stdlist) {
            //        if (state.state == sl.C) {
            //            //チェックする、＋ー２σ以内ですか, 外れ値だったら、弾く
            //            if (sl.IsOutlier(RewardList[IndexCount])) {
            //                return GetState();
            //            }
            //        }
            //    }
            //}

            return state;
        }


        // 答え(仮)算出 ここではアクションがないので、便利上全部０にする
        override protected char ActionCalculation(int index)
        {

            return '0';
        }

        // Actionに対するReward ばらつき幅同じ
        public override double ExecuteAction(char act)
        {
            
            this.Eop = true;
            if (this.Action == act)
            {
                return RewardList[this.IndexCount];
            }
            else
            {
                return 0.0;
            }
            
        }
        
        // Actionの正解･不正解
        public override int ActionExploit(char act)
        {
            // シングルステップ問題
            this.Eop = true;
            if (this.Action == act)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public override List<string> GetDataList()
        {
            return DataList;
        }
        public override List<char> GetActionList()
        {
            return ActionList;
        }
        /// <summary>
        /// ガウシアンノイズ生成
        /// </summary>
        /// <returns>ガウシアンノイズ</returns>
        protected double MakeNoise()
        {
            double A = Configuration.MT_P.NextDoublePositive();
            double B = Configuration.MT_P.NextDoublePositive();

            return this.NoiseWidth * (Math.Sqrt(-2 * Math.Log(A, Math.E)) * Math.Sin(2 * Math.PI * B));
        }
    }
}