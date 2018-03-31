using Signals;

namespace Poster.Tcp.Examples.Chat
{
    class ChatClient
    {
        public IMessageSender ServerSender { get; set; }
        public IMessageBinder ServerBinder { get; set; }

        public ISignal<(string, string)> MessageRecvSignal { get; set; }
        public ISignal<string> ClientConnectSignal { get; set; }
        public ISignal<string> ClientLeaveSignal { get; set; }

        public ChatClient(IMessageSender serverSender, IMessageBinder serverBinder, ISignal<(string, string)> messageRecvSignal, ISignal<string> clientConnectSignal, ISignal<string> clientLeaveSignal)
        {
            ServerSender = serverSender;
            ServerBinder = serverBinder;
            MessageRecvSignal = messageRecvSignal;
            ClientConnectSignal = clientConnectSignal;
            ClientLeaveSignal = clientLeaveSignal;

            ServerBinder.Bind<MessageClientConnect>(clientConnected);
            ServerBinder.Bind<MessageServerLeave>(clientLeave);
            ServerBinder.Bind<MessageToClientMessage>(recvMessage);
        }


        public void StartSession(string nick)
        {
            ServerSender.Send(new MessageClientConnect(){Nick = nick});
        }

        public void SendMessage(string message)
        {
            ServerSender.Send(new MessageMessage(){Message = message});
        }



        private void recvMessage(MessageToClientMessage message)
        {
            MessageRecvSignal.Invoke((message.Nick, message.Message));
        }

        private void clientLeave(MessageServerLeave message)
        {
            ClientLeaveSignal.Invoke(message.Nick);
        }

        private void clientConnected(MessageClientConnect message)
        {
            ClientConnectSignal.Invoke(message.Nick);
        }
    }
}