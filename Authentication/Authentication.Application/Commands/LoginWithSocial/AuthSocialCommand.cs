namespace Authentication.Application.Commands.LoginWithSocial;

public record AuthSocialCommand(string tokenId, string provider);