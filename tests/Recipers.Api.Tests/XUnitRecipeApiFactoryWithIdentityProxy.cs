using Recipers.Api.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Recipers.Api.Tests;

// Adding the IAsyncLifetime interface to manage async setup and teardown
public class XUnitRecipeApiFactoryWithIdentityProxy : RecipeApiFactoryWithIdentityProxy, IAsyncLifetime
{

}
