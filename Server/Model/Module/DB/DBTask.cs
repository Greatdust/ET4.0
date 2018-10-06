using System.Threading.Tasks;

namespace ETModel
{
    /// <summary>
    /// 任务基类  有个需要重写的接口Run();
    /// </summary>
    public abstract class DBTask : ComponentWithId
	{
		public abstract Task Run();
	}
}