using MessagePack;
using Surging.Core.CPlatform.Messages;
using System.Runtime.CompilerServices;

namespace Surging.Core.Codec.MessagePack.Messages
{
    /// <summary>
    /// Defines the <see cref="MessagePackRemoteInvokeResultMessage" />
    /// </summary>
    [MessagePackObject]
    public class MessagePackRemoteInvokeResultMessage
    {
        #region ���캯��

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackRemoteInvokeResultMessage"/> class.
        /// </summary>
        public MessagePackRemoteInvokeResultMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackRemoteInvokeResultMessage"/> class.
        /// </summary>
        /// <param name="message">The message<see cref="RemoteInvokeResultMessage"/></param>
        public MessagePackRemoteInvokeResultMessage(RemoteInvokeResultMessage message)
        {
            ExceptionMessage = message.ExceptionMessage;
            Result = message.Result == null ? null : new DynamicItem(message.Result);
        }

        #endregion ���캯��

        #region ����

        /// <summary>
        /// Gets or sets the ExceptionMessage
        /// </summary>
        [Key(0)]
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Gets or sets the Result
        /// </summary>
        [Key(1)]
        public DynamicItem Result { get; set; }

        #endregion ����

        #region ����

        /// <summary>
        /// The GetRemoteInvokeResultMessage
        /// </summary>
        /// <returns>The <see cref="RemoteInvokeResultMessage"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RemoteInvokeResultMessage GetRemoteInvokeResultMessage()
        {
            return new RemoteInvokeResultMessage
            {
                ExceptionMessage = ExceptionMessage,
                Result = Result?.Get()
            };
        }

        #endregion ����
    }
}