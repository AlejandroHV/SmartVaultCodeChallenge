namespace SmartVault.DataGeneration.Models
{
    public  class Document
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public int Length { get; set; }
        public int AccountId { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
