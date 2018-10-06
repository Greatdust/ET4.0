using System;
using System.Net;
using ETModel;
using MongoDB.Bson;

namespace ETHotfix
{
	[MessageHandler(AppType.Realm)]
	public class C2R_LoginHandler : AMRpcHandler<C2R_Login, R2C_Login>
    { 
        //登录  是普通的RPC消息
        protected override async void Run(Session session, C2R_Login message, Action<R2C_Login> reply)
		{
			R2C_Login response = new R2C_Login();
			try
			{
				//if (message.Account != "abcdef" || message.Password != "111111")
				//{
				//	response.Error = ErrorCode.ERR_AccountOrPasswordError;
				//	reply(response);
				//	return;
				//}

				// 随机分配一个Gate
				StartConfig config = Game.Scene.GetComponent<RealmGateAddressComponent>().GetAddress();
				//Log.Debug($"gate address: {MongoHelper.ToJson(config)}");
				IPEndPoint innerAddress = config.GetComponent<InnerConfig>().IPEndPoint;
				Session gateSession = Game.Scene.GetComponent<NetInnerComponent>().Get(innerAddress);

				// 向gate请求一个key,客户端可以拿着这个key连接gate
				G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey)await gateSession.Call(new R2G_GetLoginKey() {Account = message.Account});

				string outerAddress = config.GetComponent<OuterConfig>().Address2;

				response.Address = outerAddress; //gate服务器地址
                response.Key = g2RGetLoginKey.Key;  //gate服务器返回的KEY
                reply(response); //返回给客户端
            }
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}