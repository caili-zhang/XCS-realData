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
		    //while (this.CList.Count == 0)
		    //{
		    //    // situationにあうものをPopulationから探す
		    //    this.CList = P.MatchSituation(S);

		    //    if (CList.Count == 0)
		    //    {
      //              // 一部を変化させたCondition
      //              State state = new IntegralState(S);
      //              state.Covering();
		            
      //              Classifier CC;

      //              if (Configuration.ASName == "CS" || Configuration.ASName == "MaxCS" || Configuration.ASName == "Max" || Configuration.ASName == "Updatee0CS")
      //              {
      //                  CC = new SigmaNormalClassifier(state, Configuration.ExpThreshold);
      //              }
      //              else
      //              {
      //                  CC = new NormalClassifier(state);
      //              }

      //              P.Add(CC);
      //              // 整理
      //              P.Delete();
      //              this.CList = new List<Classifier>();
      //          }
		    //}

		    while( this.CList.Count == 0 )
			{
				// situationにあうものをPopulationから探す
				this.CList = P.MatchSituation( S );

				// Actionの種類
				List<char> Actions = new List<char>();
				int NumberOfActions = 0;

				// Multiplexer(2進数)
				if( Configuration.Type == "Binary" )
				{
					Actions.Add( '0' );
					Actions.Add( '1' );
					foreach( Classifier C in this.CList )
					{
						Actions.Remove( C.A );
					}
					NumberOfActions = 2 - Actions.Count;
				}
				
				// MatchSetにある行動が少ないとき
				if( NumberOfActions < Configuration.Theta_mna )
				{
					// 一部を変化させたCondition
					State state = new BinaryState( S );
					state.Covering();

					Classifier CC;
					if( Configuration.ASName == "CS" || Configuration.ASName == "MaxCS" || Configuration.ASName == "Max" || Configuration.ASName == "Updatee0CS" )
					{
						CC = new SigmaNormalClassifier( state, Actions, Configuration.ExpThreshold );
					}
					else
					{
						CC = new NormalClassifier( state, Actions );
					}
					P.Add( CC );
					// 整理
					P.Delete();
					this.CList = new List<Classifier>();
				}
			}
		}

		public override void Show()
		{
            StreamWriter sw = new StreamWriter("./MatchSet_" + Configuration.T /*+ "_" + Configuration.Seed + "CnoiseWidth" + Configuration.NoiseWidth
				+ "AS_" + Configuration.ASName + "ET_" + Configuration.ExpThreshold + "DS_" + Configuration.DifferenceSigma + "LS_" + Configuration.LookBackSigma
				+ "DE_" + Configuration.DifferenceEpsilon + "LE_" + Configuration.LookBackEpsilon */+ ".csv", true, System.Text.Encoding.GetEncoding("shift_jis"));
            //StreamWriter sw = new StreamWriter( "./Population_" + Configuration.T + "_" + Configuration.Seed + "CnoiseWidth" + Configuration.NoiseWidth
            //	+ "AS_" + "CS" + "ET_" + Configuration.ExpThreshold + "DS_" + Configuration.DifferenceSigma + "LS_" + Configuration.LookBackSigma
            //	+ "DE_" + Configuration.DifferenceEpsilon + "LE_" + Configuration.LookBackEpsilon + ".csv", true, System.Text.Encoding.GetEncoding( "shift_jis" ) );
            if (Configuration.ASName != "CS" && Configuration.ASName != "MaxCS" && Configuration.ASName != "Max" && Configuration.ASName != "Updatee0CS")
            {
                sw.WriteLine("state,action,prediction,epsilon,fitness,numerosity,experience,timestamp,actionsetsize,accuracy,epsilon_0,selectTime,mean,std,generateTime,generality");
                foreach (Classifier C in this.CList)
                {
                    //Console.WriteLine( "state: " + C.C.state + " action: " + C.A + " Prediction: " + C.P + " Epsilon: " + C.Epsilon + " Fitness" + C.F + " Numerosity: " + C.N + " Experience: " + C.Exp + " TimeStamp: " + C.Ts + " ASsize: " + C.As + " Accuracy: " + C.Kappa + "Epsilon_0: " + C.Epsilon_0 );
                    //Console.WriteLine();

                    sw.WriteLine(C.C.state + ","/* + C.A + ","*/ + C.P + "," + C.Epsilon + "," + C.F + "," + C.N + "," + C.Exp + "," + C.Ts + "," + C.As + "," + C.Kappa + "," + C.Epsilon_0 + "," + C.St + "," + C.M + "," + Math.Sqrt(C.S / (C.St - 1)) + "," + C.GenerateTime + "," + C.C.Generality);
                }
            }
            else
            {
                sw.WriteLine("time,state,action,prediction,epsilon,fitness,numerosity,experience,timestamp,actionsetsize,accuracy,epsilon_0,selectTime,mean,std,generateTime,generality,convergence");
                foreach (SigmaNormalClassifier C in this.CList)
                {
                    //Console.WriteLine( "state: " + C.C.state + " action: " + C.A + " Prediction: " + C.P + " Epsilon: " + C.Epsilon + " Fitness" + C.F + " Numerosity: " + C.N + " Experience: " + C.Exp + " TimeStamp: " + C.Ts + " ASsize: " + C.As + " Accuracy: " + C.Kappa + "Epsilon_0: " + C.Epsilon_0 );
                    //Console.WriteLine();

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
        public override List<Classifier> MatchAction(char Action)
        {
            List<Classifier> Actionset = new List<Classifier>();

			foreach( Classifier C in this.CList )
			{
				if( C.A == Action )
				{
					Actionset.Add( C );
				}
			}

			return Actionset;
        }
    }
}