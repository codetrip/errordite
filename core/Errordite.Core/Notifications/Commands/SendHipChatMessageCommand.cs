using System;
using Errordite.Core;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using HipChat;

namespace Errordite.Core.Notifications.Commands
{
    /// <summary>
    /// Renders an HipChatMessage request down to some text, then sends it.
    /// </summary>
    public class SendHipChatMessageCommand : ComponentBase, ISendHipChatMessageCommand
    {
        public SendHipChatMessageResponse Invoke(SendHipChatMessageRequest request)
        {
            if(request.HipChatRoomId > 0 && request.HipChatAuthToken.IsNotNullOrEmpty())
            { 
                if (request.Colour.HasValue)
                    HipChatClient.SendMessage(request.HipChatAuthToken, request.HipChatRoomId, "Errordite", request.Message, request.Colour.Value);
                else
                    HipChatClient.SendMessage(request.HipChatAuthToken, request.HipChatRoomId, "Errordite", request.Message);
            }
            
            return new SendHipChatMessageResponse();
        }
    }

    public interface ISendHipChatMessageCommand : ICommand<SendHipChatMessageRequest, SendHipChatMessageResponse> 
    { }

    public class SendHipChatMessageRequest
    {
        public string Message { get; set; }
        public int HipChatRoomId { get; set; }
        public string HipChatAuthToken { get; set; }
        public HipChatClient.BackgroundColor? Colour { get; set; }
    }

    public class SendHipChatMessageResponse
    {}
}