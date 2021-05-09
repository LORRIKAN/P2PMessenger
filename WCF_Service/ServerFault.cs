using System.Runtime.Serialization;

namespace WCF_Service
{
    [DataContract]
    public enum ServerFault
    {
        [EnumMember]
        ArgumentNull_Exception,
        [EnumMember]
        ServerClient_NicknameOccupied_Exception,
        [EnumMember]
        ServerClient_IPAddressOccupied_Exception,
        [EnumMember]
        Session_NameOccupied_Exception,
        [EnumMember]
        WrongPassword_Exception
    }
}