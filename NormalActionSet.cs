using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	// (従来手法)
	class NormalActionSet : ActionSet
	{
		public NormalActionSet( MatchSet M,char act )
		{
            //this.CList = M.MatchAction(act);
            this.CList = M.CList;
		}

		public override void Show()
		{

			foreach( Classifier C in this.CList )
			{
				//Console.WriteLine( C.C.state + ": " + C.A );
			}
		}

		// Sigmaは飾り
		public override void Update( Population Pop, double P, StdList Sigma )
		{
			double SumNumerosity = 0;
			foreach( Classifier C in this.CList )
			{
				SumNumerosity += C.N;
			}

			foreach( Classifier C in this.CList )
			{
				C.Exp++;

				if( C.Exp < 1 / Configuration.Beta )
				{
					C.P += ( P - C.P ) / C.Exp;
				}
				else
				{
					C.P += Configuration.Beta * ( P - C.P );
				}

				if( C.Exp < Configuration.Beta )
				{
					C.Epsilon += ( Math.Abs( P - C.P ) - C.Epsilon ) / C.Exp;
				}
				else
				{
					C.Epsilon += Configuration.Beta * ( Math.Abs( P - C.P ) - C.Epsilon );
				}

				if( C.Exp < Configuration.Beta )
				{
					C.As += ( SumNumerosity - C.As ) / C.Exp;
				}
				else
				{
					C.As += Configuration.Beta * ( SumNumerosity - C.As );
				}

				
				// 標準偏差計算
				C.St++;
				double X = P - C.M;
				C.M += X / C.St;
				C.S += ( C.St - 1 ) * X * X / C.St;
			}

			this.UpdateFitness();

			if( Configuration.DoActionSetSubsumption )
			{
				this.Subsumption( Pop );
			}
		}

		protected override void UpdateFitness()
		{
			double AccuracySum = 0;

			foreach( Classifier C in this.CList )
			{
				if( C.Epsilon < C.Epsilon_0 )
				{
					C.Kappa = 1;
				}
				else
				{
					C.Kappa = Configuration.Alpha * Math.Pow( C.Epsilon / C.Epsilon_0, -Configuration.Nyu );
				}
				AccuracySum += C.Kappa * C.N;
			}

			foreach( Classifier C in this.CList )
			{
				C.F += Configuration.Beta * ( C.Kappa * C.N / AccuracySum - C.F );
			}
		}

		protected override void Subsumption( Population Pop )
		{
			Classifier Cl = null;
             
			foreach( Classifier C in this.CList )
			{
				if( C.CouldSubsume() )
				{
					if( ( Cl == null ) || ( C.C.NumberOfSharp > Cl.C.NumberOfSharp ) || ( ( C.C.NumberOfSharp == Cl.C.NumberOfSharp ) && ( Configuration.MT.NextDouble() < 0.5 ) ) )
					{
						Cl = C;
					}
				}
			}

			if( Cl != null )
			{
				// 削除中にforeachできない
				List<Classifier> CL = new List<Classifier>();

				foreach( Classifier C in this.CList )
				{
					if( Cl.IsMoreGeneral( C ) )
					{
						Cl.N += C.N;
						// 削除Classifier登録
						CL.Add( C );
					}
				}

				foreach( Classifier C in CL )
				{
                   
					this.Remove( C );
					Pop.Remove( C );
				}
			}
		}

		public override void Remove( Classifier C )
		{
			this.CList.Remove( C );
		}

		public override void RunGA( State Situation, Population P )
		{
			double NumerositySum = 0.0;
			double TimeStampSum = 0.0;

			foreach( Classifier C in this.CList )
			{
				NumerositySum += C.N;
				TimeStampSum += C.Ts * C.N;
			}

			if( Configuration.T - TimeStampSum / NumerositySum > Configuration.Theta_GA )
			{
				foreach( Classifier C in this.CList )
				{
					C.Ts = Configuration.T;
				}

				Classifier Parent_1 = this.SelectOffspring();
				Classifier Parent_2 = this.SelectOffspring();
				Classifier Child_1 = new NormalClassifier( ( NormalClassifier )Parent_1 );
				Classifier Child_2 = new NormalClassifier( ( NormalClassifier )Parent_2 );
				Child_1.N = Child_2.N = 1;
				Child_1.Exp = Child_2.Exp = 0;
				Child_1.St = Child_2.St = 0;
				Child_1.M = Child_2.M = 0;
				Child_1.S = Child_2.S = 0;

				if( Configuration.MT.NextDouble() < Configuration.Chai )
				{
					Child_1.Crossover( Child_2 );
					Child_1.P = ( Parent_1.P + Parent_2.P ) / 2;
					Child_1.Epsilon = ( Parent_1.Epsilon + Parent_2.Epsilon ) / 2;
					Child_1.F = ( Parent_1.F + Parent_2.F ) / 2;
					//Child_1.M = ( Parent_1.M + Parent_2.M ) / 2;
					//Child_1.S = ( Parent_1.S + Parent_2.S ) / 2;
					Child_2.P = Child_1.P;
					Child_2.Epsilon = Child_1.Epsilon;
					Child_2.F = Child_1.F;
					//Child_2.M = Child_1.M;
					//Child_2.S = Child_1.S;
				}

				Child_1.F *= 0.1;
				Child_2.F *= 0.1;

				// bothChild
				Child_1.Mutation( Situation );
				Child_2.Mutation( Situation );

				if( Configuration.DoGASubsumption )
				{
					if( Parent_1.DoesSubsume( Child_1 ) )
					{
						Parent_1.N++;
					}
					else if( Parent_2.DoesSubsume( Child_1 ) )
					{
						Parent_2.N++;
					}
					else
					{
						P.Insert( Child_1 );
					}
					P.Delete();

					if( Parent_1.DoesSubsume( Child_2 ) )
					{
						Parent_1.N++;
					}
					else if( Parent_2.DoesSubsume( Child_2 ) )
					{
						Parent_2.N++;
					}
					else
					{
						P.Insert( Child_2 );
					}
					P.Delete();
				}
				else
				{
					P.Insert( Child_1 );
					P.Delete();
					P.Insert( Child_2 );
					P.Delete();
				}

			}
		}

		protected override Classifier SelectOffspring()
		{
			double FitnessSum = 0;

			foreach( Classifier C in this.CList )
			{
				FitnessSum += C.F;
			}

			double ChoicePoint = Configuration.MT.NextDouble() * FitnessSum;
			FitnessSum = 0;

			foreach( Classifier C in this.CList )
			{
				FitnessSum += C.F;
				if( FitnessSum > ChoicePoint )
				{
					return C;
				}
			}

			return null;
		}
	}
}