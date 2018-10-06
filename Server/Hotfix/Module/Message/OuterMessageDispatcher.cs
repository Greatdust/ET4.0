using System;
using ETModel;
using Google.Protobuf;

namespace ETHotfix
{
    /// <summary>
    /// 这个是对接客户端和MAP的消息分发
    /// </summary>
    public class OuterMessageDispatcher: IMessageDispatcher
	{
		public async void Dispatch(Session session, ushort opcode, object message)
		{
			try
			{
				switch (message)
				{
					case IFrameMessage iFrameMessage: // 如果是帧消息，构造成OneFrameMessage发给对应的unit
					{
						long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
						ActorLocationSender actorLocationSender = Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(unitId);

						// 这里设置了帧消息的id，防止客户端伪造
						iFrameMessage.Id = unitId;

						OneFrameMessage oneFrameMessage = new OneFrameMessage
						{
							Op = opcode, AMessage = ByteString.CopyFrom(session.Network.MessagePacker.SerializeTo(iFrameMessage))
						};
						actorLocationSender.Send(oneFrameMessage);
						return;
					}
					case IActorLocationRequest actorLocationRequest: // gate session收到actor rpc消息，先向actor 发送rpc请求，再将请求结果返回客户端
					{
						long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId; //角色单元的id
                            ActorLocationSender actorLocationSender = Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(unitId);

						int rpcId = actorLocationRequest.RpcId; // 这里要保存客户端的rpcId
						IResponse response = await actorLocationSender.Call(actorLocationRequest);
						response.RpcId = rpcId;

						session.Reply(response);
						return;
					}
					case IActorLocationMessage actorLocationMessage:  // gate session收到actor消息直接转发给actor自己去处理
                        {
						long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
						ActorLocationSender actorLocationSender = Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(unitId);
						actorLocationSender.Send(actorLocationMessage);
						return;
					}
				}

				Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, new MessageInfo(opcode, message));
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
