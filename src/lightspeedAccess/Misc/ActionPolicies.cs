﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netco.ActionPolicyServices;
using Netco.Utils;

namespace lightspeedAccess.Misc
{
	internal class ActionPolicies
	{
		public static ActionPolicy Submit
		{
			get { return _lightspeedRetryPolicy; }
		}

		private static readonly int RetryIntervalSeconds = 45; 

		private static readonly ActionPolicy _lightspeedRetryPolicy = ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
		{
			LightspeedLogger.Log.Trace( ex, "Retrying Lightspeed API submit call for the {0} time", i );
			SystemUtil.Sleep( TimeSpan.FromSeconds( RetryIntervalSeconds ) );
		} );

		public static ActionPolicyAsync SubmitAsync
		{
			get { return _lightspeedSumbitAsyncPolicy; }
		}

		private static readonly ActionPolicyAsync _lightspeedSumbitAsyncPolicy =
			ActionPolicyAsync.Handle< Exception >().RetryAsync( 10, async ( ex, i ) =>
			{
				LightspeedLogger.Log.Trace( ex, "Retrying Lightspeed API submit call for the {0} time", i );
				await Task.Delay( TimeSpan.FromSeconds( RetryIntervalSeconds ) );
			} );
	}
}