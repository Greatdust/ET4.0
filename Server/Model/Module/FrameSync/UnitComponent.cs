using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
    /// <summary>
    /// 角色单元管理组件
    /// </summary>
    public class UnitComponent: Component
	{
        /// <summary>
        /// 角色单元字典
        /// </summary>
        private readonly Dictionary<long, Unit> idUnits = new Dictionary<long, Unit>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();

			foreach (Unit unit in this.idUnits.Values)
			{
				unit.Dispose();
			}
			this.idUnits.Clear();
		}
        /// <summary>
        /// 添加角色单元到字典中
        /// </summary>
        /// <param name="unit"></param>
        public void Add(Unit unit)
		{
			this.idUnits.Add(unit.Id, unit);
		}
        /// <summary>
        /// 取得角色单元
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Unit Get(long id)
		{
			this.idUnits.TryGetValue(id, out Unit unit);
			return unit;
        }
        /// <summary>
        /// 移除角色单元
        /// </summary>
        /// <param name="id"></param>
		public void Remove(long id)
		{
			Unit unit;
			this.idUnits.TryGetValue(id, out unit);
			this.idUnits.Remove(id);
			unit?.Dispose();
		}
        /// <summary>
        /// 移除角色单元  但不回收
        /// </summary>
        /// <param name="id"></param>
        public void RemoveNoDispose(long id)
		{
			this.idUnits.Remove(id);
		}
        /// <summary>
        /// 角色单元数量
        /// </summary>
        public int Count
		{
			get
			{
				return this.idUnits.Count;
			}
		}
        /// <summary>
        /// 获取所有角色单元
        /// </summary>
        /// <returns></returns>
        public Unit[] GetAll()
		{
			return this.idUnits.Values.ToArray();
		}
	}
}