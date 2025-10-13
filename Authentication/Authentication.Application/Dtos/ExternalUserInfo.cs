namespace Authentication.Application.Dtos;

public record ExternalUserInfo(
    string ProviderId,
    string Email,
    string Name
    );