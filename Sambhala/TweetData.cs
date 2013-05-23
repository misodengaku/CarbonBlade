using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sambhala
{
    [DataContract]
    public class TweetData
    {
        [DataMember(Name = "created_at", Order = 0)]
        public string CreatedAt { get; set; }

        [DataMember(Name = "id", Order = 1)]
        public string ID { get; set; }

        [DataMember(Name = "text", Order = 2)]
        public string Text { get; set; }

        [DataMember(Name = "source", Order = 3)]
        public string Source { get; set; }

        [DataMember(Name = "truncated", Order = 4)]
        public bool Truncated { get; set; }

        [DataMember(Name = "in_reply_to_status_id", Order = 5)]
        public string InReplyToStatusId { get; set; }

        [DataMember(Name = "in_reply_to_user_id", Order = 6)]
        public string InReplyToUserId { get; set; }

        [DataMember(Name = "favorited", Order = 7)]
        public bool Favorited { get; set; }

        [DataMember(Name = "user", Order = 8)]
        public User User { get; set; }

        [DataMember(Name = "target_object")]
        public FavTweetData Target { get; set; }

        [DataMember(Name = "event")]
        public string Event { get; set; }

        [DataMember(Name = "retweeted_status")]
        public TweetData RetweetedStatus { get; set; }

        public bool isRetweetStatus { get; set; }
    }

    [DataContract]
    public class RestTweetData
    {
        [DataMember]
        public IEnumerable<TweetData> Statuses { get; set; }
    }

    [DataContract]
    public sealed class FavTweetData// : TweetData
    {
        [DataMember(Name = "target_object")]
        public TweetData TargetTweet { get; set; }

        [DataMember(Name = "source")]
        public User SourceUser { get; set; }


    }

    /*[DataContract(Name = "user", Namespace = "")]
    public sealed class TargetObj
    {

    }*/

    [DataContract(Name = "user", Namespace = "")]
    public sealed class User
    {
        public Uri Url;

        [DataMember(Name = "id", Order = 0)]
        public string ID { get; set; }

        [DataMember(Name = "name", Order = 1)]
        public string Name { get; set; }

        [DataMember(Name = "screen_name", Order = 2)]
        public string ScreenName { get; set; }

        [DataMember(Name = "description", Order = 3)]
        public string Description { get; set; }

        [DataMember(Name = "location", Order = 4)]
        public string Location { get; set; }

        [DataMember(Name = "profile_image_url", Order = 5)]
        public string ProfileImageUrl { get; set; }

        [DataMember(Name = "url", Order = 6)]
        public String UrlString
        {
            get
            {
                string ret = "";
                try
                {
                    ret = Url.ToString();
                }
                catch { }
                return ret;
            }
            set
            {
                if (value != null && value != "")
                    Url = new Uri(value);

            }
        }

        [DataMember(Name = "protected", Order = 7)]
        public bool Protected { get; set; }

        [DataMember(Name = "followers_count", Order = 8)]
        public int FollowersCount { get; set; }

        [DataMember(Name = "favourites_count")]
        public int FavsCount { get; set; }

        [DataMember(Name = "friends_count")]
        public int FollowingCount { get; set; }

        [DataMember(Name = "statuses_count")]
        public int StatusesCount { get; set; }
    }
}