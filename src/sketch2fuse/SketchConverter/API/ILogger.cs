namespace SketchConverter.API
{
	public interface ILogger
	{
		void Info(string info);
		void Warning(string warning);
		void Error(string error);
	}
}
