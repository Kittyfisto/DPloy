using System.IO;

namespace DPloy.Core
{
	sealed class ApplicationConstants
		: IApplicationConstants
	{
		public ApplicationConstants(string appDataLocalFolder, string title)
		{
			Title = title;
			AppDataLocalFolder = Path.Combine(appDataLocalFolder, title);
			LogFile = Path.Combine(AppDataLocalFolder, $"{title}.log");
		}

		public string Title { get; set; }
		public string AppDataLocalFolder { get; set; }
		public string LogFile { get; set; }
	}
}