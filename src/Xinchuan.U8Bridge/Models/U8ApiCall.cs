namespace Xinchuan.U8Bridge.Models
{
    public sealed class U8ApiCall
    {
        public string ApiAddress { get; set; }

        public string BusinessNo { get; set; }

        public string DocumentType { get; set; }

        public object Payload { get; set; }

        public static U8ApiCall Create(string apiAddress, string documentType, string businessNo, object payload)
        {
            return new U8ApiCall
            {
                ApiAddress = apiAddress,
                BusinessNo = businessNo,
                DocumentType = documentType,
                Payload = payload
            };
        }
    }
}
