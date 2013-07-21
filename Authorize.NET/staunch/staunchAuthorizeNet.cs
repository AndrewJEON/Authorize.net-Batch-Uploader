using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Required
using System.Net;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.IO;


public class CookieAwareWebClient : WebClient
{
    public CookieContainer CookieContainer { get; set; }
    public Uri Uri { get; set; }

    public CookieAwareWebClient()
        : this(new CookieContainer())
    {
    }

    public CookieAwareWebClient(CookieContainer cookies)
    {
        this.CookieContainer = cookies;
    }

    protected override WebRequest GetWebRequest(Uri address)
    {
        WebRequest request = base.GetWebRequest(address);
        if (request is HttpWebRequest)
        {
            (request as HttpWebRequest).CookieContainer = this.CookieContainer;
        }
        HttpWebRequest httpRequest = (HttpWebRequest)request;
        httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        return httpRequest;
    }

    protected override WebResponse GetWebResponse(WebRequest request)
    {
        WebResponse response = base.GetWebResponse(request);

        var thisResponse = response as HttpWebResponse;
        this.CookieContainer.Add(thisResponse.Cookies);

        return response;
    }
}

namespace AuthorizeNet
{
    public class staunchAuthorizeNet
    {
        string apiKey;
        string transactionKey;
        bool developerTesting;

        public staunchAuthorizeNet(string apiKey = "", string transactionKey = "", bool testing = true)
        {
            this.apiKey = apiKey;
            this.transactionKey = transactionKey;
            this.developerTesting = testing;
        }

        public CardPresentResponse cpTest()
        {
            return this.cardPresent(332.40M, "%B4111111111111111^CARDUSER/JOHN^1803101000000000020000831000000?", ";4111111111111111=1803101000020000831?");
        }

        public IGatewayResponse cnpTest()
        {
            return this.cardNotPresent("4111111111111111", "1216", 1314.00M, "Test Transaction");
        }

        public CardPresentResponse cardPresent(decimal amount, string track1, string track2)
        {
            //Step 1 Create the request
            var request = new CardPresentAuthorizationRequest(amount, track1, track2);

            //step 2 - create the gateway, sending in your credentials
            var gate = new CardPresentGateway(apiKey, transactionKey, developerTesting);

            //step 3 - make some money
            var response = (CardPresentResponse)gate.Send(request);

            return response;
        }

        public IGatewayResponse cardNotPresent(string cardNumber, string expMonthYear, decimal amount, string description)
        {
            //Step 1 Create the request
            var request = new AuthorizationRequest(cardNumber, expMonthYear, amount, description);

            //step 2 - create the gateway, sending in your credentials
            var gate = new Gateway(apiKey, transactionKey, developerTesting);

            //step 3 - make some money
            var response = gate.Send(request);

            return response;
        }

        ///////////////////////
        //Michael Champagnes Super cool batch uploader
        //////////////////////
        //required for the "cookieAwareWebClient" some pages need cookies
        private CookieContainer cookies = new CookieContainer();

        //Basic regex function
        private string regexSearch(string input, string pattern)
        {
            Match match = Regex.Match(input, pattern, RegexOptions.None);

            if (match.Success)
            {
                return match.Value;
            }
            else
            {
                return null;
            }
        }
        private hiddenValues regexDualSearch(string input, string pattern1, string pattern2)
        {
            //Initialize output
            hiddenValues output = new hiddenValues(false);

            //search for the viewstate variable
            string p1 = regexSearch(input, pattern1);

            if (p1 != null)
            {
                output.value1 = p1;
                output.success = true;
            }
            else
                output.success = false;

            //search for the login key variable
            string p2 = regexSearch(input, pattern2);

            if (p2 != null)
            {
                output.value2 = p2;
                output.success = true;
            }
            else
                output.success = false;

            //All done
            return output;
        }

        //validators
        //because of regexDualSearch these only serve as a way for better error reporting
        private struct hiddenValues
        {
            public bool success;
            public string value1;
            public string value2;

            public hiddenValues(bool succcess = false, string value1 = "", string value2 = "")
            {
                this.success = succcess;
                this.value1 = value1;
                this.value2 = value2;
            }
        };
        private hiddenValues getLogonValues(string htmlInput)
        {
            //Initialize output
            hiddenValues output = new hiddenValues(false, "Failed to find view state", "Failed to find login key");

            //run regex
            hiddenValues regexOutput = regexDualSearch(htmlInput, "(?<=\"__VIEWSTATE\" value=\")([ \\=A-Za-z0-9+/:|]*)(?=\")", "(?<=__LOGIN_PAGE_KEY\" value=\")([ \\=A-Za-z0-9+\\*/:|]*)(?=\")");

            //go through results
            if (regexOutput.success)
            {
                output.success = true;
                output.value1 = regexOutput.value1;
                output.value2 = regexOutput.value2;
            }
            else
            {
                if (regexOutput.value1 != "")
                    output.value1 = regexOutput.value1;

                if (regexOutput.value2 != "")
                    output.value2 = regexOutput.value2;
            }

            //done
            return output;
        }
        private hiddenValues getUploadValues(string htmlInput)
        {
            //Initialize output
            hiddenValues output = new hiddenValues(false, "Failed to find view state", "Failed to find login key");

            //run regex
            hiddenValues regexOutput = regexDualSearch(htmlInput, "(?<=SessionToken=)([ \\=A-Za-z0-9+/:|%_]*)(?=\">)", "(?<=o.value = \")([ \\=A-Za-z0-9+/:|%_]*)");

            //go through results
            if (regexOutput.success)
            {
                output.success = true;
                output.value1 = regexOutput.value1;
                output.value2 = regexOutput.value2;
            }
            else
            {
                if (regexOutput.value1 != "")
                    output.value1 = regexOutput.value1;

                if (regexOutput.value2 != "")
                    output.value2 = regexOutput.value2;
            }

            //done
            return output;
        }
        private hiddenValues verifyUpload(string htmlInput)
        {
            //Initialize output
            hiddenValues output = new hiddenValues(false, "Failed to find view state", "Failed to find login key");

            //run regex
            hiddenValues regexOutput = regexDualSearch(htmlInput, "(?<=Received successfully. FileID = )([[0-9]*)", "(?<=Number of transactions in the batch : )([[0-9]*)");

            //go through results
            if (regexOutput.success)
            {
                output.success = true;
                output.value1 = regexOutput.value1;
                output.value2 = regexOutput.value2;
            }
            else
            {
                if (regexOutput.value1 != "")
                    output.value1 = regexOutput.value1;

                if (regexOutput.value2 != "")
                    output.value2 = regexOutput.value2;
            }

            //done
            return output;
        }

        private bool checkLoggedIn(string htmlInput)
        {
            //Super simple way of checking if we logged in
            string regex = "(Authorize.Net Home)";
            string result = regexSearch(htmlInput, regex);

            if (result != null)
            {
                return true;
            }
            else
                return false;
        }

        //web requests
        private string getRequest(string url, NameValueCollection get = null, string referral = "")
        {
            //initialize client
            CookieAwareWebClient webClient = new CookieAwareWebClient(cookies);

            //generate GET uri
            string request = url;
            if (get != null)
            {
                request += "?";

                int it = 0;
                foreach (string s in get)
                {
                    foreach (string v in get.GetValues(s))
                    {
                        if (it != 0)
                            request += "&";

                        request += s + "=" + v;

                        it++;
                    }
                }
            }

            //Set webclient referrer
            if (referral != "")
                webClient.Headers["Referer"] = referral;

            //issue the request and return output
            string output = webClient.DownloadString(request);
            webClient.Dispose();

            return output;
        }
        private string postRequest(string url, NameValueCollection post = null, string referral = "")
        {
            //initialize client
            CookieAwareWebClient webClient = new CookieAwareWebClient(cookies);

            //Set webclient referrer
            if (referral != "")
                webClient.Headers["Referer"] = referral;

            //issue the request
            byte[] responseBytes = webClient.UploadValues(url, "POST", post);
            string result = Encoding.UTF8.GetString(responseBytes);

            //clear buffers and return result
            webClient.Dispose();
            return result;
        }

        //postUpload gets its on function simply because of how differently things are required to be sent
        private string postUpload(string url, NameValueCollection post = null, string uploadDetails = "")
        {
            //Create multiform boundry then initialize the request
            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
            System.Net.WebRequest webRequest = System.Net.WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "multipart/form-data; boundary=" + boundary;

            //generate multiform header
            string header = boundary + Environment.NewLine +
                            "Content-Disposition: form-data; name=\"uploadfile\"; filename=\"transactions.txt\"" + Environment.NewLine +
                            "Content-Type: text/plain" + Environment.NewLine + Environment.NewLine +

                            uploadDetails + Environment.NewLine +
                            boundary + Environment.NewLine +
                            "Content-Disposition: form-data; name=\"__PAGE_KEY\"" + Environment.NewLine + Environment.NewLine +
                            post["__PAGE_KEY"] + Environment.NewLine +
                            boundary + "--";

            //encode header to bytes
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] body = encoding.GetBytes(header);

            //set contentlength header
            webRequest.ContentLength = body.Length;

            //create the stream and write the data
            Stream newStream = webRequest.GetRequestStream();
            newStream.Write(body, 0, body.Length);
            newStream.Close();

            //Get results from stream
            StreamReader sr = new StreamReader(webRequest.GetResponse().GetResponseStream());
            string Result = sr.ReadToEnd();

            //done
            return Result;
        }

        //VERY basic error reporting
        private string postUploadError(string htmlInput)
        {
            string output = "File Validation Error";

            string badFormat = "(The file format that you have provided does not match the format configured in your Upload Transaction File Settings)";
            string badValidation = "(The transaction has failed validation)";
            string duplicate = "(The file you have selected is a duplicate of a previously uploaded file)";

            if (regexSearch(htmlInput, badFormat) != null)
            {
                return "The data that you have provided does not match the format configured in your Upload Transaction File Settings";
            }
            else if (regexSearch(htmlInput, badValidation) != null)
            {
                return "The transaction has failed validation";
            }
            else if (regexSearch(htmlInput, duplicate) != null)
            {
                return "Duplicate transaction data";
            }

            return output;
        }

        //function used externally
        public struct BatchResult
        {
            public bool success;
            public string result;
            public string id;
            public string count;
            public string debug;

            public BatchResult(bool succcess = false, string result = "", string id = "0", string count = "0")
            {
                this.success = succcess;
                this.result = result;
                this.id = id;
                this.count = count;
                this.debug = "";
            }
        };

        public BatchResult BatchUpload(string username, string password, string batch, bool sandbox = true)
        {
            //Pages the module uploads information too
            string logon;
            string uploadPage;
            string uploadPostPage;

            if (sandbox)
            {
                logon = "https://sandbox.authorize.net/UI/themes/sandbox/logon.aspx";
                uploadPage = "https://sandbox.authorize.net/UI/themes/sandbox/popup.aspx?page=batchupload&sub=newfile";
                uploadPostPage = "https://test.authorize.net/batchprocessing/batchupload.dll?page=batchupload&sub=newfile&SessionToken=";
            }
            else
            {
                //YOU MUST FIND AND ADD THE NON SANDBOX URLS HERE AS I DO NOT KNOW THEM
                //these urls MIGHT worlk i only constructed them based on guessing
                logon = "https://account.authorize.net/ui/themes/anet/logon.aspx";
                uploadPage = "https://account.authorize.net/ui/themes/sandbox/popup.aspx?page=batchupload&sub=newfile";
                uploadPostPage = "https://account.authorize.net/batchprocessing/batchupload.dll?page=batchupload&sub=newfile&SessionToken=";
            }

            //initialize the result
            BatchResult output = new BatchResult();

            //Go the the logon page so we can obtain values we need for the next
            string logonGet = getRequest(logon);
            hiddenValues logonKeys = getLogonValues(logonGet);

            //if we found the required information continue
            if (logonKeys.success)
            {
                //we have obtained the first logon page
                NameValueCollection logonAuthData = new NameValueCollection();
                logonAuthData["MerchantLogin"] = username;
                logonAuthData["Password"] = password;
                logonAuthData["__LOGIN_PAGE_KEY"] = logonKeys.value2;
                logonAuthData["__VIEWSTATE"] = logonKeys.value1;
                logonAuthData["__VIEWSTATEENCRYPTED"] = "";
                logonAuthData["__PAGE_KEY"] = "";

                //now that we have collected all the data, let's proceed to logon
                string logonPost = postRequest(logon, logonAuthData, logon);
                bool loggedIn = checkLoggedIn(logonPost);

                if (loggedIn)
                {
                    //we have successfuly logged on
                    //go to the page before we upload a transaction
                    //this page has required details before we can proceed to the next
                    string upPage = getRequest(uploadPage);
                    hiddenValues uploadKeys = getUploadValues(upPage);

                    //if we found the required information continue
                    if (uploadKeys.success)
                    {
                        //set post data
                        NameValueCollection uploadFileData = new NameValueCollection();
                        uploadFileData["__PAGE_KEY"] = uploadKeys.value2;

                        //upload file and check result
                        string uploadFile = postUpload(uploadPostPage + uploadKeys.value1, uploadFileData, batch);
                        hiddenValues uploadVerify = verifyUpload(uploadFile);

                        //now that we uploaded the file check if there was an error
                        if (uploadVerify.success)
                        {
                            output.success = true;
                            output.id = uploadVerify.value1;
                            output.count = uploadVerify.value2;
                            output.result = "Uploaded transaction details successfuly";

                            output.debug = uploadFile;
                        }
                        else
                        {
                            output.success = false;
                            output.debug = uploadFile;
                            output.result = postUploadError(uploadFile);
                        }
                    }
                    else
                    {
                        output.success = false;
                        output.debug = upPage;
                        output.result = "Failue finding upload keys";
                    }
                }
                else
                {
                    output.success = false;
                    output.debug = logonPost;
                    output.result = "Failure logging in";
                }
            }
            else
            {
                output.success = false;
                output.debug = logonGet;
                output.result = "Failure finding login keys";
            }

            return output;
        }
    }
}
