using System;
using System.Collections.Generic;

namespace ETModel
{
    /// <summary>
    /// 组件 pool.
    /// </summary>
    public class ObjectPool
    {
        /// <summary>
        /// 字典  类型->组件堆栈
        /// </summary>
        private readonly Dictionary<Type, Queue<Component>> dictionary = new Dictionary<Type, Queue<Component>>();

        /// <summary>
        /// 取出
        /// </summary>
        /// <param name="type">组件类型</param>
        /// <returns></returns>
        public Component Fetch(Type type)
        {
	        Queue<Component> queue;
            if (!this.dictionary.TryGetValue(type, out queue)) //如果池中没有 新创建一个KEY
            {
                queue = new Queue<Component>();
                this.dictionary.Add(type, queue);
            }
	        Component obj;
			if (queue.Count > 0)        //栈中数量大于0 取出        
            {
				obj = queue.Dequeue();
            }
			else                         //栈中没有  反射生成新的类
			{
				obj = (Component)Activator.CreateInstance(type);	
			}
	        obj.IsFromPool = true;    //是从内存池中生成的
            return obj;  
        }

        /// <summary>
        /// 取出组件  泛型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public T Fetch<T>() where T: Component
		{
            T t = (T) this.Fetch(typeof(T));
			return t;
		}
        
        /// <summary>
        /// 回收
        /// </summary>
        /// <param name="obj"></param>
        public void Recycle(Component obj)
        {
            Type type = obj.GetType();
	        Queue<Component> queue;
            if (!this.dictionary.TryGetValue(type, out queue)) //没有这个KEY就新建
            {
                queue = new Queue<Component>();
				this.dictionary.Add(type, queue);
            }
            queue.Enqueue(obj);
        }
    }
}