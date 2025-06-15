namespace Teams.API.Layer.DTOs;

public record MemberLeaveDateDto(Guid MemberId, DateTime? LastLeaveDate); // Pourquoi c'est un record ??
