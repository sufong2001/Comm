using System.Threading.Tasks;

namespace Sufong2001.Comm.Tests.Base
{
    public interface IGithubClient
  {
    Task<GithubUser> GetUserAsync(string userName);
  }
}
