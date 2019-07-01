using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace DPloy.Distributor
{
	internal sealed class MethodSignature
	{
		public Type ReturnType;
		public Type[] ParameterTypes;

		[Pure]
		public bool IsCompatibleTo(MethodInfo method)
		{
			if (method.ReturnType != ReturnType)
				return false;

			var parameters = method.GetParameters();
			if (parameters.Length != ParameterTypes.Length)
				return false;

			for (int i = 0; i < parameters.Length; ++i)
			{
				if (parameters[i].ParameterType != ParameterTypes[i])
					return false;

				if (parameters[i].IsOut)
					return false;

				if (parameters[i].IsRetval)
					return false;
			}

			return true;
		}

		[Pure]
		public MethodSignature Clone()
		{
			return new MethodSignature
			{
				ReturnType = ReturnType,
				ParameterTypes = ParameterTypes.ToArray()
			};
		}

		[Pure]
		public MethodSignature WithReturnType(Type newReturnType)
		{
			var clone = Clone();
			clone.ReturnType = newReturnType;
			return clone;
		}
	}
}