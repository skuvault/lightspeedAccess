﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightspeedAccess;
using LightspeedAccess.Models.Configuration;
using LightspeedAccess.Models.Order;
using LightspeedAccess.Models.Product;
using LightspeedAccess.Models.Request;
using LightspeedAccess.Services;
using NUnit.Framework;

namespace lightspeedAccessTests.Orders
{
	internal class OrderTests
	{
		private LightspeedFactory _factory;
		private LightspeedConfig _config;

		private static LightspeedConfig GetConfig()
		{
			try
			{
				using( StreamReader sr = new StreamReader( @"D:\lightspeedCredentials.txt" ) )
				{
					var accountId = sr.ReadLine();
					var token = sr.ReadLine();
					return new LightspeedConfig( Int32.Parse( accountId ), token );
				}
			}
			catch( Exception e )
			{
				return new LightspeedConfig();
			}
		}

		[ SetUp ]
		public void Init()
		{
			this._factory = new LightspeedFactory( "", "", "" );
			this._config = GetConfig();
		}

		[ Test ]
		public void OrderServiceTest()
		{
			var service = this._factory.CreateOrdersService( this._config );
			var endDate = DateTime.Now;
			var startDate = endDate.Subtract( new TimeSpan( 10000, 0, 0, 0 ) );

			var ordersTask = service.GetOrdersAsync( startDate, endDate, CancellationToken.None );

			Task.WaitAll( ordersTask );
			Assert.Greater( ordersTask.Result.Count(), 0 );
		}

		[ Test ]
		public void OrderServiceTestAsync()
		{
			var service = _factory.CreateOrdersService( _config );

			var endDate = DateTime.Now;
			var startDate = endDate.AddMonths( -6 );

			var cSource = new CancellationTokenSource();
			var orders = service.GetOrdersAsync( startDate, endDate, cSource.Token );

			orders.Wait();
			Assert.Greater( orders.Result.Count(), 0 );
		}

		[ Test ]
		public void SingleServiceThrottlerTestAsync()
		{
			var service = _factory.CreateOrdersService( _config );
			var endDate = DateTime.Now;
			var startDate = endDate.AddMonths( -6 );

			var cSource = new CancellationTokenSource();

			for( int i = 0; i < 200; i++ )
			{
				var ordersTask = service.GetOrdersAsync( startDate, endDate, cSource.Token );
				ordersTask.Wait( cSource.Token );
			}

			Assert.Greater( 5, 0 );
		}

		[ Test ]
		public void MultipleServicesThrottlerTestAsync()
		{
			var service = _factory.CreateOrdersService( _config );
			var invService = _factory.CreateShopsService( _config );
			var endDate = DateTime.Now;
			var startDate = endDate.AddMonths( -6 );

			var cSource = new CancellationTokenSource();
			var itemsTask = invService.GetItems( 1, cSource.Token );
			itemsTask.Wait( cSource.Token );
			var item = itemsTask.Result.First();

			var tasks = new List< Task >();
			for( int i = 0; i < 100; i++ )
			{
				var ordersTask = service.GetOrdersAsync( startDate, endDate, cSource.Token );
				var itemUpdateTask = invService.UpdateOnHandQuantityAsync( item.ItemId, item.Sku, item.ItemShops[ 0 ].ShopId, "Shop" + item.ItemShops[ 0 ].ItemShopId, item.ItemShops[ 0 ].ItemShopId, 10, cSource.Token );

				tasks.Add( ordersTask );
				tasks.Add( itemUpdateTask );
			}

			Task.WaitAll( tasks.ToArray() );

			Assert.Greater( 5, 0 );
		}

		[ Test ]
		public void SmokeTest()
		{
			var service = _factory.CreateLightspeedAuthService();
			var token = service.GetAuthToken( "" );
			Console.WriteLine( "YOUR TOKEN IS: " + token );
			Assert.Greater( 1, 0 );
		}
	}
}