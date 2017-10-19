using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
    class IntegralState : State
    {
        // 長さと種類をそろえる(GetStateと統合可?)
        public IntegralState(int Length, int Base)
        {
            this.Length = Length;
            Number = Base;
        }

        // すべてをリストアップ用
        public IntegralState(string S)
        {
            //８＊４のビット列にする
            string state = "";
            for (int i = 0; i < S.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (j == Convert.ToInt32(S.Substring(i, 1)))
                    {
                        state = state + "0";
                    }
                    else
                    {
                        state = state + "*";
                    }
                }

            }
            this.state = state;
            this.Length = state.Length;
            this.NumberOfSharp = 0;
        }

        // コピーコンストラクタ
        public IntegralState(State S)
        {
            this.state = S.state;
            this.Length = S.Length;
            this.NumberOfSharp = S.NumberOfSharp;
        }

        // Environment経由で0, 1で構成される長さLengthのStateを渡す(ランダム)
        override public State GetState()
        {
            this.CountSharp();

            return this;
        }

        public override void Covering()
        {
            string S = this.state;//長さはstate＝＞８＊４
            string CoveredState = "";//長さは　８＊４

            for (int i = 0; i < S.Length; i++)
            {
                if (S[i] == '*')
                {//covering
                    if (Configuration.MT.NextDouble() < Configuration.P_sharp)
                    {
                        CoveredState += "0";
                    }
                    else
                    {
                        CoveredState += "*";
                    }
                }
                else
                {
                    CoveredState += "0";
                }
            }
            this.state = CoveredState;
            this.CountSharp();
        }


        // i番目を入れ替え
        public override void Switch(State S, int i)
        {
            string t = "";
            string s = "";

            for (int j = 0; j < this.Length; j++)
            {
                if (i == j)
                {
                    t += S.state[j];
                    s += this.state[j];
                }
                else
                {
                    t += this.state[j];
                    s += S.state[j];
                }
            }

            this.state = t;
            S.state = s;

            this.CountSharp();
            S.CountSharp();
        }

        // 表示
        public override void Show()
        {
            Console.WriteLine(this.state);
        }

        /// <summary>
        /// すべての取り得るStateを生成
        /// </summary>
        /// <param name="Length">Multiplexerの長さ</param>
        /// <returns>すべての取り得るState</returns>
        public static State[] AllState(int Length)
        {
            State[] All = new State[(int)Math.Pow(Number, Length)];
            for (int i = 0; i < All.Length; i++)
            {
                string S = Convert.ToString(i, Number);
                // 頭に足りない分0を付加
                while (S.Length < Length)
                {
                    S = "0" + S;
                }
                All[i] = new IntegralState(S);
            }
            return All;
        }
    }
}