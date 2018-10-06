using System.Numerics;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 用户实体类型  主角 NPC
    /// </summary>
    public enum UnitType
	{
		Hero,
		Npc
	}

	[ObjectSystem]
	public class UnitSystem : AwakeSystem<Unit, UnitType>
	{
		public override void Awake(Unit self, UnitType a)
		{
			self.Awake(a);
		}
	}

    /// <summary>
    /// 角色单元
    /// </summary>
    public sealed class Unit: Entity
	{
        /// <summary>
        /// 角色单元类型 主角 或 NPC
        /// </summary>
        public UnitType UnitType { get; private set; }


        /// <summary>
        /// 位置
        /// </summary>
        [BsonIgnore]
		public Vector3 Position { get; set; }
		
		public void Awake(UnitType unitType)
		{
			this.UnitType = unitType;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}