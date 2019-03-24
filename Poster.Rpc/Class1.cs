using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Poster.Rpc
{
    	public class RpcDecorator<T> : DispatchProxy
	{
		private static IRequestClient<IRpcRequest, IRpcResponse> client;
		private static readonly MethodInfo CastMethod = typeof(RpcDecorator<T>).GetMethod("Convert");
		private readonly MethodInvokeInfo methodInvokeInfo = new MethodInvokeInfo();

		public T Create(IRequestClient<IRpcRequest, IRpcResponse> requestClient)
		{
			client = requestClient;
			object proxy = Create<T, RpcDecorator<T>>();
			return (T) proxy;
		}

		public static async Task<T> Convert<T>(Task<object> task)
		{
			var result = await task;
			return (T) result;
		}

		protected override object Invoke(MethodInfo targetMethod, object[] args)
		{
			methodInvokeInfo.Name = targetMethod.Name;
			methodInvokeInfo.Args = args;
			if (targetMethod.IsGenericTask())
			{
				var type = targetMethod.ReturnType.GetGenericArguments()[0];
				var convertMethod = CastMethod.MakeGenericMethod(type);
				return convertMethod.Invoke(null, new object[]
				{
					Task.Run(() =>
						client.Request(new RpcRequest
						{
							Type = typeof(T),
							MethodInfo = methodInvokeInfo.SerializeObject()
						}).GetAwaiter().GetResult().DeserializeObject())
				});
			}

			if (targetMethod.IsTask())
			{
				return Task.Run(delegate
				{
					client.Request(new RpcRequest
					{
						Type = typeof(T),
						MethodInfo = methodInvokeInfo.SerializeObject()
					}).GetAwaiter().GetResult().DeserializeObject();
				});
			}

			if (targetMethod.IsVoid())
			{
				var sw = Stopwatch.StartNew();
			
				sw.Start();
				
				if (targetMethod.IsGenericMethod)
				{
					methodInvokeInfo.IsGeneric = true;
					methodInvokeInfo.GenericArguments = targetMethod.GetGenericArguments();
				}

				var request = new RpcRequest
				{
					Type = typeof(T),
					MethodInfo = methodInvokeInfo.SerializeObject()
				};
				
				sw.Stop();
		
				client.Request(request).GetAwaiter().GetResult().DeserializeObject();
			
				Console.WriteLine($" -> {sw.Elapsed.TotalMilliseconds}");
				
				return typeof(void);
			}

			if (targetMethod.IsGenericMethod)
			{
				methodInvokeInfo.IsGeneric = true;
				methodInvokeInfo.GenericArguments = targetMethod.GetGenericArguments();
			}

			var res = client.Request(new RpcRequest
			{
				Type = typeof(T),
				MethodInfo = methodInvokeInfo.SerializeObject()
			});

			res.Wait();
			
			var r = res.Result;
			var obj = r.DeserializeObject();
			return obj;
		}
	}
}