namespace BetriebsmittelPublisher.Models
{
    public enum MqttPacketType : byte
    {
        Connect = 1,
        ConnAck = 2,
        Publish = 3,
        PubAck = 4,
        PubRec = 5,
        PubRel = 6,
        PubComp = 7,
        Subscribe = 8,
        SubAck = 9,
        Unsubscribe = 10,
        UnsubAck = 11,
        PingReq = 12,
        PingResp = 13,
        Disconnect = 14
    }

    public enum MqttQoS : byte
    {
        AtMostOnce = 0,
        AtLeastOnce = 1,
        ExactlyOnce = 2
    }

    public class MqttMessage
    {
        public string Topic { get; set; }
        public byte[] Payload { get; set; }
        public MqttQoS QoS { get; set; }
        public ushort PacketId { get; set; }
        public bool Retain { get; set; }
        public bool Dup { get; set; }
    }

    public class MqttConnectMessage
    {
        public string ClientId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public ushort KeepAlive { get; set; }
        public bool CleanSession { get; set; } = true;
    }

    public class MqttPublishMessage : MqttMessage
    {
        public MqttPublishMessage()
        {
            QoS = MqttQoS.AtMostOnce;
            Retain = false;
            Dup = false;
        }
    }

    public class MqttConnAckMessage
    {
        public bool SessionPresent { get; set; }
        public byte ReturnCode { get; set; }
    }
}