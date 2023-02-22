namespace vdb_node_api.Models
{
    public class MasterAccount
    {
        public string KeyHashBase64 { get; set; }

        public MasterAccount(string keyHashBase64)
        {
            KeyHashBase64 = keyHashBase64;
        }
    }
}
