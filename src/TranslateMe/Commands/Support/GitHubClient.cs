using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using TranslateMe.Model.GitHub;

namespace TranslateMe.Commands.Support
{
    public class GitHubClient
    {
        private const string AssetBaseUrl = "https://github.com/tobper/TranslateMe/releases/download/{0}/{1}";
        private const string ReleaseUrl = "https://api.github.com/repos/tobper/TranslateMe/releases";

        private readonly static DataContractJsonSerializer GitHubReleasesSerializer;

        static GitHubClient()
        {
            GitHubReleasesSerializer = new DataContractJsonSerializer(typeof(GitHubRelease[]));
        }

        public async Task<string> DownloadAsset(GitHubRelease release, GitHubAsset asset)
        {
            var tempPath = Path.GetTempPath();
            var tempFileName = Path.Combine(tempPath, asset.Name);

            if (!File.Exists(tempFileName))
            {
                var downloadUrl = string.Format(AssetBaseUrl, release.TagName, asset.Name);
                var webClient = CreateWebClient();

                await webClient.DownloadFileTaskAsync(downloadUrl, tempFileName);
            }

            return tempFileName;
        }

        public async Task<GitHubRelease> GetLatestRelease()
        {
            try
            {
                var webClient = CreateWebClient();
                var json = await webClient.DownloadDataTaskAsync(ReleaseUrl);
                var stream = new MemoryStream(json);
                var releases = (GitHubRelease[])GitHubReleasesSerializer.ReadObject(stream);

                return releases.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static WebClient CreateWebClient()
        {
            return new WebClient
            {
                Headers =
                {
                    { "Accept", "application/vnd.github.v3+json" },
                    { "User-Agent", "TranslateMe" }
                }
            };
        }
    }
}