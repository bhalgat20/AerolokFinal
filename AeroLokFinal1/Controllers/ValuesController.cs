using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Swashbuckle.Swagger.Annotations;

namespace AeroLokFinal1.Controllers
{
    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }

    public class ValuesController : ApiController
    {
        // GET api/values
        
        [System.Web.Http.HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        

        [Route("api/values")]
        [HttpPost]
        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public string SendImage([FromBody]string value)
        {
            var splitpath = value.Split(',');
            byte[] bytes = Convert.FromBase64String(splitpath[1]);
            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }
            var data = bytes;
            var sizeConstant = 300;
            if (image.Width > sizeConstant || image.Height > sizeConstant)
            {
                var width = sizeConstant;
                var height = sizeConstant;
                if(image.Width < sizeConstant)
                {
                    width = image.Width;
                }
                if(image.Height < sizeConstant)
                {
                    height = image.Height;
                }
                data = CreateImageThumbnail(bytes, 318,300);
            }
            //data = CreateImageThumbnail(bytes);
            //Image image;
            //using (MemoryStream ms = new MemoryStream(bytes))
            //{
            //    image = Image.FromStream(ms);
            //}

            //var yourImage = resizeImage(image, new Size(256, 256));
            var res = Convert.ToBase64String(data);

            //var result=  InvokeRequestResponseService(res).Result;
            return MLService(res);
        }

        public byte[] CreateImageThumbnail(byte[] image, int width = 300, int height = 300)
        {
            using (var stream = new System.IO.MemoryStream(image))
            {
                var img = Image.FromStream(stream);
                var thumbnail = img.GetThumbnailImage(width, height, () => false, IntPtr.Zero);

                using (var thumbStream = new System.IO.MemoryStream())
                {
                    thumbnail.Save(thumbStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return thumbStream.GetBuffer();
                }
            }
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }


        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        private string MLService(string path)
        {
            //var client = new RestClient("https://ussouthcentral.services.azureml.net/workspaces/f357977d76974a8f87368f838ae1e4c0/services/f5b3a9b6ea4949c88770f20de5f331e2/execute?api-version=2.0&details=true");
            //var request = new RestRequest(Method.POST);
            //request.AddHeader("Postman-Token", "dbb06f9a-22a3-4b53-95f2-d6b7ce8be573");
            //request.AddHeader("Cache-Control", "no-cache");
            //request.AddHeader("Authorization", "Bearer FquTrpPDoODSu8XCDCYh4fFjvuiHokw/nn8piAAiIXse6kZ286dKujlxsxwNMIFyluqB2p0tZQqL2eyoRNbOkg==");
            //request.AddHeader("Content-Type", "application/json");
            //// request.AddParameter("undefined", "{\r\n  \"Inputs\": {\r\n    \"input1\": {\r\n      \"ColumnNames\": [\r\n        \"Col1\"\r\n      ],\r\n      \"Values\": [\r\n        [\r\n          \"" + path + "\"\r\n        ],\r\n        [\r\n          \"value\"\r\n        ]\r\n      ]\r\n    }\r\n  },\r\n  \"GlobalParameters\": {}\r\n}", ParameterType.RequestBody);
            //request.AddParameter("undefined", "{\r\n  \"Inputs\": {\r\n    \"input1\": {\r\n      \"ColumnNames\": [\r\n        \"Col1\"\r\n      ],\r\n      \"Values\": [\r\n        [\r\n          \"" + path + "\"\r\n        ],\r\n        [\r\n          \"value\"\r\n        ]\r\n      ]\r\n    }\r\n  },\r\n  \"GlobalParameters\": {}\r\n}", ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            try
            {
                var client = new RestClient("https://ussouthcentral.services.azureml.net/workspaces/f357977d76974a8f87368f838ae1e4c0/services/f5b3a9b6ea4949c88770f20de5f331e2/execute?api-version=2.0&details=true");
                var request = new RestRequest(Method.POST);
                //request.AddHeader("Postman-Token", "7e4c4b19-917d-4211-b44f-eb8fdd5e0cbf");
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Authorization", "Bearer FquTrpPDoODSu8XCDCYh4fFjvuiHokw/nn8piAAiIXse6kZ286dKujlxsxwNMIFyluqB2p0tZQqL2eyoRNbOkg==");

                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("undefined", "{\r\n  \"Inputs\": {\r\n    \"input1\": {\r\n      \"ColumnNames\": [\r\n        \"Col1\"\r\n      ],\r\n      \"Values\": [\r\n        [\r\n          \"" + path + "\"\r\n        ],\r\n        [\r\n          \"value\"\r\n        ]\r\n      ]\r\n    }\r\n  },\r\n  \"GlobalParameters\": {}\r\n}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var data = response.Content;
                    var jo = JObject.Parse(data);
                    var res = jo.ToString();
                    dynamic stuff = JsonConvert.DeserializeObject(res);
                    var data2 = stuff.Results.output1.value.Values[0][1];
                    return data2;
                }
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    return response.Content;
                }
                else
                {
                    return response.Content;
                }
            }
            catch(Exception e)
            {
                return e.Message;
            }
            

            //var res = InvokeRequestResponseService(path).Result;
            //return res;
        }

        static async Task<string> InvokeRequestResponseService(string path)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {

                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"Col1"},
                                Values = new string[,] {  { path },  { "value" },  }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                const string apiKey = "FquTrpPDoODSu8XCDCYh4fFjvuiHokw/nn8piAAiIXse6kZ286dKujlxsxwNMIFyluqB2p0tZQqL2eyoRNbOkg=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/f357977d76974a8f87368f838ae1e4c0/services/f5b3a9b6ea4949c88770f20de5f331e2/execute?api-version=2.0&details=true");

                // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)


                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Result: {0}", result);
                    return result;
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                    return responseContent;
                }
            }
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }
    }
}
