using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
    class OneBitEnvironment : Environment
    {
        // Multiplexer用AddressBit
        private int AddressBit;
        private double NoiseWidth;


        private int IndexCount = -1;    // 現在読んでいる点


        public override List<string> GetDataList()
        {
            List<string> dummy = new List<string>();
            return dummy;

        }
        // Environment作成
        public OneBitEnvironment()

        {
            this.Length = 1;
            this.Number = 4;    // 2進数 
            this.NoiseWidth = Configuration.NoiseWidth;
        }

        // Populationに(ランダムな)Stateを答えを算出してから渡す
        override public State GetState()
        {
            this.s = new FourState(this.Length);
            State state = this.s.GetState();
            return state;
        }

        // Multiplexer答え算出
        override protected char ActionCalculation(string S)
        {
            //dummy
            return '0';
        }

        // Actionに対するReward　ばらつき幅同じ
        public override double ExecuteAction(char act)
        {
            // シングルステップ問題
            this.Eop = true;
            if (this.s.state[0] == '0' | this.s.state[0] == '1')
            {
                return 0.0 + this.MakeNoise();
            }
            else
            {
                return Configuration.Rho + this.MakeNoise();
            }
        }

        // Actionの正解･不正解
        public override int ActionExploit(char act)
        {
            // シングルステップ問題
            this.Eop = true;
            //dummy
            return 0;
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