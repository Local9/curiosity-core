namespace Atlas.Roleplay.Library.Billing
{
    public class BillReceiver
    {
        public BillReceiverType Type { get; set; }
        public string Name { get; set; }
    }

    public enum BillReceiverType
    {
        Business,
        Individual
    }
}