using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;

namespace Surging.Core.CPlatform.Address
{
    /// <summary>
    /// ip��ַģ�͡�
    /// </summary>
    public sealed class IpAddressModel : AddressModel
    {
        #region ���캯��

        /// <summary>
        /// Initializes a new instance of the <see cref="IpAddressModel"/> class.
        /// </summary>
        public IpAddressModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IpAddressModel"/> class.
        /// </summary>
        /// <param name="ip">ip��ַ��</param>
        /// <param name="port">�˿ڡ�</param>
        public IpAddressModel(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }

        #endregion ���캯��

        #region ����

        /// <summary>
        /// Gets or sets the HttpPort
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? HttpPort { get; set; }

        /// <summary>
        /// Gets or sets the Ip
        /// ip��ַ��
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets the MqttPort
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MqttPort { get; set; }

        /// <summary>
        /// Gets or sets the Port
        /// �˿ڡ�
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the WanIp
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string WanIp { get; set; }

        /// <summary>
        /// Gets or sets the WsPort
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? WsPort { get; set; }

        #endregion ����

        #region ����

        /// <summary>
        /// �����ս�㡣
        /// </summary>
        /// <returns></returns>
        public override EndPoint CreateEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(AddressHelper.GetIpFromAddress(Ip)), Port);
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return string.Concat(new string[] { AddressHelper.GetIpFromAddress(Ip), ":", Port.ToString() });
        }

        #endregion ����
    }
}