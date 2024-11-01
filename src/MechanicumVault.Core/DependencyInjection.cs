// using Microsoft.Extensions.DependencyInjection;
//
// namespace MechanicumVault.Core;
//
// /// <summary>
// /// Core DependencyInjection handler to share services between components.
// /// </summary>
// public static class DependencyInjection
// {
// 	private static ServiceProvider? ServiceProvider;
// 	
// 	public static ServiceProvider GetServiceProvider()
// 	{
// 		if (ServiceProvider != null) // Reuse ServiceProvider between components if already created
// 		{
// 			return ServiceProvider;
// 		}
// 		
// 		var srv = new ServiceCollection();
// 		
// 		ServiceProvider = srv.BuildServiceProvider();
// 		return ServiceProvider;
// 	}
// }