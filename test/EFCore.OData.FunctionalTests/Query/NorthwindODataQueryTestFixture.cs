// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindODataQueryTestFixture : NorthwindQuerySqlServerFixture<NoopModelCustomizer>, IODataQueryTestFixture
    {
        private IHost _selfHostServer;

        protected override string StoreName { get; } = "ODataNorthwind";

        public NorthwindODataQueryTestFixture()
        {
            (BaseAddress, ClientFactory, _selfHostServer)
                = ODataQueryTestFixtureInitializer.Initialize<NorthwindODataContext>(
                    StoreName,
                    GetEdmModel(),
                    new List<IODataControllerActionConvention> { new OrderDetailsControllerActionConvention() });
        }

        private static IEdmModel GetEdmModel()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Customer>("Customers");
            modelBuilder.EntitySet<Order>("Orders");
            modelBuilder.EntityType<OrderDetail>().HasKey(e => new { e.OrderID, e.ProductID });
            modelBuilder.EntitySet<OrderDetail>("Order Details");

            return modelBuilder.GetEdmModel();
        }

        public string BaseAddress { get; private set; }

        public IHttpClientFactory ClientFactory { get; private set; }

        public override async Task DisposeAsync()
        {
            if (_selfHostServer != null)
            {
                await _selfHostServer.StopAsync();
                _selfHostServer.Dispose();
                _selfHostServer = null;
            }
        }
    }

    public class OrderDetailsControllerActionConvention : IODataControllerActionConvention
    {
        public int Order => 0;

        public bool AppliesToController(ODataControllerActionContext context)
            => context.Controller.ControllerName == "OrderDetails";

        public bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context.Action.ActionName != "Get")
            {
                return false;
            }

            return true;
        }
    }
}
