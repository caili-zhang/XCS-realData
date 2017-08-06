using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCS
{
	class NotCoveringMatchSet : MatchSet
	{
		public NotCoveringMatchSet( State S, Population P )
		{
			this.CList = new List<Classifier>();
			this.CList = P.MatchSituation( S );
		}

		// situationに合うものをPopulationから取り、足りないときはCovering
		protected override void Covering( State S, Population P )
		{

		}

		public override void Show()
		{
			foreach( Classifier C in this.CList )
			{
				Console.WriteLine( C.C.state + ": "/* + C.A*/ );
			}
		}

		// ActionSet用にActionに合うものを返す
		public override List<Classifier> MatchAction( char Action )
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