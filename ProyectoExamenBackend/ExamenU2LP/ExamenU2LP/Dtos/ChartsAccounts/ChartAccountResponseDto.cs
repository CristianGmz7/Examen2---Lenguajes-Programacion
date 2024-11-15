namespace ExamenU2LP.Dtos.ChartsAccounts
{
    public class ChartAccountResponseDto
    {
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public Guid BehaviorTypeId { get; set; }
        public string BehaviorTypeName { get; set; }
        public bool AllowsMovement { get; set; }
        public string ParentAccountNumber { get; set; }
        public string ParentAccountName { get; set; }
        public bool IsDisabled { get; set; }
        public decimal Balance { get; set; }
    }
}
