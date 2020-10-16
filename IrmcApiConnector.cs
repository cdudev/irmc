#region DOC
/*
 * This class generates an IrmcApiConnector object which can be used for API connection (GET and POST operations) to Fujitsu's iRMC controller.
 * 
 * PROPERTIES:
 * token (string) 
 * --------------
 * This is a basic auth token needed as a value for the authorization header for the request.
 *  
 * client (HttpClient)
 * -------------------
 * A HttpClient object used for the connection to the iRMC api.
 * 
 * 
 * CONSTRUCTOR:
 * To create an instance of this class you have to pass a token string as a parameter.
 * The constructor sets the token and client properties and invokes the InitializeConnection method.
 * 
 * 
 * METHODS:
 * InitializeConnection()
 * ----------------------
 * Sets the acceptheader variable needed for the request.
 * Creates the authorizationheader with the provided token.
 * Creates the acceptheader with the acceptheader variable.
 * 
 * IrmcApiGetter(string _url)
 * --------------------------
 * Sends a GET request to the provided url and returns the response as a string, this method is executed as a async task.
 * 
 * IrmcApiPost(string _url, string _filepath)
 * ------------------------------------------
 * Sends a POST request to the provided url with an update image (only tested with an irmc update image, test with bios on to do list.
 * 
 * IrmcApiPost(string _url)
 * ------------------------
 * Sends a POST request to the provided url without extra data (eg: post a irmc reboot request)
 * 
 * 
 * EXAMPLE WITH POWERSHELL:
 * To use in Powershell this code needs to be compiled as a dll file, eg: IRMC.dll
 * 
 * 1. Create a token for basic authentication:
 *      $username = "username"
 *      $password = "pass"
 *      $credPair = "$($username):$($password)"
 *      $token = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($credPair))
 *
 * 2. Import the dll in Powershell:
 *      Add-Type -Path \path\to\IRMC.dll
 * 
 * 3. Create a new IRMC object, don't forget to pass the $token as parameter for the constructor:
 *      $irmcconnect = New-Object IRMC.IrmcApiConnector($token)
 * 
 * 4. Send a GET request:
 *      $irmcconnect.IrmcApiGetter("https://irmchostname/redfish/v1/Systems/0")
 * 
 * 5. Send a POST request to update irmc:
 *      $irmcconnect.IrmcApiPost("https://irmchostname/redfish/v1/Managers/iRMC/Actions/Oem.FTSManager.FWUpdate", "D:\irmc\update.BIN")
 *  
 * 6. Send a POST request to reboot irmc:
 *      $irmcconnect.IrmcApiPost("https://irmchostname/redfish/v1/Managers/iRMC/Actions/Manager.Reset")
 */
#endregion

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace IRMC
{
    public class IrmcApiConnector
    {
        #region PROPERTIES
        private string token { get; set; }
        private HttpClient client { get; set; }
        #endregion

        #region CONSTRUCTOR
        public IrmcApiConnector(string _token)
        {
            token = _token;
            client = new HttpClient();
            InitializeConnection();
        }
        #endregion

        #region METHODS
        private void InitializeConnection()
        {
            string acceptheader = "application/json";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptheader));
        }

        public async Task<string> IrmcApiGetter(string _url)
        {
            string url = _url;
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        public async Task<string> IrmcApiPost(string _url, string _filepath)
        {
            string url = _url;
            string filepath = _filepath;

            MultipartFormDataContent body = new MultipartFormDataContent();
            ByteArrayContent filecontent = new ByteArrayContent(File.ReadAllBytes(filepath));
            filecontent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            body.Add(filecontent, "data", Path.GetFileName(filepath));

            client.Timeout = TimeSpan.FromMinutes(10);

            HttpResponseMessage response = await client.PostAsync(url, body);
            response.EnsureSuccessStatusCode();
            string location = response.Headers.Location.OriginalString;
            return location;
        }
        public async Task<string> IrmcApiPost(string _url)
        {
            string url = _url;
            HttpResponseMessage response = await client.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        #endregion
    }
}
