using System;
using System.Collections.Generic;

namespace ETModel
{
	[ObjectSystem]
	public class OpcodeTypeComponentSystem : AwakeSystem<OpcodeTypeComponent>
	{
		public override void Awake(OpcodeTypeComponent self)
		{
			self.Load();
		}
	}
	
	[ObjectSystem]
	public class OpcodeTypeComponentLoadSystem : LoadSystem<OpcodeTypeComponent>
	{
		public override void Load(OpcodeTypeComponent self)
		{
			self.Load();
		}
	}
    /// <summary>
    /// 操作码和类的映射关系  (类是传递的数据的类型 就是PB转换的需要传递的类型)
    /// </summary>
    public class OpcodeTypeComponent : Component
	{
        //字典  存储操作码  和类型
        private readonly DoubleMap<ushort, Type> opcodeTypes = new DoubleMap<ushort, Type>();
		
		private readonly Dictionary<ushort, object> typeMessages = new Dictionary<ushort, object>();

		public void Load()
		{
			this.opcodeTypes.Clear();
			this.typeMessages.Clear();
			
			List<Type> types = Game.EventSystem.GetTypes(typeof(MessageAttribute));
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MessageAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				
				MessageAttribute messageAttribute = attrs[0] as MessageAttribute;
				if (messageAttribute == null)
				{
					continue;
				}

				this.opcodeTypes.Add(messageAttribute.Opcode, type);
				this.typeMessages.Add(messageAttribute.Opcode, Activator.CreateInstance(type));
			}
		}

        /// <summary>
        /// 根据类型获得操作码
        /// </summary>
        /// <returns>The opcode.</returns>
        /// <param name="type">Type.</param>
        public ushort GetOpcode(Type type)
		{
			return this.opcodeTypes.GetKeyByValue(type);
		}

        /// <summary>
        /// 根据操作码获得类型
        /// </summary>
        /// <returns>The type.</returns>
        /// <param name="opcode">Opcode.</param>
        public Type GetType(ushort opcode)
		{
			return this.opcodeTypes.GetValueByKey(opcode);
		}
		
		// 客户端为了0GC需要消息池，服务端消息需要跨协程不需要消息池
		public object GetInstance(ushort opcode)
		{
#if SERVER
			Type type = this.GetType(opcode);
			return Activator.CreateInstance(type);
#else
			return this.typeMessages[opcode];
#endif
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