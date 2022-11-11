using LinqToTwitter;
using LinqToTwitter.OAuth;
using LinqToTwitter.Common;

var screenname = "calmandcooldude"; // your own username
var targetScreenName = "Khamenei_fa"; // the username that you want to block all their followers

var auth = new SingleUserAuthorizer
{

    CredentialStore = new InMemoryCredentialStore()
    {
        // Consumer Keys
        ConsumerKey = "Z0UDbzJZs7UdTVroPbeKiRViy",
        ConsumerSecret = "R1eJLtBGBlatkJPucHEqPZndEESnwAVXxCkMaZMXqV25p79WFg",
        // Access Token and Secret
        OAuthToken = "568833316-WIqzPqd5taATn68sV9tdZLAK4loGIexNBqCr6nja",
        OAuthTokenSecret = "qbFjUCpAaocWoBMtuHl4P160A3CHbuzW9qTzaDmFxAASX"
    }
};
var twitterCtx = new TwitterContext(auth);
var ownTweets = new List<Status>();

Friendship? friendship;
long cursor = -1;
do
{
    try
    {
        friendship =
            await
            (from friend in twitterCtx.Friendship
             where friend.Type == FriendshipType.FollowersList &&
                   friend.ScreenName == targetScreenName &&
                   friend.Cursor == cursor
             select friend)
            .SingleOrDefaultAsync();
    }
    catch (TwitterQueryException tqe)
    {
        Console.WriteLine(tqe.ToString());
        break;
    }

    TwitterUserQuery? userResponse =
    (from usr in twitterCtx.TwitterUser
     where usr.Type == UserType.UsernameLookup &&
           usr.Usernames == screenname
     select usr)
    .SingleOrDefault();

    string? targetUserID = userResponse?.Users?.FirstOrDefault()?.ID;

    if (friendship != null && friendship.Users != null)
    {
        cursor = friendship.CursorMovement?.Next ?? 0L;

        friendship.Users.ForEach(friend =>
        {
            Console.WriteLine(
                "ID: {0} Name: {1}",
                friend.UserIDResponse, friend.ScreenNameResponse);

            string? targetUserID = friend.UserIDResponse;
            string? sourceUserID = userResponse?.Users?.FirstOrDefault()?.ID;
            // string? sourceUserID = twitterCtx.Authorizer?.CredentialStore?.UserID.ToString();

            if (targetUserID == null || sourceUserID == null)
            {
                Console.WriteLine($"Either {nameof(targetUserID)} or {nameof(sourceUserID)} is null.");
                return;
            }

            BlockingResponse? user = twitterCtx.BlockUserAsync(sourceUserID, targetUserID).Result;
            Thread.Sleep(10000);

            if (user?.Data != null)
                Console.WriteLine("Is Blocked: " + user.Data.Blocking);

        });
    }
} while (cursor != 0);