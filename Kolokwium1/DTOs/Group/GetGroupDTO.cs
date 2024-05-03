namespace Kolokwium1.DTOs.Group;

public record GetGroupDTO(int Id, string Name, List<int> students);