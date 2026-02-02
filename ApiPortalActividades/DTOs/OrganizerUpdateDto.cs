namespace ApiPortalActividades.DTOs
{
    public class OrganizerUpdateDto
    {
        // USER
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }

        // ORGANIZER
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Bio { get; set; }
        public string? Shifts { get; set; }
        public string? WorkDays { get; set; }

        public bool? Active { get; set; }
    };
}