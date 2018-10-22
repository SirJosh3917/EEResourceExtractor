using System;
using System.Reflection;

namespace SWFResourceExtractor
{
	public static class InjectionUtils
	{
		public static T CreateNew<T>(params object[] injectionVals)
			=> (T)CreateNew(typeof(T), injectionVals);

		public static object CreateNew(Type type, params object[] injectionVals)
		{
			var instance = Activator.CreateInstance(type);

			int valOn = 0;

			foreach (var i in type.GetFields(
				BindingFlags.Instance |
				BindingFlags.NonPublic))
			{
				if (i.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
				{
					i.SetValue(instance, injectionVals[valOn++]);
				}
			}

			return instance;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class InjectAttribute : Attribute
	{
		public InjectAttribute()
		{
		}
	}
}