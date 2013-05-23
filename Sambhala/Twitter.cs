using AsyncOAuth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Sambhala
{
    // a sample of twitter client
    public class TwitterClient
    {
        readonly string consumerKey;
        readonly string consumerSecret;
        readonly AccessToken accessToken;
        static RequestToken requestToken;

        public TwitterClient(string consumerKey, string consumerSecret, AccessToken accessToken)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.accessToken = accessToken;
        }

        // sample flow for Twitter authroize
        public async static Task<string> GetAuthorizeUrl(string consumerKey, string consumerSecret)
        {
            // create authorizer
            var authorizer = new OAuthAuthorizer(consumerKey, consumerSecret);

            // get request token
            var tokenResponse = await authorizer.GetRequestToken("https://api.twitter.com/oauth/request_token");
            requestToken = tokenResponse.Token;

            var pinRequestUrl = authorizer.BuildAuthorizeUrl("https://api.twitter.com/oauth/authorize", requestToken);

            // open browser and get PIN Code

            return pinRequestUrl;
        }

        // sample flow for Twitter authroize
        public async static Task<AccessToken> GetAccessToken(string consumerKey, string consumerSecret, string pinCode)
        {
            // create authorizer
            var authorizer = new OAuthAuthorizer(consumerKey, consumerSecret);

            // get access token
            var accessTokenResponse = await authorizer.GetAccessToken("https://api.twitter.com/oauth/access_token", requestToken, pinCode);

            // save access token.
            var accessToken = accessTokenResponse.Token;

            return accessToken;
        }

        public async Task<string> GetTimeline(int count, int page)
        {
            var client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            var json = await client.GetStringAsync("http://api.twitter.com/1.1/statuses/home_timeline.json?count=" + count + "&page=" + page);
            return json;
        }

        public async Task<string> PostUpdate(string status)
        {
            var client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("status", status) });

            var response = await client.PostAsync("http://api.twitter.com/1.1/statuses/update.json", content);
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }

        public async Task<string> Favorite(string id)
        {
            //http://api.twitter.com/1/favorites/create/id.format
            var client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("id", id) });

            var response = await client.PostAsync("http://api.twitter.com/1.1/favorites/create.json", content);
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }

        public async Task GetStream(Action<string> fetchAction)
        {
            var client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);
            client.Timeout = System.Threading.Timeout.InfiniteTimeSpan; // set infinite timespan

            using (var stream = await client.GetStreamAsync("https://userstream.twitter.com/1.1/user.json").ConfigureAwait(false))
            using (var sr = new StreamReader(stream))
            {
                while (!sr.EndOfStream)
                {
                    var s = await sr.ReadLineAsync().ConfigureAwait(false);
                    //s = Regex.Unescape(s);
                    fetchAction(HttpUtility.HtmlDecode(s));
                }
            }
        }

        // if you use Rx, you can write follows
        public IObservable<string> GetStream()
        {
            return Observable.Create<string>(async (observer, ct) =>
            {
                try
                {
                    var client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);
                    client.Timeout = System.Threading.Timeout.InfiniteTimeSpan; // set infinite timespan

                    using (var stream = await client.GetStreamAsync("https://userstream.twitter.com/1.1/user.json").ConfigureAwait(false))
                    using (var sr = new StreamReader(stream))
                    {
                        while (!sr.EndOfStream && !ct.IsCancellationRequested)
                        {
                            var s = await sr.ReadLineAsync().ConfigureAwait(false);
                            s = Regex.Unescape(s);
                            observer.OnNext(HttpUtility.HtmlDecode(s));
                        }
                    }
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                    return;
                }
                if (!ct.IsCancellationRequested)
                {
                    observer.OnCompleted();
                }
            });
        }

        public async Task<string> UpdateWithMedia(string status, byte[] media, string fileName)
        {
            var client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(status), "\"status\"");
            content.Add(new ByteArrayContent(media), "media[]", "\"" + fileName + "\"");

            var response = await client.PostAsync("https://upload.twitter.com/1/statuses/update_with_media.json", content);
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }
    }
}