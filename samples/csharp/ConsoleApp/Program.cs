using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Client.Http;
using GraphQL;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using Newtonsoft.Json;
using GraphQL.Client.Abstractions;

public partial class Program
{

    /// <summary>
    /// These values are provided to you by AuctionEdge
    /// </summary>
    private static string auctionCode = "AUCTION_CODE";
    private static string username = "USER_NAME";
    private static string password = "USER_PASSWORD";
    private static string clientId = "CLIENT_ID";
    private static string api_host = "API_HOST";


    public static async Task Main(string[] args)
    {

        try
        {
            var accessToken = await AuthenticateUser();

            // Setup the GraphQL Client
            var graphQLClient = new GraphQLHttpClient(
                $"https://{api_host}/graphql",
                new NewtonsoftJsonSerializer());

            Console.WriteLine($"Bearer {accessToken}");
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            // Make a simple query
            var query = new GraphQLRequest
            {
                Query = @"query($auctionCode: ID!) {
                    auction(id: $auctionCode) {
                        details{
                            ams {
                                siteName
                            }
                        }
                        assets {
                            purchased {
                                items {
                                    vin
                                    year
                                    make
                                    model
                                }
                            }
                        }
                    }
                }",
                Variables = new
                {
                    auctionCode
                }
            };

            var response = await graphQLClient.SendQueryAsync<QueryResponse>(query);
            WriteError(JsonConvert.SerializeObject(response, Formatting.Indented));

            if (response.Errors != null && response.Errors.Any())
            {
                return;
            }

            // Output the results
            foreach (var asset in response.Data.Auction.Assets.Purchased.Items)
            {
                Console.WriteLine($"{asset.Year} {asset.Make} {asset.Model} ({asset.Vin})");
            }
        }
        catch (Exception ex)
        {
            WriteError(ex.Message);
        }

        static async Task<string> AuthenticateUser()
        {
            // Authenticate with the API
            // The Amazon Cognito service client with anonymous credentials
            var cognitoProvider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), FallbackRegionFactory.GetRegionEndpoint());

            var authRequest = new InitiateAuthRequest
            {
                ClientId = clientId,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", username },
                    { "PASSWORD", password }
                },
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            };

            var authResponse = await cognitoProvider.InitiateAuthAsync(authRequest);

            if (authResponse.AuthenticationResult != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine($"Authentication successful for {AuthFlowType.USER_PASSWORD_AUTH}");
                Console.ResetColor();
            }
            else
            {
                // RespondToAuthChallenge is required for the next challenge i.e. SMS_MFA, MFA_SETUP, etc.
                Console.WriteLine($"Additional challenge {authResponse.ChallengeName} is required");
                throw new Exception($"Additional challenge {authResponse.ChallengeName} is required");
            }

            return authResponse.AuthenticationResult?.AccessToken;
        }
    }

    /// <summary>
    /// Prints error message in red color
    /// </summary>
    public static void WriteError(string buffer)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.WriteLine(buffer);
        Console.ResetColor();
    }
}

/// <summary>
/// This is the root of the response graph
/// </summary>
public class QueryResponse
{
    public Auction Auction { get; set; } = new Auction();
}

public class Auction
{
    public AuctionDetails Details { get; set; } = new AuctionDetails();
    public Assets Assets { get; set; } = new Assets();
}
public class AuctionDetails
{
    public AmsDetails Ams { get; set; } = new AmsDetails();
}
public class AmsDetails
{
    public string SiteName { get; set; } = string.Empty;
}

public class Assets
{
    public PurchasedAssets Purchased { get; set; } = new PurchasedAssets();
}

public class PurchasedAssets
{
    public List<Asset> Items { get; set; } = new List<Asset>();
}
public class Asset
{
    public string Vin { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}
