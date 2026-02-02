namespace ApiPortalActividades.DTOs
{
    public class OrganizerCreateDto
    {
        // USER
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }

        // ORGANIZER
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Bio { get; set; }

        public List<string>? Shifts { get; set; }
        public List<string>? WorkDays { get; set; }
    };
}