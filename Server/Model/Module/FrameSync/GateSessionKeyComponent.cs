using System.Collections.Generic;

namespace ETModel
{
    /// <summary>
    /// 密匙key 和账号的映射关系  登录账号 网关服务器和客户端的验证
    /// </summary>
    public class GateSessionKeyComponent : Component
	{
        /// <summary>
        /// 保存KEY 和玩家账号
        /// </summary>
        private readonly Dictionary<long, string> sessionKey = new Dictionary<long, string>();
        /// <summary>
        /// 添加KEY和玩家账号的映射关系到字典
        /// </summary>
        /// <param name="key"></param>
        /// <param name="account"></param>
        public void Add(long key, string account)
		{
			this.sessionKey.Add(key, account);
			this.TimeoutRemoveKey(key);
		}
        /// <summary>
        /// 利用KEY得到账号
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(long key)
		{
			string account = null;
			this.sessionKey.TryGetValue(key, out account);
			return account;
		}
        /// <summary>
        /// 移除一个KEY和账号的映射关系
        /// </summary>
        /// <param name="key"></param>
        public void Remove(long key)
		{
			this.sessionKey.Remove(key);
		}
        /// <summary>
        /// 20秒后移除这个KEY的映射
        /// </summary>
        /// <param name="key"></param>
        private async void TimeoutRemoveKey(long key)
		{
			await Game.Scene.GetComponent<TimerComponent>().WaitAsync(20000);
			this.sessionKey.Remove(key);
		}
	}
}
