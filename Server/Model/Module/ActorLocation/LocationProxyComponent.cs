using System.Net;
using System.Threading.Tasks;

namespace ETModel
{
    /// <summary>
    /// 存放供别人识别服务器地址
    /// </summary>
    public class LocationProxyComponent : Component
	{
        /// <summary>
        /// 保存位置服务器端口地址
        /// </summary>
        public IPEndPoint LocationAddress;
	}
}