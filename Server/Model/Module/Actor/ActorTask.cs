using System.Threading.Tasks;

namespace ETModel
{
	public struct ActorTask
	{
        /// <summary>
        /// Actor消息
        /// </summary>
        public IActorRequest ActorRequest;

        /// <summary>
        /// TaskCompletionSource  Tcs
        /// </summary>
        public TaskCompletionSource<IActorLocationResponse> Tcs;

		public ActorTask(IActorLocationMessage actorRequest)
		{
			this.ActorRequest = actorRequest;
			this.Tcs = null;
		}
		
		public ActorTask(IActorLocationRequest actorRequest, TaskCompletionSource<IActorLocationResponse> tcs)
		{
			this.ActorRequest = actorRequest;
			this.Tcs = tcs;
		}
	}
}