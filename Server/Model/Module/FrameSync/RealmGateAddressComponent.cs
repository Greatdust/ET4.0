using System.Collections.Generic;

namespace ETModel
{
	public class RealmGateAddressComponent : Component
	{
		public readonly List<StartConfig> GateAddress = new List<StartConfig>();
        /// <summary>
        /// 随机获得一个网关服务器地址
        /// </summary>
        /// <returns></returns>
        public StartConfig GetAddress()
		{
			int n = RandomHelper.RandomNumber(0, this.GateAddress.Count);
			return this.GateAddress[n];
		}
	}
}
