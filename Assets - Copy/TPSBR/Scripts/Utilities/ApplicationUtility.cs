namespace TPSBR
{
	using System;
	using System.IO;
	using UnityEngine;

	public static partial class ApplicationUtility
	{
		// PUBLIC MEMBERS

		public static readonly string DataPath;

		// CONSTRUCTORS

		static ApplicationUtility()
		{
			if (GetCommandLineArgument("-dataPath", out string dataPath) == true)
			{
				DataPath = dataPath;
			}
			else
			{
				if (Application.isEditor == true || Application.isMobilePlatform == false)
				{
					DataPath = Application.dataPath + "/..";
				}
				else
				{
					DataPath = Application.persistentDataPath;
				}
			}
		}

		// PUBLIC METHODS

		public static string GetFilePath(string fileName)
		{
			return Path.Combine(DataPath, fileName);
		}

		public static bool HasCommandLineArgument(string name)
		{
			string[] arguments = Environment.GetCommandLineArgs();
			for (int i = 0; i < arguments.Length; ++i)
			{
				if (arguments[i] == name)
					return true;
			}

			return false;
		}

		public static bool GetCommandLineArgument(string name, out string argument)
		{
			string[] arguments = Environment.GetCommandLineArgs();
			for (int i = 0; i < arguments.Length; ++i)
			{
				if (arguments[i] == name && arguments.Length > (i + 1))
				{
					argument = arguments[i + 1];
					return true;
				}
			}

			argument = default;
			return false;
		}

		public static bool GetCommandLineArgument(string name, out int argument)
		{
			string[] arguments = Environment.GetCommandLineArgs();
			for (int i = 0; i < arguments.Length; ++i)
			{
				if (arguments[i] == name && arguments.Length > (i + 1) && int.TryParse(arguments[i + 1], out int parsedArgument) == true)
				{
					argument = parsedArgument;
					return true;
				}
			}

			argument = default;
			return false;
		}
	}
}
