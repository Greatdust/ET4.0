using System.Threading.Tasks;

namespace ETModel
{
    /// <summary>
    /// Actor消息处理接口（）
    /// </summary>
	public interface IActorInterceptTypeHandler
	{
		Task Handle(Session session, Entity entity, object actorMessage);
	}
}