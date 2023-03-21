namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Identity.Domain.Entities;

    public interface IUserTokenRepository
    {
        Task<UserToken?> GetByRefreshTokenAsync(string? refreshToken);

        Task<UserToken> AddAsync(UserToken userToken);

        Task<UserToken> Remove(UserToken userToken);
    }
}
