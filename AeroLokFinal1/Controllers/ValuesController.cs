using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Swashbuckle.Swagger.Annotations;

namespace AeroLokFinal1.Controllers
{
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
            if (image.Width > 600 && image.Height > 600)
            {
                data = CreateImageThumbnail(bytes);
            }
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

        public byte[] CreateImageThumbnail(byte[] image, int width = 600, int height = 600)
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
            var client = new RestClient("https://ussouthcentral.services.azureml.net/workspaces/f357977d76974a8f87368f838ae1e4c0/services/ce49e3a9e4204409948b96b26c1a5e77/execute?api-version=2.0&details=true");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Postman-Token", "dbb06f9a-22a3-4b53-95f2-d6b7ce8be573");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Authorization", "Bearer /nuWyA5CtREB3YjtewYHSNHsxugVqDuhQjwdf8COePcmEW1SeodVM2NZeUNKgvTCbkzqgdCvmH+OgDu39MRe4g==");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined", "{\r\n  \"Inputs\": {\r\n    \"input1\": {\r\n      \"ColumnNames\": [\r\n        \"Col1\"\r\n      ],\r\n      \"Values\": [\r\n        [\r\n          \"" + path + "\"\r\n        ],\r\n        [\r\n          \"value\"\r\n        ]\r\n      ]\r\n    }\r\n  },\r\n  \"GlobalParameters\": {}\r\n}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var data= response.Content;
            var jo = JObject.Parse(data);
            var res = jo.ToString();
            dynamic stuff = JsonConvert.DeserializeObject(res);
            var data2 = stuff.Results.output1.value.Values[0][0];
            return data2; 
            

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
