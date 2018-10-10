using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanRespond.classes
{
    public class Responses
    {
        public List<Response> ResponseList;

        // Return Response object with associated Title
        public Response GetResponse(string title)
        {
            foreach (Response response in ResponseList)
            {
                if (response.Title.Equals(title))
                {
                    return response;
                }
            }

            // return null if not found
            return null;
        }
    }

    public class Response
    {
        private string title;
        private string content;

        public string Title { get => title; set => title = value; }
        public string Content { get => content; set => content = value; }
    }
}
