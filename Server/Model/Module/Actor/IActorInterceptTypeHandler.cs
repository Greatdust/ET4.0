using System.Threading.Tasks;

namespace ETModel
{
    /// <summary>
    /// Actor��Ϣ����ӿڣ���
    /// </summary>
	public interface IActorInterceptTypeHandler
	{
		Task Handle(Session session, Entity entity, object actorMessage);
	}
}