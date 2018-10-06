using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class ServerFrameComponentSystem : AwakeSystem<ServerFrameComponent>
    {
	    public override void Awake(ServerFrameComponent self)
	    {
		    self.Awake();
	    }
    }
    /// <summary>
    /// 处理帧同步
    /// </summary>
    public static class ServerFrameComponentEx
    {
        public static void Awake(this ServerFrameComponent self)
        {
            self.Frame = 0;
            self.FrameMessage = new FrameMessage() {Frame = self.Frame};

            self.UpdateFrameAsync();
        }

        public static async void UpdateFrameAsync(this ServerFrameComponent self)
        {
            TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();

            long instanceId = self.InstanceId;

            while (true)
            {
                if (self.InstanceId != instanceId)
                {
                    return;
                }

                await timerComponent.WaitAsync(100);  //延时100毫秒

                MessageHelper.Broadcast(self.FrameMessage);  //广播一次帧消息

                ++self.Frame; //帧+1
                self.FrameMessage = new FrameMessage() { Frame = self.Frame };
            }
        }
        /// <summary>
        /// 把一帧的消息放到帧消息链表中
        /// </summary>
        /// <param name="self"></param>
        /// <param name="oneFrameMessage"></param>
        public static void Add(this ServerFrameComponent self, OneFrameMessage oneFrameMessage)
        {
            self.FrameMessage.Message.Add(oneFrameMessage);
        }
    }
}