using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	class NormalMatchSet : MatchSet
	{
		public NormalMatchSet( State S, Population P )
		{
			this.CList = new List<Classifier>();
			this.Covering( S, P );
		}

		// situationに合うものをPopulationから取り、足りないときはCovering
		protected override void Covering( State S, Population P )
		{
		    while (this.CList.Count == 0)
		    {
		        // situationにあうものをPopulationから探す
		        this.CList = P.MatchSituation(S);

		        if (CList.Count == 0)
		        {
                    // 一部を変化させたCondition
                    State state = new IntegralState(S);
                    state.Covering();
                    
                    Classifier CC;

                    if (Configuration.ASName == "CS" || Configuration.ASName == "MaxCS" || Configuration.ASName == "Max" || Configuration.ASName == "Updatee0CS")
                    {
                        CC = new SigmaNormalClassifier(state, Configuration.ExpThreshold);
                    }
                    else
                    {
                        CC = new NormalClassifier(state);
                    }
                    
                    if (CC.C.state.Substring(16,4).Equals("0***") & CC.C.state.Substring(28,4).Equals("*0**"))//"bath0 rehabi1"
                    {
                        Configuration.Problem.WriteLine(CC.C.state + "," + Configuration.T + "," + CC.P + "," + CC.M + "," + CC.Epsilon + "," + CC.F + "," +
                            CC.N + "," + CC.Exp + "," + CC.Ts + "," + CC.As + "," + CC.Kappa + "," + CC.Epsilon_0 + "," + CC.St + "," + CC.GenerateTime + ", covering");
                    }
                    P.Add(CC);
                    // 整理
                    P.Delete();
                    this.CList = new List<Classifier>();
                }
		    }


		}

		public override void Show()
		{
            StreamWriter sw = new StreamWriter("./MatchSet_" + Configuration.T + ".csv", true, System.Text.Encoding.GetEncoding("shift_jis"));
            
            if (Configuration.ASName != "CS" && Configuration.ASName != "MaxCS" && Configuration.ASName != "Max" && Configuration.ASName != "Updatee0CS")
            {
                sw.WriteLine("state,action,prediction,epsilon,fitness,numerosity,experience,timestamp,actionsetsize,accuracy,epsilon_0,selectTime,mean,std,generateTime,generality");
                foreach (Classifier C in this.CList)
                {
                    sw.WriteLine(C.C.state + ","/* + C.A + ","*/ + C.P + "," + C.Epsilon + "," + C.F + "," + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.M + "," + Math.Sqrt(C.S / (C.St - 1)) + "," + C.GenerateTime + "," + C.C.Generality);
                }
            }
            else
            {
                sw.WriteLine("time,state,action,prediction,epsilon,fitness,numerosity,experience,timestamp,actionsetsize,accuracy,epsilon_0,selectTime,mean,std,generateTime,generality,convergence");
                foreach (SigmaNormalClassifier C in this.CList)
                {
                    sw.WriteLine(Configuration.T + "," + C.C.state + "," /*+ C.A + "," */+ C.P + "," + C.Epsilon + "," + C.F + ","
                        + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.M + "," + Math.Sqrt(C.S / (C.St - 1)) + "," + C.GenerateTime + "," + C.C.Generality + "," + (C.IsConvergenceEpsilon() ? 1 : 0));
                }
            }
            sw.Close();
			foreach( Classifier C in this.CList )
			{
				//Console.WriteLine( C.C.state + ": " + C.A );
			}
		}

		// ActionSet用にActionに合うものを返す
		//public override List<Classifier> MatchAction( char Action )
		//{
			/*List<Classifier> Actionset = new List<Classifier>();

			foreach( Classifier C in this.CList )
			{
				if( C.A == Action )
				{
					Actionset.Add( C );
				}
			}

			return Actionset;*/
		//}
	}
}