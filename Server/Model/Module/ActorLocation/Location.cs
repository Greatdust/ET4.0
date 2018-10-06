namespace ETModel
{
    /// <summary>
    /// 位置
    /// </summary>
    public class Location : Entity
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string Address;

        public Location(long id, string address) : base(id)
        {
            this.Address = address;
        }
    }
}
