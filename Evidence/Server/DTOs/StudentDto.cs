namespace Server.DTOs
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime AdmissionDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public string AddressJson { get; set; }
        public string ImageBase64 { get; set; }
    }
}
